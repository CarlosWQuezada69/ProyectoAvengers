import { PublicSettings } from '../models/settings';

export const DEFAULT_WHATSAPP_MESSAGE =
  'Hola, me gustaría recibir información sobre sus productos.';

export function resolveWhatsAppNumber(settings: PublicSettings | null | undefined): string | null {
  const raw = settings?.contact_whatsapp || settings?.contact_phone;
  if (!raw) return null;
  const digits = raw.replace(/\D/g, '');
  return digits ? digits : null;
}

export function buildWhatsAppLink(phone: string, message: string): string {
  const digits = phone.replace(/\D/g, '');
  const encoded = encodeURIComponent(message);
  return `https://wa.me/${digits}?text=${encoded}`;
}