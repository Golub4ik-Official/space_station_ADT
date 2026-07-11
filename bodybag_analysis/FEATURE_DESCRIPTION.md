# Описание новых мешков для тел, каталок и механик

## Типы мешков для тел

### 1. BodyBag (обычный синий)
- **Родитель:** BaseDeployFoldable + BasePaperLabelableVisualized
- **Вместимость:** 1
- **Размер в слоте:** Small
- **Замедление гниения:** ×2 (BodyBagRotSlow, DecayMultiplier = 0.5)
- **Прочее:** PaperLabel (подпись), Buckle, Pullable, Damageable, Destructible
- **Получение:** Только **крафт** коробки `BoxBodyBag` (5 ткани Cloth, 4 сек). На техфабе НЕ печатается.
- **Спрайты:** bag, bag_folded, open_overlay, paper

### 2. BodyBagBluespace — Блюспейс мешок для тела
- **Родитель:** BodyBag
- **Вместимость:** 15 существ (capacity: 15)
- **Размер в слоте:** Small (маленький)
- **Новый компонент:** `BluespaceBodyBag`
- **Ограничение складывания:** Нельзя свернуть, если внутри > 50% вместимости или есть другой контейнер (предотвращает рекурсию)
- **Динамический размер:** Размер в руке меняется в зависимости от самого тяжёлого предмета внутри (`UpdateWClass`)
- **Замедление гниения:** ×2 (наследует BodyBagRotSlow от BodyBag)
- **Прочее:** PaperLabel, BodyBagScanTarget (сканирование через мешок), Buckle, Pullable
- **Получение:** **MedicalTechFab** + исследование **ADTBluespaceChemistry** (Plastic:300, Silver:100, Bluespace:100)
- **Спрайты:** bluebodybag, bluebodybag_open, bluebodybag_folded

### 3. BodyBagEnvironmental — Защитный мешок для выживания
- **Родитель:** BodyBag
- **Вместимость:** 1
- **Размер в слоте:** Normal
- **Новый компонент:** `BodyBagGas` (режим: Breathable — заполняет мешок дыхательной смесью O₂+N₂ при закрытии)
- **Фича:** Герметичный (airtight: true), устойчив к пепельным бурям (`AshStormImmune`)
- **Замедление гниения:** ×2 (наследует BodyBagRotSlow от BodyBag)
- **Прочее:** PaperLabel, BodyBagScanTarget, Buckle, Pullable, StaticPrice 100
- **Получение:** **MedicalTechFab** + исследование **ADTAdvancedMedicalCare** (Plastic:500, Steel:200)
- **Спрайты:** envirobag, envirobag_open, envirobag_folded

### 4. BodyBagEnvironmentalNanotrasen — Элитный защитный мешок Nanotrasen
- **Родитель:** BodyBagEnvironmental
- **Описание:** Полная изоляция от внешних факторов
- **Замедление гниения:** ×2 (наследует BodyBagRotSlow от BodyBag через BodyBagEnvironmental)
- **StaticPrice:** 200
- **Получение:** **MedicalTechFab** + исследование **ADTAdvancedMedicalCare** (Plastic:500, Steel:500, Silver:200)
- **Спрайты:** ntenvirobag, ntenvirobag_open, ntenvirobag_folded

### 5. BodyBagPrisoner — Мешок для перевозки заключённых
- **Родитель:** BodyBagEnvironmental
- **Новый компонент:** `BodyBagCinch` — стяжки
- **Механика стяжек:**
  - Активация через глагол "Cinch bag/Uncinch bag"
  - Время затягивания: 10 секунд (DoAfter)
  - Затянутый мешок нельзя открыть изнутри (блокирует `StorageOpenAttemptEvent`)
  - Время взлома изнутри: 240 секунд (при конвертации в BreakoutTime)
  - Визуал: cinchedLayer (ремни на спрайте)
- **Замедление гниения:** ×2 (наследует BodyBagRotSlow от BodyBag)
- **StaticPrice:** 150
- **Получение:** **MedicalTechFab** + исследование **ADTAdvancedMedicalCare** (Plastic:500, Steel:300)
- **Спрайты:** prisonerenvirobag, prisonerenvirobag_open, prisonerenvirobag_folded, prisonerenvirobag_cinched

### 6. BodyBagSyndicate — Мешок Синдиката
- **Родитель:** BodyBagPrisoner
- **Новый компонент:** `BodyBagGas` (режим: N2O — заполняет усыпляющим газом N₂O при закрытии)
- **Фича:** Имеет стяжки (BodyBagCinch)
- **Замедление гниения:** ×2 (наследует BodyBagRotSlow от BodyBag)
- **StaticPrice:** 250
- **Получение:** Только **аплинк** — коробка `BoxBodyBagSyndicate` за **4 телекристалла**. На техфабе НЕ печатается.
- **Спрайты:** syndieenvirobag, syndieenvirobag_open, syndieenvirobag_folded, syndieenvirobag_cinched

### 7. BodyBagStasis — Стазис-мешок (одноразовый)
- **Родитель:** BodyBag
- **Вместимость:** 1
- **Новый компонент:** `StasisBodyBag` + `StasisBodyBagOccupantComponent`
- **Механика стазиса:**
  - При закрытии: температура внутри опускается до -60°C
  - Через ~3 секунды: тушит горящие тела (`StasisBodyBagFireExtinguishEvent`)
  - Через ~5 секунд: активирует стазис — замедление метаболизма в 10× (метаболический мультипликатор)
  - Со временем накапливает повреждения (0.33 урона/сек, max 300 integrity)
  - При открытии: сброс всех таймеров и повреждений
  - Одноразовый: при разрушении (40 урона) выпадает Shreds + SheetPlastic
- **Осмотр:** Показывает степень износа при integrity < 75%
- **Гниение:** Полностью остановлено (`AntiRottingContainer`)
- **Прочее:** PaperLabel, BodyBagScanTarget, Buckle, Pullable
- **Звуки:** spray.ogg (открытие/закрытие)
- **Получение:** **MedicalTechFab** + исследование **ADTCryoTech** (Plastic:500, Silver:200)
- **Спрайты:** holobag_med, holobag_med_open, stasis_bag_folded

### 8. BodyBagLostCrew — Мешок Потерянного экипажа
- **Родитель:** BodyBag
- **Вместимость:** 15
- **Новый компонент:** `LostCrewBodyBag`
- **Механика:**
  - При первом открытии: спавнит внутри тело случайного гуманоида (`SalvageHumanCorpse`)
  - Спавнит случайный лут из таблицы `LostCrewLootTable` (медицинские принадлежности, инструменты, еда, сигареты)
  - Спавнит случайный запертый ящик из `LostCrewLockboxTable` (Engineering/Medical/Security/Science/Service)
  - Флаг `SpawnBodyOnMapInit` — для премапленных тел в мешках
- **StaticPrice:** 50
- **Гниение:** Полностью остановлено (`AntiRottingContainer`)
- **Специальная вариация:** `BodyBagLostCrewWithBody` — с телом при инициализации карты
- **Прочее:** PaperLabel, BodyBagScanTarget
- **Получение:** Только **карго** (CrateLostCrew, 1500 кредитов) или на **астероидах/заброшенных структурах**. На техфабе НЕ печатается.
- **Спрайты:** bodybag_lost, bodybag_lost_open, bodybag_lost_folded
- **Лут (LostCrewLootTable):**
  - Weight 5: разнообразные аптечки
  - Weight 4: баночки с таблетками, HealthAnalyzer
  - Weight 2: инструменты (crowbar, wrench, screwdriver и т.д.)
  - Weight 2: еда/вода
  - Weight 1: солнцезащитные очки, сигара, зажигалка
  - Weight 1: коробка мелков

## Новые каталки

### 1. HoloStretcher — Голографическая каталка
- **Родитель:** BaseItem + BaseDeployFoldable
- **Размер в слоте:** Small (помещается в карман!)
- **Прочность:** 100 HP (разрушение), 50 HP (частичное — выпадает 1 лист стали)
- **Время пристёгивания:** 0.25 секунды
- **Новый компонент:** `BodyBagStretcher` (позволяет класть сложенные мешки на каталку с DoAfter)
- **Отображение:** drawdepth: DeadMobs
- **Физика:** MovedByPressure, DamageOnHighSpeedImpact, InjectorBoost
- **Получение:** **MedicalTechFab** + исследование **ADTHoloRollerBed** (Steel:1000, Silver:500, Glass:500, Diamond:200; время: 4 сек)
- **Исследование:** ADTHoloRollerBed (Biochemical, tier 3, cost: 7500, requires: ADTCryoTech)
- **Спрайты:** ADT/Structures/Furniture/holo_stretcher.rsi

### 2. Изменения обычных каталок (RollerBed)
- **Изменён размер в слоте:** Normal → **Huge** (не помещается в рюкзак)
- **Добавлен компонент:** `RollerBedPullSlow` (замедление при буксировке)
- **Добавлен компонент:** `BodyBagStretcher` (можно класть мешки)
- **Добавлен drawdepth:** DeadMobs
- **Добавлен** `buckleDoafterTime: 2` (пристёгивание с прогресс-баром)
- **Аналогичные изменения:** CheapRollerBed, EmergencyRollerBed

## Новые общие механики

### 1. BodyBagStretcher — Размещение мешков на каталках
- **Система:** `BodyBagStretcherSystem` (Shared)
- **Действие:** Использовать сложенный мешок для тела на развёрнутой каталке
- **Время doAfter:** берётся из `Strap.BuckleDoafterTime` (для обычной каталки: 2 сек, для голо: 0.25 сек)
- **Процесс:** DoAfter → раскладывание мешка на каталке → пристёгивание мешка к каталке
- **Защита от складывания:** Если на каталке лежит мешок — её нельзя сложить (`OnBodyBagFoldAttempt`)
- **Защита от складывания мешка:** Если мешок пристёгнут к каталке — его нельзя свернуть (`OnBuckleFoldAttempt`)

### 2. RollerBedPullSlow — Замедление буксировки каталок
- **Компонент:** `RollerBedPullSlowComponent`
- **Параметры:** WalkSpeedModifier = 0.7, SprintSpeedModifier = 0.8
- **Условие:** Замедление работает ТОЛЬКО если на каталке кто-то есть (buckled occupant или закрытый мешок с содержимым)
- **Система:** `RollerBedPullSlowSystem` — обновляет скорость буксирующего при посадке/высадке

### 3. BodyBagPullSlow — Замедление буксировки мешков
- **Где:** `PullingSystem.cs` (в `OnRefreshMovespeed`)
- **Параметры:** WalkSpeedModifier = 0.55, SprintSpeedModifier = 0.65
- **Условие:** Если тащишь закрытый мешок для тела с occupant'ом
- **Не суммируется:** с RollerBedPullSlow (проверка приоритета)

### 4. BodyBagCinch — Стяжки на мешках
- **Компонент:** `BodyBagCinchComponent`
- **Время затягивания:** 10 секунд (CinchTime)
- **Время взлома изнутри:** 240 секунд (BreakoutTime)
- **Визуал:** `BodyBagCinchVisuals.Cinched` — слой cinchedLayer
- **Блокировка открытия:** Затянутый мешок нельзя открыть (даже изнутри)
- **Звук затягивания:** toolbelt_equip.ogg

### 5. BodyBagGas — Заполнение газом
- **Компонент:** `BodyBagGasComponent`
- **Режимы:**
  - `Breathable` — заполняет O₂ + N₂ (дыхательная смесь) — Environmental
  - `N2O` — заполняет O₂ + N₂O (усыпляющий газ) — Syndicate
- **Срабатывает:** При закрытии мешка (`StorageAfterCloseEvent`)
- **Давление:** 1 атм, 20°C

### 6. BodyBagScanTarget — Сканирование через мешок
- **Компонент:** `BodyBagScanTargetComponent`
- **Система:** `BodyBagScanSystem` (Server)
- **Действие:** Использовать HealthAnalyzer на закрытом мешке → сканирует находящегося внутри моба
- **Проверка:** LOS от пользователя до цели (range: 3)
- **Применение:** На всех мешках, кроме обычного BodyBag

### 7. PaperLabel — Подписи и срезание
- **Система:** `PaperLabelBodyBagSystem` (Shared)
- **При открытии мешка:** если есть прикреплённая бумажка — она выпадает рядом
- **Система:** `PaperLabelCutSystem` (Shared) — срезание бумажки кусачками
- **Действие:** Использовать кусачки (CutQuality) на мешке → бумажка отваливается
- **Звук срезания:** wirecutter.ogg

### 8. MorgueBodyBag — Отображение подписи в морге
- **Система:** `MorgueBodyBagSystem` (Server)
- **При закрытии морга:** если внутри есть мешок с подписью — название морга обновляется на `"Морг (подпись)"`
- **При открытии морга:** название возвращается к прототипному

### 9. BodyBagDisposal — Утилизация в мусоропровод
- **Система:** `BodyBagDisposalSystem` (Server)
- **Действие:** Перетащить пустой развёрнутый мешок на мусоропровод
- **Автоматически:** складывает мешок и отправляет в утиль
- **Предупреждение:** Если мешок не пуст — выводит сообщение "Fold the body bag first!"

### 10. Shreds — Остатки мешка
- **Новый предмет:** при разрушении стазис-мешка выпадают обрывки (мусор)
- **Родитель:** BaseItem (size: Tiny)
- **Спрайт:** plastic (из материалов)
- **Тег:** Trash

### 11. Buckle-изменения (DrawDepth)
- **Изменение:** Пристёгнутые к каталке мобы/мешки рисуются ПОВЕРХ каталки (was -1, now +1)
- **Файлы:** `BuckleSystem.cs`, `SharedBuckleSystem.Buckle.cs`
- **Для стретчеров:** всегда показывать прогресс-бар DoAfter при пристёгивании

## Изменения RollerBed

- Размер `Item` изменён с `Normal` на `Huge`
- Добавлен `RollerBedPullSlow`
- Добавлен `BodyBagStretcher`
- DrawDepth изменён на `DeadMobs`
- Добавлен `buckleDoafterTime: 2`

## Прототипы новых предметов

### Коробки (Boxes)
- `BoxBodyBag` — 4 обычных синих мешка (через крафт, 5 ткани)
- `BoxBodyBagBluespace` — 4 блюспейс мешка
- `BoxBodyBagEnvironmental` — 4 защитных мешка
- `BoxBodyBagPrisoner` — 4 мешка для заключённых
- `BoxBodyBagStasis` — 4 стазис-мешка
- `BoxBodyBagEnvironmentalNanotrasen` — 4 элитных защитных мешка
- `BoxBodyBagSyndicate` — 4 синдикатовских мешка (аплинк, 4 ТК)
- `BoxBodyBagLostCrew` — 4 мешка потерянного экипажа

### Крафт
- `BoxBodyBag` (construction) — можно скрафтить из **5 ткани** (Cloth), 4 секунды

### Карго
- `CrateLostCrew` — ящик с 2 коробками мешков LostCrew, цена 1500 кредитов

### Аплинк
- `BoxBodyBagSyndicate` — коробка 4 мешков Синдиката, **4 телекристалла**

### MedicalTechFab — схема получения (динамические паки)

| Мешок | Технология | Пак |
|---|---|---|
| `BodyBagBluespace` | ADTBluespaceChemistry | `Chemistry` |
| `BodyBagEnvironmental` | ADTAdvancedMedicalCare | `ADTAdvancedMedicalCarePack` |
| `BodyBagEnvironmentalNanotrasen` | ADTAdvancedMedicalCare | `ADTAdvancedMedicalCarePack` |
| `BodyBagPrisoner` | ADTAdvancedMedicalCare | `ADTAdvancedMedicalCarePack` |
| `BodyBagStasis` | ADTCryoTech | `ADTCryoTech` |

**Не печатаются на техфабе:**
- `BodyBag` (обычный) — только крафт
- `BodyBagLostCrew` — только карго / астероиды
- `BodyBagSyndicate` — только аплинк

### 12. BodyBagRotSlow — Замедление гниения в мешках

- **Компонент:** `BodyBagRotSlowComponent` (добавлен на базовый `BodyBag`)
- **Эффект:** Мобы внутри мешка гниют в **2 раза** медленнее (DecayMultiplier = 0.5)
- **Наследуется:** всеми типами мешков, кроме Stasis и LostCrew (у них `AntiRottingContainer` — полная остановка)
- **Морг + Мешок:** Если тело находится в мешке, который лежит внутри морга (`MorgueComponent`) — гниение полностью останавливается
- **Цепочка проверки:** `IsRotProgressing()` → есть ли вокруг мешка (BodyBagRotSlow), есть ли вокруг мешка морг (MorgueComponent) → stop
- **Rate:** `GetRotRate()` → множитель 0.5 для скорости гниения

## Визуальные спрайты

Добавлены спрайты:
- **bodybags.rsi:** bluebodybag, envirobag, ntenvirobag, prisonerenvirobag, syndieenvirobag, holobag_med, holobag_sec, bodybag_lost, stasis_bag_folded, bodybag_label, paper_written (+ варианты open/folded/cinched)
- **holo_stretcher.rsi:** новый набор спрайтов для голо-каталки
- **Звуки:** duct_tape_rip.ogg, toolbelt_equip.ogg
