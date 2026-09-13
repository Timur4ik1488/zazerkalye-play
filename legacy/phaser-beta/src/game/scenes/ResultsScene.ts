import Phaser from 'phaser';
import { addKukichi, loadSave, unlockBestiary, writeSave } from '../data/save';
import { Sfx } from '../systems/audio';
import { getYandexSdk } from '../yandex/sdk';
import type { MatchResult } from './MatchScene';

const FATE_LINES = [
  'Брунявая Чуня или Чунявая Бруня? … Правильный ответ — обои.',
  'Жвачники выйдут на болота Сумеречной рощи',
  'Казимир снова уведёт ложки пацаноидов',
  'Дядюшка Фантасмагор ждёт алмазные соления',
  'Акакий Куролесов заберётся слишком высоко',
  'Болотный Скрипач заведёт путников от Спектрального колодца',
  'Поленыч преклонит колено — а Коленыча лучше не встречать',
];

export class ResultsScene extends Phaser.Scene {
  private result!: MatchResult;
  private doubled = false;

  constructor() {
    super('Results');
  }

  init(data: MatchResult): void {
    this.result = data;
    this.doubled = false;
  }

  create(): void {
    const { width, height } = this.scale;
    let save = loadSave();
    save.matchesPlayed += 1;
    save = addKukichi(save, this.result.kukichi);
    if (this.result.perfect) {
      save = unlockBestiary(save, 'fantasmagor');
      save = unlockBestiary(save, 'jvachnik');
    }
    if (this.result.mirrorFound) save = unlockBestiary(save, 'istukanus');
    if (this.result.eyeFound) save = unlockBestiary(save, 'spectral');
    if (this.result.kokked >= 5) save = unlockBestiary(save, 'scripach');
    if (Math.random() < 0.25) save = unlockBestiary(save, 'kazimir');
    if (this.result.score > save.bestScore) save.bestScore = this.result.score;
    writeSave(save);

    this.add.rectangle(0, 0, width, height, 0x0b120f).setOrigin(0);
    for (let i = 0; i < 12; i += 1) {
      this.add
        .image(
          Phaser.Math.Between(20, width - 20),
          Phaser.Math.Between(20, height - 20),
          'tex_spark_green',
        )
        .setScale(Phaser.Math.FloatBetween(1, 3))
        .setAlpha(0.35);
    }

    const title = this.result.perfect
      ? 'ИДЕАЛЬНЫЙ ЗАХОД'
      : this.result.shards >= 3
        ? 'СЛЕДЫ СОБРАНЫ'
        : 'СМЕНА ОБОРВАНА';
    this.add
      .text(width / 2, 36, title, {
        fontFamily: 'Courier New',
        fontSize: '22px',
        color: '#e8c76b',
        stroke: '#2a1a08',
        strokeThickness: 5,
        align: 'center',
        wordWrap: { width: width - 40 },
      })
      .setOrigin(0.5);

    const lines = [
      `ОЧКИ: ${this.result.score}`,
      `Рекорд: ${save.bestScore}`,
      '',
      `Кукичи: ${this.result.kukichi}   (всего ${save.kukichi})`,
      `Коков: ${this.result.kokked}   Следы: ${this.result.shards}/3`,
      `Макс комбо: x${this.result.maxCombo}   Урон: ${this.result.damageTaken}`,
      `Зеркало: ${this.result.mirrorFound ? 'да' : 'нет'}   Колодец: ${this.result.eyeFound ? 'да' : 'нет'}`,
    ];
    this.add
      .text(width / 2, 88, lines.join('\n'), {
        fontFamily: 'Courier New',
        fontSize: '14px',
        color: '#c9e8b8',
        align: 'center',
        lineSpacing: 4,
      })
      .setOrigin(0.5, 0);

    const fate = Phaser.Utils.Array.GetRandom(FATE_LINES);
    writeSave(unlockBestiary(loadSave(), 'istukanus'));
    this.add
      .text(width / 2, height - 236, `Истуканус шепчет:\n«${fate}»`, {
        fontFamily: 'Courier New',
        fontSize: '13px',
        color: '#d7b4ff',
        align: 'center',
        backgroundColor: '#1a1020cc',
        padding: { x: 10, y: 8 },
        wordWrap: { width: width - 48 },
      })
      .setOrigin(0.5);

    this.btn(width / 2, height - 150, '×2 КУКИЧИ ЗА РОЛИК', async () => {
      if (this.doubled) return;
      const sdk = await getYandexSdk();
      this.sound?.pauseAll?.();
      const ok = await sdk.showRewarded();
      this.sound?.resumeAll?.();
      if (ok) {
        this.doubled = true;
        addKukichi(loadSave(), this.result.kukichi);
        Sfx.power();
        this.note('Алмазные соления вовремя. Дядюшка Фантасмагор доволен.');
      } else {
        this.note('Ролик недоступен. Попробуйте позже.');
      }
    }, 0x6b3d3d);

    this.btn(width / 2, height - 95, 'ЕЩЁ ЗАХОД', async () => {
      Sfx.ui();
      const sdk = await getYandexSdk();
      this.sound?.pauseAll?.();
      await sdk.showInterstitial();
      this.sound?.resumeAll?.();
      this.scene.start('Match');
    });

    this.btn(width / 2, height - 45, 'В ХАБ', async () => {
      Sfx.ui();
      const sdk = await getYandexSdk();
      this.sound?.pauseAll?.();
      await sdk.showInterstitial();
      this.sound?.resumeAll?.();
      this.scene.start('Menu');
    }, 0x2a4a3a);
  }

  private note(msg: string): void {
    const t = this.add
      .text(this.scale.width / 2, this.scale.height / 2, msg, {
        fontFamily: 'Courier New',
        fontSize: '14px',
        color: '#ffe9a8',
        backgroundColor: '#000000cc',
        padding: { x: 10, y: 8 },
        wordWrap: { width: this.scale.width - 48 },
        align: 'center',
      })
      .setOrigin(0.5);
    this.time.delayedCall(2200, () => t.destroy());
  }

  private btn(x: number, y: number, label: string, cb: () => void, color = 0x3d6b4a): void {
    const bg = this.add
      .rectangle(x, y, Math.min(340, this.scale.width - 40), 38, color)
      .setInteractive({ useHandCursor: true });
    this.add
      .text(x, y, label, { fontFamily: 'Courier New', fontSize: '14px', color: '#f2ffe8' })
      .setOrigin(0.5);
    bg.on('pointerover', () => bg.setFillStyle(color + 0x151515));
    bg.on('pointerout', () => bg.setFillStyle(color));
    bg.on('pointerup', cb);
  }
}
