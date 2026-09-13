import Phaser from 'phaser';
import { BESTIARY } from '../data/content';
import { loadSave } from '../data/save';
import { Sfx } from '../systems/audio';

export class MenuScene extends Phaser.Scene {
  constructor() {
    super('Menu');
  }

  create(): void {
    const { width, height } = this.scale;
    const save = loadSave();

    this.add.rectangle(0, 0, width, height, 0x0a1410).setOrigin(0);
    const bg = this.add.tileSprite(0, 0, width, height, 'tex_ground').setOrigin(0).setAlpha(0.45);
    this.tweens.add({
      targets: bg,
      tilePositionX: 64,
      tilePositionY: 32,
      duration: 8000,
      repeat: -1,
    });

    for (let i = 0; i < 10; i += 1) {
      const bob = this.add
        .image(
          Phaser.Math.Between(30, width - 30),
          Phaser.Math.Between(120, height - 120),
          i % 3 === 0 ? 'tex_bobyl_hard' : 'tex_bobyl',
        )
        .setAlpha(0.22)
        .setScale(Phaser.Math.FloatBetween(0.8, 1.4));
      this.tweens.add({
        targets: bob,
        y: bob.y - 12,
        duration: Phaser.Math.Between(1200, 2200),
        yoyo: true,
        repeat: -1,
      });
    }

    this.add
      .text(width / 2, 48, 'ЗАЗЕРКАЛЬЕ', {
        fontFamily: 'Courier New',
        fontSize: `${Math.min(52, Math.floor(width * 0.12))}px`,
        color: '#e8c76b',
        stroke: '#2a1a08',
        strokeThickness: 7,
      })
      .setOrigin(0.5);

    this.add
      .text(width / 2, 96, 'СУМЕРЕЧНАЯ РОЩА · сессии 2–5 мин · комбо-экшен', {
        fontFamily: 'Courier New',
        fontSize: '13px',
        color: '#8fb89a',
      })
      .setOrigin(0.5);

    this.add.image(width / 2 - 80, 175, 'tex_player').setScale(1.55);
    this.add.image(width / 2 + 10, 185, 'tex_bobyl').setScale(1.35);
    this.add.image(width / 2 + 75, 188, 'tex_jvachnik').setScale(1.25).setTint(0xffaabb);
    this.add.image(width / 2 + 40, 150, 'tex_pacanoid').setScale(1.1);

    const unlocked = Object.values(save.bestiary).filter(Boolean).length;
    this.add
      .text(
        width / 2,
        height - 230,
        `Кукичи: ${save.kukichi}   Пакичи: ${save.pakichi}\nРекорд очков: ${save.bestScore}   Бестиарий: ${unlocked}/${BESTIARY.length}\nЗаходов: ${save.matchesPlayed}`,
        { fontFamily: 'Courier New', fontSize: '14px', color: '#c9e8b8', align: 'center' },
      )
      .setOrigin(0.5);

    this.add
      .text(
        width / 2,
        height - 165,
        'WASD — ходьба · ПРОБЕЛ — КОК · SHIFT — РЫВОК\nСобирай следы, копи комбо, переживи ночь жвачников',
        {
          fontFamily: 'Courier New',
          fontSize: '12px',
          color: '#9bb8a4',
          align: 'center',
        },
      )
      .setOrigin(0.5);

    this.makeButton(width / 2, height - 105, 'ВОЙТИ В СУМЕРЕЧНУЮ РОЩУ', () => {
      Sfx.ui();
      this.scene.start('Match');
    }, 0x3d6b4a);
    this.makeButton(width / 2, height - 52, 'БЕСТИАРИЙ', () => {
      Sfx.ui();
      this.scene.start('Bestiary');
    }, 0x2a4a3a);
  }

  private makeButton(x: number, y: number, label: string, onClick: () => void, color: number): void {
    const bg = this.add
      .rectangle(x, y, Math.min(380, this.scale.width - 36), 42, color, 0.95)
      .setInteractive({ useHandCursor: true });
    this.add
      .text(x, y, label, { fontFamily: 'Courier New', fontSize: '15px', color: '#f2ffe8' })
      .setOrigin(0.5);
    bg.on('pointerover', () => bg.setFillStyle(color + 0x141414, 1));
    bg.on('pointerout', () => bg.setFillStyle(color, 0.95));
    bg.on('pointerup', onClick);
  }
}
