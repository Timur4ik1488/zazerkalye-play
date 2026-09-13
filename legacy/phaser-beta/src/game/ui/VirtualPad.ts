import Phaser from 'phaser';

export class VirtualPad {
  vector = new Phaser.Math.Vector2(0, 0);
  private base: Phaser.GameObjects.Arc;
  private thumb: Phaser.GameObjects.Arc;
  private kokBtn: Phaser.GameObjects.Arc;
  private dashBtn: Phaser.GameObjects.Arc;
  private kokLabel: Phaser.GameObjects.Text;
  private dashLabel: Phaser.GameObjects.Text;
  private stickId: number | null = null;
  private kokQueued = false;
  private dashQueued = false;
  private readonly radius = 56;

  constructor(scene: Phaser.Scene) {
    const w = scene.scale.width;
    const h = scene.scale.height;

    this.base = scene.add.circle(96, h - 96, this.radius, 0x000000, 0.4).setScrollFactor(0).setDepth(9000);
    this.thumb = scene.add.circle(96, h - 96, 30, 0xc9e8b8, 0.6).setScrollFactor(0).setDepth(9001);

    this.kokBtn = scene.add
      .circle(w - 78, h - 100, 46, 0xd43b3b, 0.62)
      .setScrollFactor(0)
      .setDepth(9000)
      .setInteractive();
    this.kokLabel = scene.add
      .text(w - 78, h - 100, 'КОК', {
        fontFamily: 'Courier New',
        fontSize: '16px',
        color: '#ffffff',
        fontStyle: 'bold',
      })
      .setOrigin(0.5)
      .setScrollFactor(0)
      .setDepth(9001);

    this.dashBtn = scene.add
      .circle(w - 168, h - 72, 34, 0x3a6a9a, 0.62)
      .setScrollFactor(0)
      .setDepth(9000)
      .setInteractive();
    this.dashLabel = scene.add
      .text(w - 168, h - 72, 'РЫВ', {
        fontFamily: 'Courier New',
        fontSize: '13px',
        color: '#ffffff',
      })
      .setOrigin(0.5)
      .setScrollFactor(0)
      .setDepth(9001);

    scene.input.on('pointerdown', (p: Phaser.Input.Pointer) => {
      if (Phaser.Math.Distance.Between(p.x, p.y, this.base.x, this.base.y) <= this.radius + 28) {
        this.stickId = p.id;
        this.moveStick(p.x, p.y);
      }
      if (Phaser.Math.Distance.Between(p.x, p.y, this.kokBtn.x, this.kokBtn.y) <= 50) {
        this.kokQueued = true;
        this.kokBtn.setScale(0.9);
      }
      if (Phaser.Math.Distance.Between(p.x, p.y, this.dashBtn.x, this.dashBtn.y) <= 40) {
        this.dashQueued = true;
        this.dashBtn.setScale(0.9);
      }
    });
    scene.input.on('pointermove', (p: Phaser.Input.Pointer) => {
      if (p.id === this.stickId) this.moveStick(p.x, p.y);
    });
    scene.input.on('pointerup', (p: Phaser.Input.Pointer) => {
      if (p.id === this.stickId) {
        this.stickId = null;
        this.vector.set(0, 0);
        this.thumb.setPosition(this.base.x, this.base.y);
      }
      this.kokBtn.setScale(1);
      this.dashBtn.setScale(1);
    });

    scene.scale.on('resize', (size: Phaser.Structs.Size) => {
      this.base.setPosition(96, size.height - 96);
      this.thumb.setPosition(96, size.height - 96);
      this.kokBtn.setPosition(size.width - 78, size.height - 100);
      this.kokLabel.setPosition(size.width - 78, size.height - 100);
      this.dashBtn.setPosition(size.width - 168, size.height - 72);
      this.dashLabel.setPosition(size.width - 168, size.height - 72);
    });
  }

  private moveStick(x: number, y: number): void {
    const dx = x - this.base.x;
    const dy = y - this.base.y;
    const len = Math.min(this.radius, Math.hypot(dx, dy));
    const ang = Math.atan2(dy, dx);
    this.thumb.setPosition(this.base.x + Math.cos(ang) * len, this.base.y + Math.sin(ang) * len);
    this.vector.set((Math.cos(ang) * len) / this.radius, (Math.sin(ang) * len) / this.radius);
  }

  consumeKok(): boolean {
    if (!this.kokQueued) return false;
    this.kokQueued = false;
    return true;
  }

  consumeDash(): boolean {
    if (!this.dashQueued) return false;
    this.dashQueued = false;
    return true;
  }
}
