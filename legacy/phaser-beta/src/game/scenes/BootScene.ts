import Phaser from 'phaser';
import { generateTextures } from '../systems/textures';
import { getYandexSdk } from '../yandex/sdk';

export class BootScene extends Phaser.Scene {
  constructor() {
    super('Boot');
  }

  async create(): Promise<void> {
    generateTextures(this);
    const sdk = await getYandexSdk();
    sdk.ready();
    this.scene.start('Menu');
  }
}
