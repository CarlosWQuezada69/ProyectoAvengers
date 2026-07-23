-- =========================================================
-- Esquema de base de datos — Plataforma de catálogo + admin
-- PostgreSQL
-- =========================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto"; -- para gen_random_uuid()

-- =========================================================
-- 1. IDENTIDAD Y ACCESO
-- =========================================================

CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name      VARCHAR(100) NOT NULL,
    last_name       VARCHAR(100) NOT NULL,
    email           VARCHAR(255) NOT NULL UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    phone           VARCHAR(30),
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ,
    last_login_at   TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);

CREATE TABLE roles (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(50) NOT NULL UNIQUE,
    description TEXT
);

CREATE TABLE permissions (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code        VARCHAR(100) NOT NULL UNIQUE, -- ej. 'products.create'
    module      VARCHAR(50) NOT NULL,
    action      VARCHAR(50) NOT NULL,
    description TEXT
);

CREATE TABLE role_permissions (
    role_id       UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE user_roles (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE refresh_tokens (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id       UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token         VARCHAR(500) NOT NULL,
    expires_at    TIMESTAMPTZ NOT NULL,
    revoked_at    TIMESTAMPTZ,
    created_by_ip VARCHAR(50),
    created_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX idx_refresh_tokens_user ON refresh_tokens(user_id);

CREATE TABLE password_reset_tokens (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token      VARCHAR(255) NOT NULL UNIQUE,
    expires_at TIMESTAMPTZ NOT NULL,
    used_at    TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE email_change_requests (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id       UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    new_email     VARCHAR(255) NOT NULL,
    token         VARCHAR(255) NOT NULL UNIQUE,
    expires_at    TIMESTAMPTZ NOT NULL,
    confirmed_at  TIMESTAMPTZ,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- =========================================================
-- 2. CATÁLOGO
-- =========================================================

CREATE TABLE categories (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    parent_category_id  UUID REFERENCES categories(id) ON DELETE SET NULL,
    name                VARCHAR(150) NOT NULL,
    slug                VARCHAR(150) NOT NULL UNIQUE,
    description         TEXT,
    image_url           TEXT,
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    display_order       INT NOT NULL DEFAULT 0
);

CREATE TABLE products (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    sku                VARCHAR(50) NOT NULL UNIQUE,
    name               VARCHAR(200) NOT NULL,
    slug               VARCHAR(200) NOT NULL UNIQUE,
    description        TEXT,
    price              NUMERIC(12,2) NOT NULL,
    compare_at_price   NUMERIC(12,2),
    stock              INT NOT NULL DEFAULT 0,
    category_id        UUID REFERENCES categories(id) ON DELETE SET NULL,
    is_active          BOOLEAN NOT NULL DEFAULT TRUE,
    is_featured        BOOLEAN NOT NULL DEFAULT FALSE,
    created_by_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    row_version        BYTEA,           -- concurrencia optimista
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at         TIMESTAMPTZ,
    deleted_at         TIMESTAMPTZ      -- soft delete
);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_active   ON products(is_active) WHERE deleted_at IS NULL;

CREATE TABLE product_images (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id    UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    url           TEXT NOT NULL,
    alt_text      VARCHAR(200),
    display_order INT NOT NULL DEFAULT 0,
    is_primary    BOOLEAN NOT NULL DEFAULT FALSE
);
CREATE INDEX idx_product_images_product ON product_images(product_id);

CREATE TABLE product_restrictions (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id       UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    restriction_type VARCHAR(50) NOT NULL CHECK (restriction_type IN (
        'AGE_MIN', 'PURCHASE_LIMIT_USER', 'PURCHASE_LIMIT_ORDER',
        'AVAILABILITY_WINDOW', 'GEOGRAPHIC', 'LIMITED_STOCK'
    )),
    config     JSONB NOT NULL DEFAULT '{}'::jsonb, -- parámetros según restriction_type
    starts_at  TIMESTAMPTZ,
    ends_at    TIMESTAMPTZ,
    is_active  BOOLEAN NOT NULL DEFAULT TRUE
);
CREATE INDEX idx_product_restrictions_product   ON product_restrictions(product_id);
CREATE INDEX idx_product_restrictions_config_gin ON product_restrictions USING GIN (config);

CREATE TABLE product_stats_daily (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    product_id UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    date       DATE NOT NULL,
    views      INT NOT NULL DEFAULT 0,
    purchases  INT NOT NULL DEFAULT 0,
    UNIQUE (product_id, date)
);

-- =========================================================
-- 3. CONTENIDO DEL SITIO
-- =========================================================

CREATE TABLE slider_items (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title              VARCHAR(200) NOT NULL,
    subtitle           VARCHAR(300),
    image_url          TEXT NOT NULL,
    link_url           TEXT,
    display_order      INT NOT NULL DEFAULT 0,
    starts_at          TIMESTAMPTZ,
    ends_at            TIMESTAMPTZ,
    is_active          BOOLEAN NOT NULL DEFAULT TRUE,
    created_by_user_id UUID REFERENCES users(id) ON DELETE SET NULL,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE site_settings (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    key                VARCHAR(100) NOT NULL UNIQUE, -- logo_url, business_name, copyright_text, contact_email, contact_phone, social_links...
    value              TEXT,
    updated_at         TIMESTAMPTZ,
    updated_by_user_id UUID REFERENCES users(id) ON DELETE SET NULL
);

-- =========================================================
-- 4. AUDITORÍA
-- =========================================================

CREATE TABLE audit_logs (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID REFERENCES users(id) ON DELETE SET NULL,
    action      VARCHAR(50) NOT NULL,      -- CREATE, UPDATE, DELETE
    entity_name VARCHAR(100) NOT NULL,     -- 'Product', 'User', etc.
    entity_id   UUID,
    changes     JSONB,
    ip_address  VARCHAR(50),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_name, entity_id);
CREATE INDEX idx_audit_logs_user   ON audit_logs(user_id);

-- =========================================================
-- 5. SEED — catálogo mínimo de permisos y rol SuperAdmin
-- =========================================================

INSERT INTO permissions (code, module, action, description) VALUES
    ('products.view',    'products',  'view',   'Ver productos en el panel admin'),
    ('products.create',  'products',  'create', 'Crear productos'),
    ('products.update',  'products',  'update', 'Editar productos'),
    ('products.delete',  'products',  'delete', 'Eliminar productos'),
    ('products.manage-restrictions', 'products', 'manage', 'Gestionar restricciones de producto'),
    ('categories.create', 'categories', 'create', 'Crear categorías'),
    ('categories.update', 'categories', 'update', 'Editar categorías'),
    ('categories.delete', 'categories', 'delete', 'Eliminar categorías'),
    ('slider.view',   'slider',   'view',   'Ver ítems del carrusel'),
    ('slider.create', 'slider',   'create', 'Crear ítems del carrusel'),
    ('slider.update', 'slider',   'update', 'Editar ítems del carrusel'),
    ('slider.delete', 'slider',   'delete', 'Eliminar ítems del carrusel'),
    ('settings.view',   'settings', 'view',   'Ver configuración del sitio'),
    ('settings.update', 'settings', 'update', 'Editar configuración del sitio'),
    ('users.view',          'users', 'view',   'Ver usuarios'),
    ('users.create',        'users', 'create', 'Crear usuarios'),
    ('users.update',        'users', 'update', 'Editar usuarios'),
    ('users.delete',        'users', 'delete', 'Desactivar usuarios'),
    ('users.manage-roles',  'users', 'manage', 'Asignar roles a usuarios'),
    ('roles.view',   'roles', 'view',   'Ver roles y permisos'),
    ('roles.create', 'roles', 'create', 'Crear roles'),
    ('roles.update', 'roles', 'update', 'Editar roles y sus permisos'),
    ('roles.delete', 'roles', 'delete', 'Eliminar roles'),
    ('stats.view', 'stats', 'view', 'Ver estadísticas'),
    ('audit.view', 'audit', 'view', 'Ver bitácora de auditoría');

INSERT INTO roles (name, description) VALUES
    ('SuperAdmin', 'Acceso total al sistema');

-- Asigna todos los permisos existentes al rol SuperAdmin
INSERT INTO role_permissions (role_id, permission_id)
SELECT (SELECT id FROM roles WHERE name = 'SuperAdmin'), id FROM permissions;

-- Nota: el usuario SuperAdmin inicial (email/contraseña) debe crearse desde
-- la aplicación (nunca hardcodeado en el script), usando variables de entorno
-- en el primer arranque.
