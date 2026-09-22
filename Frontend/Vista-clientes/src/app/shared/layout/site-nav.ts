import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLinkActive, RouterLink } from '@angular/router';

import { NAV_LINKS } from './nav-links';

@Component({
  selector: 'app-site-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './site-nav.html',
  styleUrls: ['./site-nav.scss']
})
export class SiteNavComponent {
  readonly navLinks = NAV_LINKS;
}