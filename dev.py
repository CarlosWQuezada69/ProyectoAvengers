#!/usr/bin/env python3
"""Levanta el proyecto ProyectoAvengers (backend + frontend) y crea un usuario admin de login.

Uso:
    python3 dev.py                          # todo: postgres + backend + frontend
    python3 dev.py --email admin@correo.com --password MiClave123!
    python3 dev.py --backend-only           # solo API (sin frontend)
    python3 dev.py --frontend-only          # solo frontend (asume API arriba)
    python3 dev.py --no-migrations          # no aplica migraciones

Ctrl+C detiene ambos servidores.
"""

import argparse
import json
import os
import secrets
import shutil
import signal
import subprocess
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent
BACKEND_DIR = ROOT / "Backend"
API_DIR = BACKEND_DIR / "src" / "Api"
INFRA_DIR = BACKEND_DIR / "src" / "Infrastructure"
FRONTEND_DIR = ROOT / "Frontend" / "Panel-administrativo"

API_URL = "http://localhost:5167"
FRONTEND_URL = "http://localhost:4200"
DB_CONNECTION = "Host=localhost;Database=proyecto_avengers;Username=postgres;Password=postgres"

CYAN = "\033[96m"
GREEN = "\033[92m"
YELLOW = "\033[93m"
RED = "\033[91m"
BOLD = "\033[1m"
RESET = "\033[0m"


def info(msg: str) -> None:
    print(f"{CYAN}[dev]{RESET} {msg}", flush=True)


def ok(msg: str) -> None:
    print(f"{GREEN}[dev]{RESET} {msg}", flush=True)


def warn(msg: str) -> None:
    print(f"{YELLOW}[dev]{RESET} {msg}", flush=True)


def fail(msg: str) -> None:
    print(f"{RED}[dev]{RESET} {msg}", flush=True)
    sys.exit(1)


def check_tool(name: str, hint: str = "") -> None:
    if shutil.which(name) is None:
        fail(f"No se encontró '{name}'. {hint}")


def wait_for(url: str, timeout: int = 180, label: str = "") -> None:
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            with urllib.request.urlopen(url, timeout=3) as resp:
                if resp.status < 500:
                    ok(f"{label or url} está respondiendo ({resp.status})")
                    return
        except (urllib.error.URLError, ConnectionError, OSError):
            time.sleep(2)
    fail(f"{label or url} no respondió en {timeout}s.")


def run(cmd: list[str], cwd: Path | None = None, env: dict | None = None,
        check: bool = False, capture: bool = False):
    full_env = os.environ.copy()
    if env:
        full_env.update(env)
    result = subprocess.run(cmd, cwd=cwd, env=full_env, check=check,
                            capture_output=capture, text=True)
    return result


def postgres_running() -> bool:
    if shutil.which("pg_isready") is None:
        return False
    return run(["pg_isready", "-h", "localhost", "-p", "5432", "-q"]).returncode == 0


def port_responding(url: str, timeout: float = 3) -> bool:
    try:
        with urllib.request.urlopen(url, timeout=timeout) as resp:
            return resp.status < 500
    except (urllib.error.URLError, ConnectionError, OSError):
        return False


def start_postgres() -> None:
    if postgres_running():
        ok("PostgreSQL ya está corriendo")
        return

    warn("PostgreSQL no está activo. Intentando arrancarlo...")
    start_ok = False
    for data_dir in (Path("/tmp/opencode/pgdata"), Path("/var/lib/postgres/data")):
        if data_dir.exists() and shutil.which("pg_ctl"):
            info(f"Probando arranque local con data dir {data_dir}...")
            result = run(["pg_ctl", "-D", str(data_dir), "-l", "/tmp/pg_proyectoavengers.log", "start"])
            if result.returncode == 0 or postgres_running():
                start_ok = True
                break

    if not start_ok and shutil.which("systemctl"):
        result = run(["systemctl", "is-enabled", "postgresql"])
        if result.returncode != 0:
            start_ok = run(["sudo", "systemctl", "enable", "--now", "postgresql"]).returncode == 0
        else:
            start_ok = run(["sudo", "systemctl", "start", "postgresql"]).returncode == 0

    if start_ok:
        deadline = time.time() + 60
        while time.time() < deadline:
            if postgres_running():
                ok("PostgreSQL arrancado")
                return
            time.sleep(2)

    fail(
        "No se pudo arrancar PostgreSQL. "
        "Inícialo manualmente (ej: 'sudo systemctl start postgresql') y vuelve a ejecutar el script."
    )


def apply_migrations(no_migrations: bool) -> None:
    if no_migrations:
        warn("Omitiendo migraciones (--no-migrations)")
        return

    tools_dir = Path.home() / ".dotnet" / "tools"
    if tools_dir.exists():
        os.environ["PATH"] = str(tools_dir) + os.pathsep + os.environ.get("PATH", "")

    if shutil.which("dotnet-ef") is None:
        warn("No se encontró dotnet-ef. Instalándolo...")
        run(["dotnet", "tool", "install", "--global", "dotnet-ef"], check=True)
        os.environ["PATH"] = str(tools_dir) + os.pathsep + os.environ.get("PATH", "")

    info("Aplicando migraciones de base de datos...")
    result = run(
        ["dotnet", "ef", "database", "update",
         "--project", str(INFRA_DIR), "--startup-project", str(API_DIR)],
        cwd=BACKEND_DIR, env={"CONNECTIONSTRINGS__DEFAULT": DB_CONNECTION}
    )
    if result.returncode != 0:
        fail("Fallaron las migraciones. Revisa que la BD 'proyecto_avengers' exista y el usuario postgres tenga acceso.")
    ok("Migraciones aplicadas")


def ensure_jwt_secret() -> None:
    result = run(["dotnet", "user-secrets", "list"], cwd=API_DIR, capture=True)
    if "Jwt:Secret" in result.stdout:
        return

    secret = secrets.token_urlsafe(48)
    info("Configurando Jwt:Secret en User Secrets (solo local)...")
    run(["dotnet", "user-secrets", "set", "Jwt:Secret", secret], cwd=API_DIR, check=True)


def start_backend(email: str, password: str) -> subprocess.Popen | None:
    if port_responding(f"{API_URL}/swagger/v1/swagger.json"):
        ok("Backend ya está corriendo en " + API_URL)
        return None

    info("Arrancando backend (dotnet run)...")
    proc = subprocess.Popen(
        ["dotnet", "run", "--project", "src/Api"],
        cwd=BACKEND_DIR,
        env={
            **os.environ,
            "ADMIN_EMAIL": email,
            "ADMIN_PASSWORD": password,
            "ASPNETCORE_ENVIRONMENT": "Development",
            "CONNECTIONSTRINGS__DEFAULT": DB_CONNECTION,
        },
    )
    wait_for(f"{API_URL}/swagger/v1/swagger.json", label="Backend")
    return proc


def start_frontend() -> subprocess.Popen | None:
    if port_responding(FRONTEND_URL):
        ok("Frontend ya está corriendo en " + FRONTEND_URL)
        return None

    if not (FRONTEND_DIR / "node_modules").exists():
        info("Instalando dependencias del frontend (npm install)...")
        run(["npm", "install"], cwd=FRONTEND_DIR, check=True)

    info("Arrancando frontend (ng serve)...")
    proc = subprocess.Popen(["npm", "start"], cwd=FRONTEND_DIR)
    wait_for(FRONTEND_URL, timeout=240, label="Frontend")
    return proc


def verify_login(email: str, password: str) -> None:
    payload = json.dumps({"email": email, "password": password}).encode()
    request = urllib.request.Request(
        f"{API_URL}/api/v1/auth/login", data=payload,
        headers={"Content-Type": "application/json"}, method="POST",
    )
    try:
        with urllib.request.urlopen(request, timeout=10) as resp:
            ok(f"Login verificado con {email}")
    except urllib.error.HTTPError as e:
        if e.code == 401:
            warn(f"Login rechazado: el usuario {email} ya existía con otra contraseña. "
                 "Bórralo de la BD o usa otro email.")
        elif e.code == 400:
            warn(f"Login rechazado con {email}/{password} (respuesta {e.code}). Revisa los requisitos de contraseña.")
        else:
            warn(f"Respuesta inesperada del login: {e.code}")
    except Exception:
        warn("No se pudo verificar el login ahora; revisa la API.")


def main() -> None:
    parser = argparse.ArgumentParser(description="Levanta backend y frontend de ProyectoAvengers")
    parser.add_argument("--email", default="admin@example.com", help="Email del usuario admin (default: admin@example.com)")
    parser.add_argument("--password", default="Admin123!", help="Contraseña del usuario admin (default: Admin123!)")
    parser.add_argument("--backend-only", action="store_true", help="Solo levantar la API")
    parser.add_argument("--frontend-only", action="store_true", help="Solo levantar el frontend")
    parser.add_argument("--no-migrations", action="store_true", help="No aplicar migraciones")
    args = parser.parse_args()

    print(f"\n{BOLD}ProyectoAvengers — entorno de desarrollo{RESET}\n")

    check_tool("dotnet", "Instala el SDK de .NET 8.")
    if not args.frontend_only:
        check_tool("psql", "Instala PostgreSQL 16+.")

    if args.frontend_only:
        check_tool("npm", "Instala Node.js 20+.")
        frontend = start_frontend()
    else:
        start_postgres()
        apply_migrations(args.no_migrations)
        ensure_jwt_secret()

        backend = start_backend(args.email, args.password)

        if args.backend_only:
            print(f"\n{GREEN}API lista: {API_URL} (Swagger: {API_URL}/swagger){RESET}")
            print(f"Credenciales: {args.email} / {args.password}\n")
            if backend:
                signal.signal(signal.SIGINT, lambda *_: (backend.terminate(), sys.exit(0)))
                signal.pause()
            return

        frontend = start_frontend()

    verify_login(args.email, args.password)

    print(f"""
{BOLD}Todo listo.{RESET}

  Panel admin : {FRONTEND_URL}
  API         : {API_URL}  (Swagger: {API_URL}/swagger)
  Usuario     : {args.email}
  Contraseña  : {args.password}

{GREEN}Presiona Ctrl+C para detener ambos servidores.{RESET}""")

    try:
        signal.pause()
    except KeyboardInterrupt:
        pass
    finally:
        for proc in (frontend, backend):
            if proc is None:
                continue
            proc.terminate()
            try:
                proc.wait(timeout=10)
            except subprocess.TimeoutExpired:
                proc.kill()
        ok("Servidores detenidos. ¡Hasta pronto!")


if __name__ == "__main__":
    main()