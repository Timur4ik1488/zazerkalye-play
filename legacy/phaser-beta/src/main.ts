import './style.css';
import Phaser from 'phaser';
import { BootScene } from './game/scenes/BootScene';
import { MenuScene } from './game/scenes/MenuScene';
import { MatchScene } from './game/scenes/MatchScene';
import { ResultsScene } from './game/scenes/ResultsScene';
import { BestiaryScene } from './game/scenes/BestiaryScene';

const root = document.querySelector('#game-root');
if (!root) throw new Error('#game-root not found');

document.body.appendChild(Object.assign(document.createElement('div'), { className: 'crt-overlay' }));
document.body.appendChild(Object.assign(document.createElement('div'), { className: 'vignette' }));

new Phaser.Game({
  type: Phaser.AUTO,
  parent: root as HTMLElement,
  backgroundColor: '#0b120f',
  scale: {
    mode: Phaser.Scale.RESIZE,
    autoCenter: Phaser.Scale.CENTER_BOTH,
    width: window.innerWidth,
    height: window.innerHeight,
  },
  physics: {
    default: 'arcade',
    arcade: { debug: false },
  },
  scene: [BootScene, MenuScene, MatchScene, ResultsScene, BestiaryScene],
  input: { activePointers: 3 },
  render: { pixelArt: true, antialias: false },
});
