# Зазеркалье — Unity (URP)

Короткие PvE-сессии **2–5 мин** в **Сумеречной роще** для Яндекс Игр.  
Вид от третьего лица. Тон: сюрреализм кассетной эпохи, **не хоррор**.

Phaser-прототип сохранён в `legacy/phaser-beta/` только как референс.

## Требования

- **Unity 6** (`6000.0.36f1` или совместимая 6000.0.x)
- Universal Render Pipeline (пакет в `Packages/manifest.json`)

В этой cloud-среде **нет Unity Editor** — WebGL здесь не собирается. Откройте проект локально.

## Запуск

1. Откройте папку репозитория в Unity Hub → Open.
2. Дождитесь импорта пакетов.
3. Меню **Зазеркалье → Configure URP** (если розовый/серый экран без пайплайна).
4. Откройте сцену `Assets/Scenes/Bootstrap.unity` и нажмите Play.

Игра также поднимается через `GameBootstrap` (runtime initialize), даже из почти пустой сцены.

### Управление

| Клавиша | Действие |
|---------|----------|
| WASD / стрелки | Ходьба |
| Пробел / ЛКМ | Кок |
| Shift | Рывок (i-frames) |

## Геймплей (вертикальный срез)

- Цель: собрать **3 следа** пропавшего пацаноида за ~2.5 мин
- Кокайте бобылей / Чёрных Эклеров, комбо и счёт
- Кукичи, пауэр-апы (магнит / ярость / ускорение / щит / пир)
- Ночь (~60 с до конца): **жвачники**
- Болота замедляют
- Секреты: зеркало **Истукануса**, **Спектральный колодец**
- Меню → матч → итог (мок рекламы) → бестиарий

Лор сверстан с [zazerwiki.com](https://zazerwiki.com/).

## Визуал

Приглушённая сумеречная роща: туман, тёплый key + холодный fill, простые меши, спокойный UI — без CRT/конфетного Phaser-вайба.

## Сборка WebGL для Яндекс Игр

1. File → Build Settings → WebGL → Switch Platform.
2. Player Settings: сжатие **Gzip** или **Brotli**, включите decompression fallback при необходимости.
3. Build в папку `Build/WebGL`.
4. В итоговом `index.html` подключите SDK:

```html
<script src="https://yandex.ru/games/sdk/v2"></script>
```

Плагин `Assets/Plugins/WebGL/YandexGames.jslib` + `YandexGamesSdk` — ready / interstitial / rewarded.  
В Editor работает **mock** (логи в Console).

## Структура

```
Assets/Scripts/Core      — bootstrap, матч
Assets/Scripts/Player    — 3rd person, камера
Assets/Scripts/Enemies   — бобыли / жвачники
Assets/Scripts/World     — роща, пикапы, секреты
Assets/Scripts/UI        — меню / HUD / итог / бестиарий
Assets/Scripts/Yandex    — мост YaGames
legacy/phaser-beta       — старый веб-прототип
docs/                    — дизайн-заметки
```

## Документы

- `docs/ZAZERKALYE_BETA_CONCLUSION.md` — дизайн-заключение беты
- `docs/LORE_WIKI_ALIGN.md` — сверка с wiki
- `docs/VIDEO_CATALOG.md` — разбор референс-видео
