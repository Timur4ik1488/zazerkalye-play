export type Rarity = 'common' | 'uncommon' | 'rare' | 'secret';

export type BestiaryDef = {
  id: string;
  name: string;
  rarity: Rarity;
  blurb: string;
};

export type SaveData = {
  kukichi: number;
  pakichi: number;
  bestScore: number;
  matchesPlayed: number;
  bestiary: Record<string, boolean>;
};

export const MATCH_SECONDS = 150;

/**
 * Бестиарий по фанатской wiki: https://zazerwiki.com/
 * Имена и роли — канон wiki; механика матча (следы/таймер) — игровая условность.
 */
export const BESTIARY: BestiaryDef[] = [
  {
    id: 'sator',
    name: 'Сатор Арепыч',
    rarity: 'common',
    blurb: 'К бобылям беспощаден: кокает без раздумий. Остальным путникам кокание — всё же выбор.',
  },
  {
    id: 'bobyl',
    name: 'Бобыль',
    rarity: 'common',
    blurb: 'Яйцеподобное существо. Лесные бобыли узнаются по клокочущему запаху. Каждый уникален — и звуком, и сложностью кокания.',
  },
  {
    id: 'bobyl_hard',
    name: 'Чёрный Эклер',
    rarity: 'uncommon',
    blurb: 'Один из знаменитых бобылей. Скорлупа потолще — кокайте дважды. (Также: Летописный Артём, Гузлик, Ёксель-Моксель…)',
  },
  {
    id: 'pacanoid',
    name: 'Пацаноид',
    rarity: 'uncommon',
    blurb: 'Их всего двое. Зелёноголовые культовые существа; один пропадал, второй в тумане. Конфликт с Казимиром — из‑за ложек.',
  },
  {
    id: 'fantasmagor',
    name: 'Дядюшка Фантасмагор',
    rarity: 'rare',
    blurb: 'Ключевая фигура Зазеркалья. Ему везут алмазные соления со всех уголков — опоздаешь с доставкой, накличет беду.',
  },
  {
    id: 'jvachnik',
    name: 'Жвачник',
    rarity: 'rare',
    blurb: 'Хищник болот Сумеречной рощи. Выглядит как Пакет, но утащит под воду. Ходит стаями — одного не бывает.',
  },
  {
    id: 'mihail',
    name: 'Михаил',
    rarity: 'secret',
    blurb: 'Охотник за скорлупками: с бобылей, из ямы Доктора Эпикантуса или от пескарей. Не стой между ним и скорлупой.',
  },
  {
    id: 'kazimir',
    name: 'Казимир',
    rarity: 'uncommon',
    blurb: 'Полуволк в доспехах. Мастерски ворует ложки (уже 40+). Пацаноиды на него в ярости — он снова увёл их ложки.',
  },
  {
    id: 'istukanus',
    name: 'Истуканус',
    rarity: 'rare',
    blurb: 'Неподвижный идол в заброшенном храме. Загадка: «Брунявая Чуня или Чунявая Бруня?» Правильный ответ — обои.',
  },
  {
    id: 'akaky',
    name: 'Акакий Куролесов',
    rarity: 'common',
    blurb: 'Безобидный обитатель Сумеречной рощи. Лазает по веткам, пугается шороха листвы. То ли человек, то ли недогриб.',
  },
  {
    id: 'scripach',
    name: 'Болотный Скрипач',
    rarity: 'secret',
    blurb: 'Маска, канифоль, обман. Ночью на болотах рощи заводит путников в сторону от Спектрального колодца.',
  },
  {
    id: 'spectral',
    name: 'Спектральный колодец',
    rarity: 'secret',
    blurb: 'Легенда Сумеречной рощи: появляется под одним и тем же деревом. Местонахождение прячут все зазеркальцы.',
  },
  {
    id: 'polenych',
    name: 'Поленыч',
    rarity: 'common',
    blurb: 'Преклонил колено — значит, ты добрый путник. Не путать с Коленычем: тот преклоняет полено и это опасно.',
  },
];

export function emptySave(): SaveData {
  return {
    kukichi: 0,
    pakichi: 0,
    bestScore: 0,
    matchesPlayed: 0,
    bestiary: { sator: true, fantasmagor: true },
  };
}
