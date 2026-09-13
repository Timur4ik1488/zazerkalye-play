import { emptySave, type SaveData } from './content';

const KEY = 'zazerkalye_save_v2';

/** Старые id бестиария → канон zazerwiki.com */
const ID_MIGRATE: Record<string, string> = {
  ulitoliy: 'akaky',
  kozhnik: 'mihail',
  ono: 'jvachnik',
  arbiter: 'istukanus',
  tree: 'scripach',
  eye: 'spectral',
};

function migrateBestiary(raw: Record<string, boolean> | undefined): Record<string, boolean> {
  const out: Record<string, boolean> = { ...(raw ?? {}) };
  for (const [from, to] of Object.entries(ID_MIGRATE)) {
    if (out[from]) {
      out[to] = true;
      delete out[from];
    }
  }
  return out;
}

export function loadSave(): SaveData {
  try {
    const raw = localStorage.getItem(KEY) ?? localStorage.getItem('zazerkalye_save_v1');
    if (!raw) return emptySave();
    const parsed = JSON.parse(raw) as Partial<SaveData>;
    return {
      ...emptySave(),
      ...parsed,
      bestiary: migrateBestiary(parsed.bestiary),
    };
  } catch {
    return emptySave();
  }
}

export function writeSave(data: SaveData): void {
  localStorage.setItem(KEY, JSON.stringify(data));
}

export function addKukichi(data: SaveData, amount: number): SaveData {
  const next: SaveData = {
    ...data,
    kukichi: data.kukichi + amount,
    pakichi: data.pakichi + Math.floor(amount / 2),
  };
  writeSave(next);
  return next;
}

export function unlockBestiary(data: SaveData, id: string): SaveData {
  const canon = ID_MIGRATE[id] ?? id;
  if (data.bestiary[canon]) return data;
  const next: SaveData = {
    ...data,
    bestiary: { ...data.bestiary, [canon]: true },
  };
  writeSave(next);
  return next;
}
