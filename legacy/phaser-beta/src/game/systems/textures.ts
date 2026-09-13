import Phaser from 'phaser';

/** Dense PS1/Dendy-looking procedural atlas — no external art deps. */
export function generateTextures(scene: Phaser.Scene): void {
  const g = scene.make.graphics({ x: 0, y: 0 });

  // Ground — dithered moss tile
  g.fillStyle(0x163526, 1);
  g.fillRect(0, 0, 64, 64);
  g.fillStyle(0x1c4030, 1);
  for (let y = 0; y < 64; y += 8) {
    for (let x = (y / 8) % 2 === 0 ? 0 : 4; x < 64; x += 8) {
      g.fillRect(x, y, 4, 4);
    }
  }
  g.fillStyle(0x2a5540, 0.55);
  g.fillCircle(12, 20, 3);
  g.fillCircle(48, 44, 2);
  g.fillCircle(30, 52, 2);
  g.generateTexture('tex_ground', 64, 64);

  // Swamp slow zone
  g.clear();
  g.fillStyle(0x1a3040, 0.85);
  g.fillEllipse(32, 24, 60, 40);
  g.fillStyle(0x2a5060, 0.5);
  g.fillEllipse(24, 20, 20, 12);
  g.generateTexture('tex_swamp', 64, 48);

  // Player — coat + papakha (Sator vibes)
  g.clear();
  g.fillStyle(0x2a1a12, 1);
  g.fillRect(10, 28, 28, 30);
  g.fillStyle(0x4a3424, 1);
  g.fillRect(8, 8, 32, 20);
  g.fillStyle(0x1a120c, 1);
  g.fillRect(6, 6, 36, 8);
  g.fillStyle(0xd4b896, 1);
  g.fillRect(14, 22, 20, 14);
  g.fillStyle(0x1a120c, 1);
  g.fillRect(18, 26, 4, 4);
  g.fillRect(28, 26, 4, 4);
  g.fillStyle(0x6a4428, 1);
  g.fillRect(18, 34, 12, 3);
  g.fillStyle(0xc9a44a, 1);
  g.fillRect(36, 34, 6, 14); // kokalka stick
  g.generateTexture('tex_player', 48, 64);

  // Bobyl
  g.clear();
  g.fillStyle(0x6bcf4a, 1);
  g.fillEllipse(24, 30, 38, 46);
  g.fillStyle(0x8ae868, 1);
  g.fillEllipse(18, 22, 12, 10);
  g.fillStyle(0x0d1a10, 1);
  g.fillCircle(16, 26, 4);
  g.fillCircle(32, 26, 4);
  g.fillStyle(0xffffff, 0.35);
  g.fillCircle(15, 24, 1.5);
  g.fillCircle(31, 24, 1.5);
  g.fillStyle(0x2a5a20, 1);
  g.fillEllipse(24, 38, 16, 8);
  g.generateTexture('tex_bobyl', 48, 56);

  // Hard bobyl (Black Eclair)
  g.clear();
  g.fillStyle(0x1a2a18, 1);
  g.fillEllipse(24, 30, 42, 50);
  g.fillStyle(0x3d5a30, 1);
  g.fillEllipse(24, 30, 34, 42);
  g.fillStyle(0xc9e8b8, 1);
  g.fillRect(8, 10, 32, 8);
  g.fillStyle(0xffe9a8, 1);
  g.fillRect(10, 12, 8, 4);
  g.fillStyle(0x0d1a10, 1);
  g.fillCircle(16, 28, 5);
  g.fillCircle(32, 28, 5);
  g.fillStyle(0xff4466, 1);
  g.fillCircle(16, 28, 2);
  g.fillCircle(32, 28, 2);
  g.generateTexture('tex_bobyl_hard', 48, 56);

  // Jvachnik (night hunter)
  g.clear();
  g.fillStyle(0x2a4050, 1);
  g.fillEllipse(24, 28, 40, 44);
  g.fillStyle(0x8ab0c8, 0.7);
  g.fillEllipse(24, 28, 28, 32);
  g.fillStyle(0xff6688, 1);
  g.fillCircle(14, 24, 5);
  g.fillCircle(34, 24, 5);
  g.fillStyle(0xffe9a8, 1);
  g.fillCircle(14, 24, 2);
  g.fillCircle(34, 24, 2);
  g.fillStyle(0x102030, 1);
  g.fillTriangle(18, 36, 30, 36, 24, 48);
  g.generateTexture('tex_jvachnik', 48, 56);

  // Pacanoid shard
  g.clear();
  g.fillStyle(0x3dff7a, 1);
  g.fillCircle(16, 16, 14);
  g.fillStyle(0xa8ffc8, 1);
  g.fillCircle(16, 16, 8);
  g.fillStyle(0xffffff, 0.7);
  g.fillCircle(12, 12, 4);
  g.fillStyle(0x1a4028, 1);
  g.fillCircle(16, 16, 3);
  g.generateTexture('tex_pacanoid', 32, 32);

  // Kukichi
  g.clear();
  g.fillStyle(0xd43b3b, 1);
  g.fillCircle(16, 18, 14);
  g.fillStyle(0xff8a6a, 1);
  g.fillCircle(12, 14, 5);
  g.fillStyle(0x0b120f, 1);
  g.fillCircle(24, 12, 7);
  g.fillStyle(0x5a3a1a, 1);
  g.fillRect(14, 2, 4, 8);
  g.fillStyle(0x3a8b3a, 1);
  g.fillTriangle(18, 4, 28, 2, 22, 12);
  g.generateTexture('tex_kukich', 32, 32);

  // Powerup gem
  g.clear();
  g.fillStyle(0xffe14a, 1);
  g.fillTriangle(16, 2, 30, 16, 16, 30);
  g.fillTriangle(16, 2, 2, 16, 16, 30);
  g.fillStyle(0xfff6a8, 1);
  g.fillTriangle(16, 6, 22, 16, 16, 24);
  g.generateTexture('tex_power', 32, 32);

  // Tree
  g.clear();
  g.fillStyle(0x3a2818, 1);
  g.fillRect(22, 44, 12, 44);
  g.fillStyle(0x1a3a28, 1);
  g.fillCircle(28, 30, 26);
  g.fillStyle(0x0e2418, 1);
  g.fillCircle(12, 38, 14);
  g.fillCircle(44, 38, 14);
  g.fillStyle(0x2a5540, 0.6);
  g.fillCircle(28, 22, 10);
  g.generateTexture('tex_tree', 56, 88);

  // Mirror
  g.clear();
  g.lineStyle(4, 0xc9a44a, 1);
  g.strokeRect(4, 4, 40, 64);
  g.fillStyle(0x6ec8ff, 0.55);
  g.fillRect(8, 8, 32, 56);
  g.fillStyle(0xffffff, 0.25);
  g.fillRect(12, 12, 10, 20);
  g.generateTexture('tex_mirror', 48, 72);

  // Akaky
  g.clear();
  g.fillStyle(0x6a8a5a, 1);
  g.fillEllipse(24, 38, 30, 36);
  g.fillStyle(0xc8d8a8, 1);
  g.fillCircle(24, 18, 13);
  g.fillStyle(0x1a120c, 1);
  g.fillCircle(20, 18, 2);
  g.fillCircle(28, 18, 2);
  g.fillStyle(0x8a6a4a, 1);
  g.fillEllipse(24, 10, 18, 8);
  g.generateTexture('tex_akaky', 48, 56);

  // Heart UI
  g.clear();
  g.fillStyle(0xff4466, 1);
  g.fillCircle(8, 8, 7);
  g.fillCircle(18, 8, 7);
  g.fillTriangle(2, 10, 24, 10, 13, 24);
  g.generateTexture('tex_heart', 26, 26);

  // Spark / particles
  g.clear();
  g.fillStyle(0xffe9a8, 1);
  g.fillCircle(4, 4, 4);
  g.generateTexture('tex_spark', 8, 8);

  g.clear();
  g.fillStyle(0x66ffaa, 1);
  g.fillCircle(3, 3, 3);
  g.generateTexture('tex_spark_green', 6, 6);

  g.clear();
  g.fillStyle(0xff6688, 1);
  g.fillCircle(3, 3, 3);
  g.generateTexture('tex_spark_red', 6, 6);

  g.destroy();
}
