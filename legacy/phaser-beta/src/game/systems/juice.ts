import Phaser from 'phaser';

export function floatText(
  scene: Phaser.Scene,
  x: number,
  y: number,
  msg: string,
  color = '#ffe9a8',
): void {
  const t = scene.add
    .text(x, y, msg, {
      fontFamily: 'Courier New',
      fontSize: '16px',
      color,
      stroke: '#000000',
      strokeThickness: 4,
    })
    .setOrigin(0.5)
    .setDepth(8000);
  scene.tweens.add({
    targets: t,
    y: y - 44,
    alpha: 0,
    duration: 720,
    ease: 'Cubic.easeOut',
    onComplete: () => t.destroy(),
  });
}

export function burst(
  scene: Phaser.Scene,
  x: number,
  y: number,
  tex: string,
  count = 12,
  speed = 120,
): void {
  const p = scene.add.particles(x, y, tex, {
    speed: { min: speed * 0.35, max: speed },
    lifespan: 400,
    quantity: 1,
    scale: { start: 1.2, end: 0 },
  });
  p.explode(count);
  scene.time.delayedCall(500, () => p.destroy());
}

export function hitstop(scene: Phaser.Scene, ms = 40): void {
  scene.time.timeScale = 0.12;
  scene.time.delayedCall(ms, () => {
    scene.time.timeScale = 1;
  });
}

export function screenPunch(scene: Phaser.Scene, intensity = 0.01, ms = 80): void {
  scene.cameras.main.shake(ms, intensity);
}
