export interface SocialLink {
  name: 'facebook' | 'instagram' | 'youtube' | 'whatsapp' | 'tiktok' | 'x' | string;
  label: string;
  url: string;
}

const KNOWN = new Map<string, string>([
  ['instagram', 'Instagram'],
  ['facebook', 'Facebook'],
  ['youtube', 'YouTube'],
  ['whatsapp', 'WhatsApp'],
  ['tiktok', 'TikTok'],
  ['x', 'X']
]);

export function parseSocialLinks(raw: string | null | undefined): SocialLink[] {
  if (!raw) return [];
  const trimmed = raw.trim();
  if (!trimmed) return [];

  // Formato JSON: { "instagram": "https://…", "facebook": "https://…" } o array de objetos
  if (trimmed.startsWith('{') || trimmed.startsWith('[')) {
    try {
      const parsed = JSON.parse(trimmed);
      const entries = Array.isArray(parsed)
        ? parsed.map((item: { name?: string; url?: string; label?: string }) => [
            item.name ?? 'link',
            item.url ?? ''
          ])
        : Object.entries(parsed).filter(([, v]) => typeof v === 'string' && (v as string).length > 0);

      return entries
        .filter(([, url]) => typeof url === 'string' && /^https?:\/\//i.test(url as string))
        .map(([name, url]) => ({
          name: name as string,
          label: KNOWN.get(name.toLowerCase()) ?? name,
          url: url as string
        }));
    } catch {
      return [];
    }
  }

  // Formato plano: nombre=enlace separado por comas o saltos de línea
  return trimmed
    .split(/[\n,]/)
    .map((pair) => pair.trim())
    .filter(Boolean)
    .map((pair) => {
      const [key, url] = pair.split('=');
      return { name: key.trim(), label: KNOWN.get(key.trim().toLowerCase()) ?? key.trim(), url: url.trim() };
    })
    .filter((s) => /^https?:\/\//i.test(s.url));
}