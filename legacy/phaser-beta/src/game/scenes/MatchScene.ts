import Phaser from 'phaser';
import { MATCH_SECONDS } from '../data/content';
import { loadSave, unlockBestiary, writeSave } from '../data/save';
import { Sfx } from '../systems/audio';
import { burst, floatText, hitstop, screenPunch } from '../systems/juice';
import { VirtualPad } from '../ui/VirtualPad';

type MobKind = 'bobyl' | 'hard' | 'jvachnik';

type Mob = Phaser.Physics.Arcade.Sprite & {
  hp: number;
  kind: MobKind;
  speed: number;
};

type PowerKind = 'magnet' | 'rage' | 'haste' | 'shield' | 'feast';

export type MatchResult = {
  kukichi: number;
  kokked: number;
  shards: number;
  perfect: boolean;
  mirrorFound: boolean;
  eyeFound: boolean;
  score: number;
  maxCombo: number;
  damageTaken: number;
};

const WORLD_W = 1760;
const WORLD_H = 1320;
const KOK_RANGE = 118;
const DASH_CD = 1.15;
const COMBO_WINDOW = 2.6;

export class MatchScene extends Phaser.Scene {
  private player!: Phaser.Physics.Arcade.Sprite;
  private mobs!: Phaser.Physics.Arcade.Group;
  private shardsGroup!: Phaser.Physics.Arcade.Group;
  private pickups!: Phaser.Physics.Arcade.Group;
  private powers!: Phaser.Physics.Arcade.Group;
  private swamps: Phaser.GameObjects.Image[] = [];
  private hearts: Phaser.GameObjects.Image[] = [];

  private pad!: VirtualPad;
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private wasd!: Record<'W' | 'A' | 'S' | 'D', Phaser.Input.Keyboard.Key>;
  private keyKok!: Phaser.Input.Keyboard.Key;
  private keyDash!: Phaser.Input.Keyboard.Key;

  private hud!: Phaser.GameObjects.Text;
  private goal!: Phaser.GameObjects.Text;
  private toast!: Phaser.GameObjects.Text;
  private comboText!: Phaser.GameObjects.Text;
  private marker!: Phaser.GameObjects.Triangle;
  private nightFx!: Phaser.GameObjects.Rectangle;
  private ground!: Phaser.GameObjects.TileSprite;
  private facing = new Phaser.Math.Vector2(1, 0);

  private timeLeft = MATCH_SECONDS;
  private kukichi = 0;
  private kokked = 0;
  private shards = 0;
  private score = 0;
  private combo = 0;
  private maxCombo = 0;
  private comboTimer = 0;
  private hp = 3;
  private damageTaken = 0;
  private mirrorFound = false;
  private eyeFound = false;
  private ended = false;
  private night = false;
  private wave = 0;

  private invuln = 0;
  private kokCd = 0;
  private dashCd = 0;
  private dashLeft = 0;
  private magnetLeft = 0;
  private rageLeft = 0;
  private hasteLeft = 0;
  private shieldOn = false;

  constructor() {
    super('Match');
  }

  create(): void {
    this.resetState();

    this.physics.world.setBounds(0, 0, WORLD_W, WORLD_H);
    this.cameras.main.setBounds(0, 0, WORLD_W, WORLD_H);
    this.cameras.main.setBackgroundColor('#0c1a14');

    this.buildWorld();
    this.buildPlayer();
    this.buildGroups();
    this.spawnInitialEntities();
    this.bindSecrets();
    this.bindOverlaps();
    this.bindInput();
    this.buildHud();
    this.unlockIntroLore();

    this.time.addEvent({ delay: 1000, loop: true, callback: () => this.tickSecond() });
    this.scale.on('resize', (size: Phaser.Structs.Size) => {
      this.goal.setPosition(size.width / 2, 6).setWordWrapWidth(size.width - 20);
      this.toast.setPosition(size.width / 2, 92).setWordWrapWidth(size.width - 32);
      this.comboText.setPosition(size.width - 12, 48);
      this.nightFx.setSize(size.width, size.height);
    });
  }

  private resetState(): void {
    this.ended = false;
    this.night = false;
    this.timeLeft = MATCH_SECONDS;
    this.kukichi = 0;
    this.kokked = 0;
    this.shards = 0;
    this.score = 0;
    this.combo = 0;
    this.maxCombo = 0;
    this.comboTimer = 0;
    this.hp = 3;
    this.damageTaken = 0;
    this.mirrorFound = false;
    this.eyeFound = false;
    this.invuln = 0;
    this.kokCd = 0;
    this.dashCd = 0;
    this.dashLeft = 0;
    this.magnetLeft = 0;
    this.rageLeft = 0;
    this.hasteLeft = 0;
    this.shieldOn = false;
    this.wave = 0;
    this.swamps = [];
    this.hearts = [];
    this.facing.set(1, 0);
  }

  private buildWorld(): void {
    this.ground = this.add.tileSprite(0, 0, WORLD_W, WORLD_H, 'tex_ground').setOrigin(0).setDepth(-30);

    for (let i = 0; i < 34; i += 1) {
      const x = Phaser.Math.Between(50, WORLD_W - 50);
      const y = Phaser.Math.Between(50, WORLD_H - 50);
      const tree = this.add.image(x, y, 'tex_tree').setDepth(y).setScale(Phaser.Math.FloatBetween(0.85, 1.2));
      this.tweens.add({
        targets: tree,
        angle: { from: -2.5, to: 2.5 },
        duration: Phaser.Math.Between(1800, 3200),
        yoyo: true,
        repeat: -1,
      });
    }

    for (let i = 0; i < 7; i += 1) {
      this.swamps.push(
        this.add
          .image(
            Phaser.Math.Between(200, WORLD_W - 200),
            Phaser.Math.Between(200, WORLD_H - 200),
            'tex_swamp',
          )
          .setDepth(5)
          .setAlpha(0.88),
      );
    }
  }

  private buildPlayer(): void {
    this.player = this.physics.add.sprite(240, 240, 'tex_player');
    this.player.setCollideWorldBounds(true).setDepth(100).setSize(22, 30).setOffset(13, 24);
    this.cameras.main.startFollow(this.player, true, 0.14, 0.14);
    // Spawn grace so opening overlap can't delete all 3 HP instantly.
    this.invuln = 2.5;
  }

  private buildGroups(): void {
    this.mobs = this.physics.add.group();
    this.shardsGroup = this.physics.add.group();
    this.pickups = this.physics.add.group();
    this.powers = this.physics.add.group();
  }

  private spawnInitialEntities(): void {
    for (let i = 0; i < 10; i += 1) this.spawnBobyl(false);
    for (let i = 0; i < 3; i += 1) this.spawnBobyl(true);
    this.spawnShard();
    this.spawnShard();
    for (let i = 0; i < 22; i += 1) this.spawnKukichi();
  }

  private bindSecrets(): void {
    const guide = this.physics.add.staticImage(700, 400, 'tex_akaky').setDepth(400);
    this.physics.add.overlap(this.player, guide, () => {
      this.flashToast('Акакий Куролесов: «Днём кокай, ночью беги от жвачников!»');
      writeSave(unlockBestiary(loadSave(), 'akaky'));
    });

    const mirror = this.physics.add.staticImage(1580, 1100, 'tex_mirror').setDepth(1100);
    this.physics.add.overlap(this.player, mirror, () => {
      if (this.mirrorFound) return;
      this.mirrorFound = true;
      this.ground.setTint(0x88aaff);
      this.flashToast('Зеркало Зазеркалья: Истуканус открыл третий след!');
      this.spawnShard();
      this.bumpScore(150, this.player.x, this.player.y - 20, 'СЕКРЕТ');
      Sfx.shard();
      writeSave(unlockBestiary(loadSave(), 'istukanus'));
      screenPunch(this, 0.012, 120);
    });

    const well = this.add.circle(980, 180, 18, 0xff66aa, 0.95).setDepth(200);
    this.physics.add.existing(well, true);
    this.tweens.add({
      targets: well,
      scale: { from: 0.9, to: 1.25 },
      duration: 700,
      yoyo: true,
      repeat: -1,
    });
    this.physics.add.overlap(
      this.player,
      well as unknown as Phaser.Types.Physics.Arcade.GameObjectWithBody,
      () => {
        if (this.eyeFound) return;
        this.eyeFound = true;
        well.destroy();
        this.hp = Math.min(3, this.hp + 1);
        this.kukichi += 5;
        this.bumpScore(80, this.player.x, this.player.y - 20, '+HP');
        this.flashToast('Спектральный колодец: +1 HP и +5 кукичей');
        Sfx.power();
        writeSave(unlockBestiary(loadSave(), 'spectral'));
        this.refreshHearts();
      },
    );
  }

  private bindOverlaps(): void {
    this.physics.add.overlap(this.player, this.shardsGroup, (_p, obj) => {
      const spr = obj as Phaser.Physics.Arcade.Sprite;
      spr.destroy();
      this.shards += 1;
      this.kukichi += 3;
      this.bumpScore(200 + this.combo * 10, spr.x, spr.y, 'СЛЕД!');
      burst(this, spr.x, spr.y, 'tex_spark_green', 18, 160);
      Sfx.shard();
      writeSave(unlockBestiary(loadSave(), 'pacanoid'));
      this.flashToast(`След пропавшего пацаноида ${this.shards}/3`);
      if (this.shards >= 3) this.finish(true);
    });

    this.physics.add.overlap(this.player, this.pickups, (_p, obj) => {
      const spr = obj as Phaser.Physics.Arcade.Sprite;
      spr.destroy();
      const gain = Math.max(1, 1 + Math.floor(this.combo / 3));
      this.kukichi += gain;
      this.bumpScore(10 * gain, spr.x, spr.y, `+${gain}`);
      Sfx.pickup();
      burst(this, spr.x, spr.y, 'tex_spark', 6, 80);
    });

    this.physics.add.overlap(this.player, this.powers, (_p, obj) => {
      const spr = obj as Phaser.Physics.Arcade.Sprite & { power?: PowerKind };
      const kind = spr.power ?? 'feast';
      spr.destroy();
      this.applyPower(kind);
    });

    // Contact damage is gated by invuln inside hurt() — never roll per-frame chance.
    this.physics.add.overlap(this.player, this.mobs, (_p, obj) => {
      const mob = obj as Mob;
      if (!mob.active) return;
      if (mob.kind === 'jvachnik') {
        this.hurt(1, mob.x, mob.y);
      } else if (this.night) {
        // Night bobyls nip once per i-frame window.
        this.hurt(1, mob.x, mob.y);
      }
      // Daytime bobyls are kok targets only — no contact chip damage.
    });
  }

  private bindInput(): void {
    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.wasd = this.input.keyboard.addKeys('W,A,S,D') as typeof this.wasd;
      this.keyKok = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);
      this.keyDash = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SHIFT);
    }
    this.pad = new VirtualPad(this);
  }

  private unlockIntroLore(): void {
    let save = loadSave();
    save = unlockBestiary(save, 'bobyl');
    save = unlockBestiary(save, 'sator');
    save = unlockBestiary(save, 'polenych');
    writeSave(save);
    this.flashToast('Поленыч преклонил колено. Собери следы — и не сдохни.');
    this.time.delayedCall(5500, () => {
      if (!this.ended) this.flashToast('Комбо коков множит кукичи. Рывок (SHIFT) даёт неуязвимость.');
    });
  }

  private buildHud(): void {
    this.goal = this.add
      .text(
        this.scale.width / 2,
        6,
        '3 следа · КОК/ПРОБЕЛ · РЫВОК/SHIFT · комбо = жир · ночь = жвачники',
        {
          fontFamily: 'Courier New',
          fontSize: '12px',
          color: '#ffe9a8',
          backgroundColor: '#000000bb',
          padding: { x: 8, y: 5 },
          align: 'center',
          wordWrap: { width: this.scale.width - 20 },
        },
      )
      .setOrigin(0.5, 0)
      .setScrollFactor(0)
      .setDepth(10000);

    this.hud = this.add
      .text(10, 44, '', {
        fontFamily: 'Courier New',
        fontSize: '13px',
        color: '#e8ffd8',
        backgroundColor: '#000000aa',
        padding: { x: 8, y: 6 },
      })
      .setScrollFactor(0)
      .setDepth(10000);

    this.comboText = this.add
      .text(this.scale.width - 12, 48, '', {
        fontFamily: 'Courier New',
        fontSize: '20px',
        color: '#ffcc66',
        stroke: '#000',
        strokeThickness: 5,
        align: 'right',
      })
      .setOrigin(1, 0)
      .setScrollFactor(0)
      .setDepth(10000);

    this.toast = this.add
      .text(this.scale.width / 2, 92, '', {
        fontFamily: 'Courier New',
        fontSize: '14px',
        color: '#ffe9a8',
        backgroundColor: '#000000cc',
        padding: { x: 10, y: 6 },
        align: 'center',
        wordWrap: { width: this.scale.width - 32 },
      })
      .setOrigin(0.5, 0)
      .setScrollFactor(0)
      .setDepth(10000)
      .setAlpha(0);

    this.marker = this.add.triangle(0, 0, 0, 16, 10, 0, 20, 16, 0xffe14a, 0.95).setDepth(5000);
    this.nightFx = this.add
      .rectangle(0, 0, this.scale.width, this.scale.height, 0x081828, 0)
      .setOrigin(0)
      .setScrollFactor(0)
      .setDepth(50);

    for (let i = 0; i < 3; i += 1) {
      this.hearts.push(
        this.add.image(18 + i * 28, 28, 'tex_heart').setScrollFactor(0).setDepth(10001).setScale(0.9),
      );
    }
    this.refreshHearts();
  }

  private tickSecond(): void {
    if (this.ended) return;
    this.timeLeft -= 1;

    if (this.timeLeft === 120) this.triggerWave(1, 'ВОЛНА 1');
    if (this.timeLeft === 90) this.triggerWave(2, 'ВОЛНА 2');

    if (this.timeLeft === 60 && !this.night) {
      this.night = true;
      this.cameras.main.setBackgroundColor('#071018');
      this.ground.setTint(0x3a5570);
      this.tweens.add({ targets: this.nightFx, fillAlpha: 0.35, duration: 800 });
      this.flashToast('НОЧЬ. Жвачники на охоте!');
      Sfx.night();
      writeSave(unlockBestiary(loadSave(), 'jvachnik'));
      writeSave(unlockBestiary(loadSave(), 'scripach'));
      this.triggerWave(3, 'НОЧНАЯ ВОЛНА');
    }

    if (this.timeLeft === 45 && this.shards < 3 && !this.mirrorFound) {
      this.flashToast('Подсказка: зеркало в углу открывает 3-й след.');
    }

    if (this.timeLeft === 30) {
      this.cameras.main.flash(280, 255, 120, 80);
      this.triggerWave(4, 'ФИНАЛЬНЫЙ НАПЛЫВ');
    }

    if (this.timeLeft <= 0) this.finish(false);
  }

  private triggerWave(n: number, label: string): void {
    this.wave = n;
    floatText(this, this.player.x, this.player.y - 50, label, '#ff8866');
    screenPunch(this, 0.008, 100);
    for (let i = 0; i < 4 + n * 2; i += 1) this.spawnBobyl(false);
    for (let i = 0; i < n; i += 1) this.spawnBobyl(true);
    if (this.night) for (let i = 0; i < 2 + n; i += 1) this.spawnJvachnik();
  }

  private spawnBobyl(hard: boolean): void {
    const far = this.awayFromPlayer(340);
    const spr = this.mobs.create(far.x, far.y, hard ? 'tex_bobyl_hard' : 'tex_bobyl') as Mob;
    spr.hp = hard ? 2 : 1;
    spr.kind = hard ? 'hard' : 'bobyl';
    spr.speed = hard ? 55 : 72;
    spr.setCollideWorldBounds(true).setBounce(1).setDepth(spr.y);
    spr.setSize(28, 34).setOffset(10, 12);
    spr.setVelocity(Phaser.Math.Between(-60, 60), Phaser.Math.Between(-60, 60));
    this.tweens.add({
      targets: spr,
      scaleY: { from: 0.92, to: 1.08 },
      duration: 380,
      yoyo: true,
      repeat: -1,
    });
  }

  private spawnJvachnik(): void {
    const far = this.awayFromPlayer(380);
    const spr = this.mobs.create(far.x, far.y, 'tex_jvachnik') as Mob;
    spr.hp = 1;
    spr.kind = 'jvachnik';
    spr.speed = 118;
    spr.setCollideWorldBounds(true).setDepth(spr.y).setTint(0xffaabb);
    spr.setSize(28, 34).setOffset(10, 12);
  }

  private spawnShard(): void {
    const far = this.awayFromPlayer(200);
    const s = this.shardsGroup.create(far.x, far.y, 'tex_pacanoid') as Phaser.Physics.Arcade.Sprite;
    s.setDepth(s.y);
    this.tweens.add({
      targets: s,
      scale: { from: 0.85, to: 1.25 },
      angle: 360,
      duration: 1400,
      yoyo: true,
      repeat: -1,
    });
  }

  private spawnKukichi(): void {
    const k = this.pickups.create(
      Phaser.Math.Between(60, WORLD_W - 60),
      Phaser.Math.Between(60, WORLD_H - 60),
      'tex_kukich',
    ) as Phaser.Physics.Arcade.Sprite;
    k.setScale(0.85).setDepth(k.y);
  }

  private spawnPower(x: number, y: number): void {
    const kinds: PowerKind[] = ['magnet', 'rage', 'haste', 'shield', 'feast'];
    const kind = Phaser.Utils.Array.GetRandom(kinds);
    const p = this.powers.create(x, y, 'tex_power') as Phaser.Physics.Arcade.Sprite & { power: PowerKind };
    p.power = kind;
    p.setDepth(y + 1);
    this.tweens.add({ targets: p, y: y - 8, duration: 500, yoyo: true, repeat: -1 });
  }

  private awayFromPlayer(minDist: number): { x: number; y: number } {
    for (let i = 0; i < 24; i += 1) {
      const x = Phaser.Math.Between(80, WORLD_W - 80);
      const y = Phaser.Math.Between(80, WORLD_H - 80);
      if (Phaser.Math.Distance.Between(x, y, this.player.x, this.player.y) >= minDist) {
        return { x, y };
      }
    }
    return { x: Phaser.Math.Between(80, WORLD_W - 80), y: Phaser.Math.Between(80, WORLD_H - 80) };
  }

  update(time: number, dtMs: number): void {
    if (this.ended || !this.player?.body) return;
    const dt = dtMs / 1000;

    this.kokCd = Math.max(0, this.kokCd - dt);
    this.dashCd = Math.max(0, this.dashCd - dt);
    this.dashLeft = Math.max(0, this.dashLeft - dt);
    this.invuln = Math.max(0, this.invuln - dt);
    this.magnetLeft = Math.max(0, this.magnetLeft - dt);
    this.rageLeft = Math.max(0, this.rageLeft - dt);
    this.hasteLeft = Math.max(0, this.hasteLeft - dt);
    this.comboTimer = Math.max(0, this.comboTimer - dt);
    if (this.comboTimer <= 0 && this.combo > 0) this.combo = 0;

    let vx = 0;
    let vy = 0;
    if (this.cursors?.left.isDown || this.wasd?.A.isDown) vx -= 1;
    if (this.cursors?.right.isDown || this.wasd?.D.isDown) vx += 1;
    if (this.cursors?.up.isDown || this.wasd?.W.isDown) vy -= 1;
    if (this.cursors?.down.isDown || this.wasd?.S.isDown) vy += 1;
    vx += this.pad.vector.x;
    vy += this.pad.vector.y;

    const len = Math.hypot(vx, vy);
    if (len > 0.05) this.facing.set(vx / len, vy / len);

    let speed = this.hasteLeft > 0 ? 270 : 205;
    if (this.dashLeft > 0) speed = 430;
    if (this.inSwamp() && this.dashLeft <= 0) speed *= 0.55;

    if (len > 0.05) this.player.setVelocity((vx / len) * speed, (vy / len) * speed);
    else this.player.setVelocity(0, 0);

    this.player.setDepth(this.player.y);
    this.player.setAlpha(this.invuln > 0 ? 0.55 + Math.sin(time / 40) * 0.25 : 1);

    if ((this.keyKok && Phaser.Input.Keyboard.JustDown(this.keyKok)) || this.pad.consumeKok()) {
      this.tryKok();
    }
    if ((this.keyDash && Phaser.Input.Keyboard.JustDown(this.keyDash)) || this.pad.consumeDash()) {
      this.tryDash();
    }

    this.aiMobs();
    this.pullMagnet();
    this.updateMarker();
    this.updateHud();
    this.ground.tilePositionX += dt * 4;
  }

  private inSwamp(): boolean {
    return this.swamps.some(
      (s) => Phaser.Math.Distance.Between(this.player.x, this.player.y, s.x, s.y) < 48,
    );
  }

  private aiMobs(): void {
    this.mobs.children.each((obj) => {
      const m = obj as Mob;
      if (!m.active) return true;
      m.setDepth(m.y);

      if (m.kind === 'jvachnik') {
        const ang = Phaser.Math.Angle.Between(m.x, m.y, this.player.x, this.player.y);
        m.setVelocity(Math.cos(ang) * m.speed, Math.sin(ang) * m.speed);
      } else if (Phaser.Math.Between(0, 100) < 3) {
        const d = Phaser.Math.Distance.Between(m.x, m.y, this.player.x, this.player.y);
        if (d < 230) {
          const ang = Phaser.Math.Angle.Between(m.x, m.y, this.player.x, this.player.y);
          m.setVelocity(Math.cos(ang) * m.speed * 0.85, Math.sin(ang) * m.speed * 0.85);
        } else {
          m.setVelocity(Phaser.Math.Between(-70, 70), Phaser.Math.Between(-70, 70));
        }
      }
      return true;
    });
  }

  private pullMagnet(): void {
    if (this.magnetLeft <= 0) return;
    this.pickups.children.each((obj) => {
      const s = obj as Phaser.Physics.Arcade.Sprite;
      if (!s.active) return true;
      const ang = Phaser.Math.Angle.Between(s.x, s.y, this.player.x, this.player.y);
      s.x += Math.cos(ang) * 7;
      s.y += Math.sin(ang) * 7;
      return true;
    });
  }

  private tryDash(): void {
    if (this.dashCd > 0) return;
    this.dashCd = DASH_CD;
    this.dashLeft = 0.22;
    this.invuln = Math.max(this.invuln, 0.28);
    burst(this, this.player.x, this.player.y, 'tex_spark', 8, 100);
    Sfx.dash();
    floatText(this, this.player.x, this.player.y - 28, 'РЫВОК', '#8ec8ff');
  }

  private tryKok(): void {
    if (this.kokCd > 0) return;
    this.kokCd = this.rageLeft > 0 ? 0.18 : 0.32;
    Sfx.kok();

    const range = this.rageLeft > 0 ? 165 : KOK_RANGE;
    const slash = this.add
      .circle(
        this.player.x + this.facing.x * 28,
        this.player.y + this.facing.y * 28,
        range / 4,
        0xffe9a8,
        0.35,
      )
      .setDepth(this.player.y + 1);
    this.tweens.add({
      targets: slash,
      alpha: 0,
      scale: 1.6,
      duration: 160,
      onComplete: () => slash.destroy(),
    });

    const hits = (this.mobs.getChildren() as Mob[]).filter(
      (e) => e.active && Phaser.Math.Distance.Between(this.player.x, this.player.y, e.x, e.y) <= range,
    );
    if (hits.length === 0) {
      floatText(this, this.player.x, this.player.y - 20, 'мимо', '#aaaaaa');
      return;
    }

    screenPunch(this, 0.008, 60);
    for (const hit of hits) {
      hit.hp -= 1;
      hit.setTintFill(0xffffff);
      this.time.delayedCall(70, () => {
        if (hit.active) hit.clearTint();
      });
      burst(this, hit.x, hit.y, 'tex_spark', 8, 110);
      const ang = Phaser.Math.Angle.Between(this.player.x, this.player.y, hit.x, hit.y);
      hit.setVelocity(Math.cos(ang) * 240, Math.sin(ang) * 240);
      if (hit.hp > 0) {
        floatText(this, hit.x, hit.y - 16, 'ТРЕЩИНА', '#ffe9a8');
        continue;
      }
      this.onMobKilled(hit);
    }
  }

  private onMobKilled(hit: Mob): void {
    const hard = hit.kind === 'hard';
    const jv = hit.kind === 'jvachnik';
    const { x, y } = hit;
    hit.destroy();

    this.kokked += 1;
    this.bumpCombo();
    hitstop(this, 30);
    Sfx.kokKill();
    burst(this, x, y, jv ? 'tex_spark_red' : 'tex_spark_green', 16, 150);

    const mult = 1 + Math.floor(this.combo / 3);
    const gain = (hard ? 3 : jv ? 4 : 1) * mult;
    this.kukichi += gain;
    this.bumpScore(gain * 12 + this.combo * 5, x, y, `x${mult}`);

    let save = loadSave();
    save = unlockBestiary(save, jv ? 'jvachnik' : hard ? 'bobyl_hard' : 'bobyl');

    if (Phaser.Math.Between(1, 100) <= 14) this.spawnPower(x, y);
    if (Phaser.Math.Between(1, 100) <= 12) {
      save = unlockBestiary(save, 'mihail');
      this.kukichi += 3;
      floatText(this, x, y - 30, 'Михаил +скорлупа', '#c9a44a');
    }
    if (Phaser.Math.Between(1, 100) <= 40) {
      const k = this.pickups.create(
        x + Phaser.Math.Between(-24, 24),
        y + Phaser.Math.Between(-24, 24),
        'tex_kukich',
      ) as Phaser.Physics.Arcade.Sprite;
      k.setScale(0.9).setDepth(k.y);
    }

    writeSave(save);
    if (this.mobs.countActive(true) < 10) this.spawnBobyl(Math.random() < 0.35);
  }

  private bumpCombo(): void {
    this.combo += 1;
    this.maxCombo = Math.max(this.maxCombo, this.combo);
    this.comboTimer = COMBO_WINDOW;
    if (this.combo >= 5 && this.combo % 5 === 0) {
      floatText(this, this.player.x, this.player.y - 40, `КОМБО ${this.combo}!`, '#ffcc66');
      screenPunch(this, 0.012, 90);
    }
  }

  private applyPower(kind: PowerKind): void {
    Sfx.power();
    burst(this, this.player.x, this.player.y, 'tex_spark', 14, 140);

    if (kind === 'magnet') {
      this.magnetLeft = 9;
      this.flashToast('МАГНИТ кукичей — 9 сек');
    } else if (kind === 'rage') {
      this.rageLeft = 7;
      this.flashToast('ЯРОСТЬ КОКА — AOE 7 сек');
    } else if (kind === 'haste') {
      this.hasteLeft = 8;
      this.flashToast('УСКОРЕНИЕ — 8 сек');
    } else if (kind === 'shield') {
      this.shieldOn = true;
      this.flashToast('ЩИТ — блокирует 1 удар');
    } else {
      this.kukichi += 8;
      this.bumpScore(80, this.player.x, this.player.y - 20, 'ПИР');
      this.flashToast('ПИР ДЯДЮШКИ — +8 кукичей');
      writeSave(unlockBestiary(loadSave(), 'fantasmagor'));
    }
  }

  private hurt(amount: number, fromX: number, fromY: number): void {
    if (this.invuln > 0 || this.dashLeft > 0 || this.ended) return;

    if (this.shieldOn) {
      this.shieldOn = false;
      this.invuln = 1.0;
      floatText(this, this.player.x, this.player.y - 24, 'ЩИТ!', '#8ec8ff');
      Sfx.dash();
      return;
    }

    this.hp -= amount;
    this.damageTaken += amount;
    // Long enough that one overlap can't melt all hearts in a frame burst.
    this.invuln = 1.35;
    this.combo = 0;
    this.comboTimer = 0;
    this.refreshHearts();
    Sfx.hurt();
    screenPunch(this, 0.02, 160);
    this.cameras.main.flash(80, 180, 40, 40);
    burst(this, this.player.x, this.player.y, 'tex_spark_red', 12, 130);

    const ang = Phaser.Math.Angle.Between(fromX, fromY, this.player.x, this.player.y);
    this.player.setVelocity(Math.cos(ang) * 280, Math.sin(ang) * 280);
    floatText(this, this.player.x, this.player.y - 20, '-HP', '#ff6688');

    if (this.hp <= 0) this.finish(false);
  }

  private refreshHearts(): void {
    this.hearts.forEach((h, i) => h.setVisible(i < this.hp));
  }

  private bumpScore(n: number, x: number, y: number, label?: string): void {
    this.score += n;
    if (label) floatText(this, x, y, label, '#ffe9a8');
  }

  private updateMarker(): void {
    let nearestX = 0;
    let nearestY = 0;
    let best = Number.POSITIVE_INFINITY;
    let found = false;

    this.shardsGroup.children.each((obj) => {
      const s = obj as Phaser.Physics.Arcade.Sprite;
      if (!s.active) return true;
      const d = Phaser.Math.Distance.Between(this.player.x, this.player.y, s.x, s.y);
      if (d < best) {
        best = d;
        nearestX = s.x;
        nearestY = s.y;
        found = true;
      }
      return true;
    });

    if (!found || best < 100) {
      this.marker.setVisible(false);
      return;
    }

    const ang = Phaser.Math.Angle.Between(this.player.x, this.player.y, nearestX, nearestY);
    this.marker
      .setVisible(true)
      .setPosition(this.player.x + Math.cos(ang) * 46, this.player.y + Math.sin(ang) * 46)
      .setRotation(ang + Math.PI / 2);
  }

  private updateHud(): void {
    const mm = Math.floor(this.timeLeft / 60);
    const ss = String(this.timeLeft % 60).padStart(2, '0');
    const buffs: string[] = [];
    if (this.magnetLeft > 0) buffs.push('М');
    if (this.rageLeft > 0) buffs.push('Я');
    if (this.hasteLeft > 0) buffs.push('У');
    if (this.shieldOn) buffs.push('Щ');

    this.hud.setText(
      `Роща ${mm}:${ss}${this.night ? ' · НОЧЬ' : ''}${buffs.length ? ' [' + buffs.join('') + ']' : ''}\n` +
        `Кукичи ${this.kukichi}  Кок ${this.kokked}  Следы ${this.shards}/3\n` +
        `Очки ${this.score}  Волна ${this.wave}`,
    );
    this.comboText.setText(this.combo >= 2 ? `COMBO x${this.combo}` : '');
  }

  private flashToast(msg: string): void {
    this.toast.setText(msg);
    this.tweens.killTweensOf(this.toast);
    this.toast.setAlpha(1);
    this.tweens.add({ targets: this.toast, alpha: 0, delay: 2400, duration: 400 });
  }

  private finish(gotAllShards: boolean): void {
    if (this.ended) return;
    this.ended = true;
    this.player.setVelocity(0, 0);

    const perfect = gotAllShards && this.shards >= 3 && this.damageTaken === 0;
    if (gotAllShards && this.shards >= 3) Sfx.win();

    const bonus = (gotAllShards ? 10 : 0) + (perfect ? 25 : 0) + this.maxCombo * 2;
    const result: MatchResult = {
      kukichi: this.kukichi + bonus,
      kokked: this.kokked,
      shards: this.shards,
      perfect,
      mirrorFound: this.mirrorFound,
      eyeFound: this.eyeFound,
      score: this.score + bonus * 10 + this.maxCombo * 40,
      maxCombo: this.maxCombo,
      damageTaken: this.damageTaken,
    };
    this.scene.start('Results', result);
  }
}
