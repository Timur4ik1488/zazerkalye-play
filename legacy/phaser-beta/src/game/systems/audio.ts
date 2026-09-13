type Tone = { freq: number; dur: number; type?: OscillatorType; gain?: number };

let ctx: AudioContext | null = null;

function ac(): AudioContext | null {
  try {
    if (!ctx) ctx = new AudioContext();
    if (ctx.state === 'suspended') void ctx.resume();
    return ctx;
  } catch {
    return null;
  }
}

function beep(tones: Tone[]): void {
  const c = ac();
  if (!c) return;
  let t = c.currentTime;
  for (const tone of tones) {
    const o = c.createOscillator();
    const g = c.createGain();
    o.type = tone.type ?? 'square';
    o.frequency.value = tone.freq;
    g.gain.value = tone.gain ?? 0.045;
    g.gain.exponentialRampToValueAtTime(0.001, t + tone.dur);
    o.connect(g);
    g.connect(c.destination);
    o.start(t);
    o.stop(t + tone.dur);
    t += tone.dur * 0.55;
  }
}

export const Sfx = {
  kok: () => beep([{ freq: 180, dur: 0.05 }, { freq: 320, dur: 0.07, type: 'sawtooth' }]),
  kokKill: () =>
    beep([
      { freq: 220, dur: 0.05 },
      { freq: 440, dur: 0.07 },
      { freq: 660, dur: 0.1, type: 'triangle', gain: 0.05 },
    ]),
  pickup: () => beep([{ freq: 660, dur: 0.05, type: 'triangle' }, { freq: 880, dur: 0.07, type: 'triangle' }]),
  shard: () =>
    beep([
      { freq: 520, dur: 0.06, type: 'sine' },
      { freq: 780, dur: 0.08, type: 'sine' },
      { freq: 1040, dur: 0.12, type: 'sine', gain: 0.04 },
    ]),
  hurt: () => beep([{ freq: 120, dur: 0.12, type: 'sawtooth', gain: 0.06 }, { freq: 80, dur: 0.14, type: 'square' }]),
  dash: () => beep([{ freq: 400, dur: 0.04, type: 'triangle' }, { freq: 200, dur: 0.07, type: 'triangle' }]),
  power: () =>
    beep([
      { freq: 300, dur: 0.05, type: 'sine' },
      { freq: 450, dur: 0.05, type: 'sine' },
      { freq: 600, dur: 0.1, type: 'sine', gain: 0.05 },
    ]),
  night: () => beep([{ freq: 90, dur: 0.25, type: 'sine', gain: 0.05 }, { freq: 60, dur: 0.28, type: 'sine', gain: 0.04 }]),
  win: () =>
    beep([
      { freq: 440, dur: 0.08, type: 'triangle' },
      { freq: 554, dur: 0.08, type: 'triangle' },
      { freq: 659, dur: 0.15, type: 'triangle', gain: 0.05 },
    ]),
  ui: () => beep([{ freq: 500, dur: 0.04, type: 'square', gain: 0.03 }]),
};
