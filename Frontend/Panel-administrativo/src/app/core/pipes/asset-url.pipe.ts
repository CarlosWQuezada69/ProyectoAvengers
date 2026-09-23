import { Pipe, type PipeTransform } from '@angular/core';

import { toAssetUrl } from '../utils/asset-url';

@Pipe({
  name: 'assetUrl',
  standalone: true,
})
export class AssetUrlPipe implements PipeTransform {
  transform(value: string | null | undefined): string | null {
    return toAssetUrl(value);
  }
}