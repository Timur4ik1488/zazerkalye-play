import Phaser from 'phaser';
import { BESTIARY } from '../data/content';
import { loadSave } from '../data/save';

export class BestiaryScene extends Phaser.Scene {
  constructor() {
    super('Bestiary');
  }

  create(): void {
    const { width, height } = this.scale;
    const save = loadSave();
    this.add.rectangle(0, 0, width, height, 0x0b120f).setOrigin(0);
    this.add
      .text(width / 2, 28, 'БЕСТИАРИЙ ЗАЗЕРКАЛЬЯ', {
        fontFamily: 'Courier New',
        fontSize: '20px',
        color: '#e8c76b',
      })
      .setOrigin(0.5);

    const unlocked = Object.values(save.bestiary).filter(Boolean).length;
    this.add
      .text(width / 2, 52, `Открыто ${unlocked} из ${BESTIARY.length}`, {
        fontFamily: 'Courier New',
        fontSize: '12px',
        color: '#8fb89a',
      })
      .setOrigin(0.5);

    const panelH = height - 110;
    const maskShape = this.add.graphics().fillRect(0, 70, width, panelH);
    const mask = maskShape.createGeometryMask();
    maskShape.setVisible(false);

    const container = this.add.container(0, 70);
    container.setMask(mask);

    let y = 0;
    BESTIARY.forEach((entry) => {
      const open = !!save.bestiary[entry.id];
      const color =
        entry.rarity === 'secret' ? '#ff9ad5' : entry.rarity === 'rare' ? '#e8c76b' : '#c9e8b8';
      container.add(
        this.add.text(20, y, `${open ? entry.name : '???'}  [${entry.rarity}]`, {
          fontFamily: 'Courier New',
          fontSize: '14px',
          color,
        }),
      );
      container.add(
        this.add.text(20, y + 18, open ? entry.blurb : 'Ещё не встречали в Сумеречной роще.', {
          fontFamily: 'Courier New',
          fontSize: '12px',
          color: '#8fb89a',
          wordWrap: { width: width - 40 },
        }),
      );
      y += 52;
    });

    const maxScroll = Math.max(0, y - panelH + 8);
    this.input.on('wheel', (_p: unknown, _g: unknown, _dx: number, dy: number) => {
      container.y = Phaser.Math.Clamp(container.y - dy * 0.4, 70 - maxScroll, 70);
    });

    let dragY = 0;
    this.input.on('pointerdown', (p: Phaser.Input.Pointer) => {
      dragY = p.y;
    });
    this.input.on('pointermove', (p: Phaser.Input.Pointer) => {
      if (!p.isDown || p.y > height - 50) return;
      const dy = p.y - dragY;
      dragY = p.y;
      container.y = Phaser.Math.Clamp(container.y + dy, 70 - maxScroll, 70);
    });

    const back = this.add
      .rectangle(width / 2, height - 34, 200, 36, 0x2a4a3a)
      .setInteractive({ useHandCursor: true })
      .setDepth(10);
    this.add
      .text(width / 2, height - 34, 'НАЗАД', {
        fontFamily: 'Courier New',
        fontSize: '14px',
        color: '#f2ffe8',
      })
      .setOrigin(0.5)
      .setDepth(11);
    back.on('pointerup', () => this.scene.start('Menu'));
  }
}
