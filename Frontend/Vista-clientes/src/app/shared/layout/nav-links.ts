export interface NavLink {
  label: string;
  href: string;
  query?: Record<string, string>;
  fragment?: string;
}

export const NAV_LINKS: NavLink[] = [
  { label: 'Inicio', href: '/' },
  { label: 'Colecciones', href: '/productos' },
  { label: 'Rellenos', href: '/productos', query: { q: 'rellenos' } },
  { label: 'Productos', href: '/productos' },
  { label: 'Acerca de', href: '/acerca' },
  { label: 'Contacto', href: '/', fragment: 'contacto' }
];