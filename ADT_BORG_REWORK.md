# ADT Cyborg Rework (WikiHampter's Cyborg Rework)

## Обзор

Полный реворк системы киборгов для Space Station 14. Реализует модульную слотовую систему,
компонентный урон, управление законами, AI remote control, сменяемые визуальные субтайпы и HUD.

Автор: WikiHampter | MIT License

---

## 1. Архитектура

Реализация следует ECS-паттерну SS14 с разделением на Shared/Server/Client слои,
плюс YAML-прототипы и RSI-текстуры.

### 1.1 Слой Shared (`Content.Shared/ADT/Silicons/Borgs/`)

#### Компоненты

| Компонент | Описание |
|-----------|----------|
| `BorgComponent` | Маркер сущности борга. Отслеживает активный модуль, индекс слота, активность, блокировку, техпанель, мозг |
| `ADTBorgModuleComponent` | Данные модуля на entity. Содержит список предметов, emag-предметы, override-предметы, энергохранилища, радио-каналы, врождённые действия, броню, языки |
| `BorgSlotComponent` | 4 слота для предметов (MaxSlots: 4, Slot0-3). Текущий индекс, блокировка цикла, вайтлист модулей, макс. модулей |
| `BorgLawComponent` | Набор законов. ID прототипа, синхронизация, подключённый ИИ, нулевой закон/оверрайд |
| `BorgBatteryComponent` | Управление питанием. ID контейнера батареи, заряд, макс. заряд, зарядка, низкий заряд, потребление |
| `BorgWiresComponent` | Система взлома проводов. 4 провода: AiControl, Camera, LawCheck, Locked. Каждый с цветом, состоянием |
| `BorgSensorComponent` | Режимы сенсоров (Off/Meson/Thermal) |
| `BorgAiLinkComponent` | Связь с ИИ. Сущность ИИ, синхронизация законов, действие возврата к ИИ |
| `BorgEmagComponent` | Состояние EMAG-субверсии. Флаг emagged, mindslave-мастер, разблокировка оружия |
| `BorgUpgradeComponent` | Компонент апгрейда. Тип апгрейда, требуемый модуль, установлен |
| `BorgGripperComponent` | Схват для предметов. Использует UserActivateInWorldEvent для поднятия. Верб Drop Gripper Item |
| `BorgSelfRepairComponent` | Саморемонт. Включён, количество ремонта/тик, аккумулятор |
| `BorgRebootComponent` | Состояние перезагрузки. Перезагружается, время, длительность (15с), источник питания |
| `BorgAbilityComponent` | Общая способность (для YAML). Тип (Toggle/Instant), иконка, стоимость, активна |
| `BorgComponentPartComponent` | Повреждаемая часть тела. Тип части, brute/burn урон, макс. урон, сломана/установлена |

#### События

| Событие | Описание |
|---------|----------|
| `BorgModuleChangedEvent` | Модуль установлен/удалён |
| `BorgSlotChangedEvent` | Активный слот изменился |
| `BorgLawChangedEvent` | Законы изменились |
| `BorgWireActionEvent` | Провод разрезан/починен/проверен脉冲 |
| `BorgComponentPartDamagedEvent` | Часть повреждена/починена |
| `BorgEmagEvent` | EMAG применён |
| `BorgThrustersToggleEvent` | Двигатели включены/выключены |
| `BorgHeadlampToggleEvent` | Фонарик вкл/выкл |
| `BorgStateLawsEvent` | Огласить законы |
| `BorgSelfDiagnosisEvent` | Самодиагностика |
| `BorgSensorModeEvent` | Смена режима сенсоров |
| `BorgMagpulseToggleEvent` | Магпульс вкл/выкл |
| `BorgReturnToAiEvent` | Возврат разума в ИИ |

#### Прототипы данных

| Прототип | Описание |
|----------|----------|
| `BorgModuleTypePrototype` (`borgModuleType`) | Тип модуля с предметами, радио, бронёй, языками. 9 типов: Engineering, Medical, Security, Janitor, Miner, Service, Combat, Destroyer, Syndicate |
| `BorgLawSetPrototype` (`borgLawSet`) | Набор законов |
| `BorgSubtypePrototype` | Косметический субтайп: путь к спрайту, тип борга, состояния спрайта |

### 1.2 Слой Server (`Content.Server/ADT/Silicons/Borgs/`)

#### Системы (Core) — 19 систем

| Система | Описание |
|---------|----------|
| `BorgSystem` | Инициализация: спавнит 6 частей (Armour 100, Actuator 50, Radio 40, BinaryComm 30, Camera 40, DiagnosisUnit 30) |
| `BorgModuleSystem` | Установка/удаление модулей. Проверка вайтлиста, макс. модулей, borgModuleType прототипа. Заполнение слотов из модуля |
| `BorgSlotSystem` | Управление слотами (4). Активация/деактивация предметов при смене слота |
| `BorgLawSystem` | Синхронизация с ИИ, сброс при перезагрузке, оверрайд синдиката. 6 наборов законов |
| `BorgWiresSystem` | Эффекты от проводов: AiControl → отключение синхр. законов; Camera → нокдаун 1с при импульсе; LawCheck → отключение синхр.; Locked → переключение блокировки. Стандартный WiresSystem UI |
| `BorgAiLinkSystem` | При смерти/крите: возвращает разум в ИИ. TakeAiControl: передача разума. ReturnToAi: action возврата |
| `BorgAbilitiesSystem` | Реализация способностей: фонарик (5 энергии), сенсоры (Meson/Thermal/Off), магпульс (10 энергии), двигатели (25 энергии), законы (звук biamthelaw), диагностика (попап + UI) |
| `BorgBatterySystem` | Управление батареей: заряд/разряд, проверка заряда, низкий заряд |
| `BorgDeathSystem` | Смерть: деактивация, звук borg_deathsound.ogg, извлечение мозга. Гиб: извлечение мозга, удаление |
| `BorgDamageSystem` | Распределение урона: blunt/slash/pierce → brute, heat/shock → burn. Броня первая, затем случайные части |
| `BorgComponentPartSystem` | Урон/ремонт частей. Удаление случайной части при открытой панели |
| `BorgInteractionSystem` | Взаимодействия: панель (отвёртка), монтировка (отрывает часть), сварка (чинит), мультитул/кусачки → WiresSystem UI |
| `BorgEnergyRechargeSystem` | Периодическая подзарядка модулей от батареи борга |
| `BorgUpgradeSystem` | 16 типов апгрейдов. Все заполнены: Reset, Restart, Thrusters, SelfRepair, VTEC, WeaponsUnlock, DisablerCooler, DiamondDrill, SatchelOfHolding, Lavaproof, RCD, RPED, FloorBuffer, BluespaceTrashBag, RSFExecutive, ExpandedChassis |
| `BorgSelfRepairSystem` | Периодический саморемонт (каждые 3с, тратит 5 питания) |
| `BorgRebootSystem` | Машина состояний перезагрузки (15с) |
| `BorgEmagSystem` | Обработка EMAG: установка компонента, оверрайд законов синдиката |
| `BorgGripperSystem` | Захват предметов через UserActivateInWorldEvent. Верб Drop Gripper Item |
| `BorgInteractionSystem` | Wire hacking: мультитул/кусачки с открытой панелью → WiresSystem; с закрытой → блокируется |

#### Системы (вне Core)

| Система | Описание |
|---------|----------|
| `BorgInfoSystem` | Окно информации о борге (сервер) |
| `BorgSwitchableSubtypeSystem` | Обработка выбора субтайпа |
| `AiRemoteControlSystem` | Полный AI remote control: передача разума, перезапись законов, радио |

### 1.3 Слой Client (`Content.Client/ADT/Silicons/Borgs/`)

#### Системы

| Система | Описание |
|---------|----------|
| `BorgSystem` | Клиентские запросы информации о борге |
| `BorgHudSystem` | Жизненный цикл HUD-окна: открытие при аттаче, закрытие при детаче |
| `BorgSelfDiagnosisSystem` | Переключение окна самодиагностики |
| `BorgSwitchableSubtypeSystem` | Смена RSI и состояний спрайта при выборе субтайпа |
| `BorgVisualsSystem` | Визуальные эффекты: красный tint при эмаге, мигание глаз при низком заряде (<15%), серый цвет и пульсация при ребуте, подсветка активного слота |
| `BorgSlotSystem` | Клиентская работа со слотами: отслеживание смены активного слота, получение имени предмета в слоте, проверка установленного модуля |

#### UI

| Окно | Описание |
|------|----------|
| `BorgHudWindow` | 4 слота, прогресс-бар заряда, здоровье/статус, законы, модуль |
| `BorgSelfDiagnosisWindow` | Батарея, законы, целостность частей, состояние проводов |
| `BorgInfoWindow` | Заряд, спрайт (5x), модули, манифест экипажа, станция |
| `ChassisSpriteSelection` | Выбор субтайпа с превью спрайта |
| `BorgInfoModule` | Отображение одного модуля |

---

## 2. Механики

### 2.1 Система частей (Component Parts)

Борг состоит из 6 частей, каждая с запасом прочности:
- **Armour** (100 HP) — броня
- **Actuator** (50 HP) — актуатор
- **Radio** (40 HP) — радио
- **BinaryCommunication** (30 HP) — бин. связь
- **Camera** (40 HP) — камера
- **DiagnosisUnit** (30 HP) — диагностика

**Распределение урона**: blunt/slash/pierce → brute; heat/shock → burn.
Сначала урон идёт в броню, затем случайным образом в не-броневые части.
Когда суммарный урон превышает макс. здоровье — борт погибает.

**Ремонт**: сварка чинит все части. Монтировка с открытой панелью отрывает случайную часть.

### 2.2 Система модулей (Modules)

Каждый модуль (сущность с `ADTBorgModuleComponent`) при установке через `TryInstallModule()`:
1. Проверяет вайтлист, макс. модулей и существование `borgModuleType` прототипа
2. Создаёт предметы из `Items` (или `EmagItems`/`OverrideItems` в зависимости от состояния)
3. Заполняет слоты (4 штуки)
4. Настраивает энергохранилища, радио-каналы, броню, языки

**Типы модулей**: Engineering, Medical, Security, Janitor, Miner, Service, Combat, Destroyer, Syndicate.

### 2.3 Слоты (Slots)

4 слота для предметов (MaxSlots: 4, поля Slot0-Slot3).
Активный слот определяет, какой предмет в руке.
Циклический перебор слотов (CycleForward/CycleBackward).
При смене слота старый предмет деактивируется, новый активируется.

### 2.4 Законы (Laws)

Борг использует `BorgLawComponent` с набором законов.
6 предустановленных наборов: Asimov, Crewsimov, SyndicateOverride, Corporate, Robocop, Antimov.

- **Синхронизация с ИИ**: законы синхронизируются с подключённым ИИ
- **Оверрайд синдиката**: через EMAG или `ApplySyndicateOverride()`
- **Сброс при перезагрузке**: возврат к Crewsimov

### 2.5 Взлом проводов (Wire Hacking)

4 провода с уникальными цветами (случайными при инициализации).
Использует стандартный `WiresSystem` UI (мультитул/кусачки с открытой панелью).

| Провод | Cut | Mend | Pulse |
|--------|-----|------|-------|
| **AiControl** | Отключает синхр. законов | Включает синхр. законов | — |
| **Camera** | Отключает StationAiVision | Включает StationAiVision | Нокдаун 1с |
| **LawCheck** | Отключает синхр. законов | Включает синхр. законов | — |
| **Locked** | Блокирует борга | Разблокирует борга | Переключает блокировку |

### 2.6 EMAG

Применение EMAG:
1. Устанавливает `BorgEmagComponent`
2. Применяет оверрайд законов синдиката
3. Разблокирует emag-предметы в модулях
4. WeaponsUnlock апгрейд дополнительно разблокирует override-предметы

### 2.7 AI Remote Control

Полная система управления боргом через ИИ:
1. ИИ может взять управление: передача разума, перезапись законов (как нулевой), управление радио
2. При гибели/крите борга: разум возвращается в ядро ИИ
3. При недееспособности ИИ: разум возвращается
4. BUI-список удалённых устройств с MoveToDevice/TakeControl
5. ActionBorgReturnToAI: добровольный возврат разума

### 2.8 Питание (Battery)

- Батарея управляется через `BorgBatteryComponent`
- При вставке/извлечении элемента питания: обновление заряда
- `TryDrainPower()` — попытка потратить энергию
- `HasCharge()` — проверка наличия заряда
- `IsLowPower()` — низкий заряд (ниже порога)
- Периодическая подзарядка модулей от батареи (EnergyRechargeSystem)

### 2.9 Апгрейды (Upgrades)

17 типов апгрейдов. Все полностью реализованы:
- **Reset** — полный ремонт, очистка слотов/законов/emag/саморемонта
- **Restart** — перезагрузка (15с)
- **Thrusters** — ионные двигатели
- **SelfRepair** — саморемонт (периодический, тратит питание)
- **VTEC** — увеличение скорости на 1.5x
- **WeaponsUnlock** — разблокировка оружия (override)
- **DisablerCooler** — увеличение скорости стрельбы
- **DiamondDrill** — улучшение бура
- **SatchelOfHolding** — улучшение сумки
- **Lavaproof** — защита от лавы
- **RCD** — RCD апгрейд
- **RPED** — RPED апгрейд
- **FloorBuffer** — улучшение швабры
- **BluespaceTrashBag** — улучшение мешка
- **RSFExecutive** — улучшение сервиса
- **ExpandedChassis** — +2 слота модулей

### 2.10 Перезагрузка (Reboot)

Машина состояний:
1. `StartReboot()` — проверка питания, установка таймера
2. `FinishReboot()` — событие `BorgRebootEvent`, снятие стана, сброс законов
3. `CancelReboot()` — отмена

### 2.11 Саморемонт (Self-Repair)

Каждые 3 секунды:
1. Проверка: установлен ли `BorgSelfRepairComponent`
2. Тратит 5 единиц питания
3. Чинит все установленные не-сломанные части на 2 ед. × интервал

### 2.12 Субтайпы (Visual Subtypes)

Система `BorgSwitchableSubtype` позволяет менять внешний вид борга:
1. YAML-прототип `borgSubtype` определяет путь к RSI
2. При выборе субтайпа клиентская система загружает новый RSI
3. Устанавливает состояния для 4 слоёв: Body, Light (hasMind), Light (noMind), LightStatus
4. Все слои используют один и тот же RSI

### 2.13 SS13-субтайпы (конвертированные из DMI — 303+ RSI)

34 косметических субтайпа, конвертированных из SS13 `robots.dmi`:

| Серия | generic | security | medical | engineering | mining | janitor | service |
|-------|---------|----------|---------|-------------|--------|---------|---------|
| **Standard** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Noble** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Cricket** | — | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Rover** | — | — | ✅ | ✅ | — | ✅ | ✅ |

Индивидуальные: bloodhound (security), droid (generic), droid_medical, droid_mining,
heavy_sec (security), qualified_doctor_dmi (medical), squat_miner (mining),
repairbot (generic), xenoborg (generic), landmate (engineering),
old_generic (generic), tread_engineer, qualified_doctor, egg_security,
tread_security, bartender_service, brobot_service, female_service,
kerfus_service, kerfus_janitor.

Каждый RSI содержит состояния с 4 направлениями:
- `{type}` — тело (4 направления)
- `{type}_e` — глаза/свет с разумом
- `{type}_e_r` — глаза/свет без разума
- `{type}_l` — индикатор фонарика

Все сконвертированы в RSI-формат и размещены в `Resources/Textures/`:
- `ADT/Mobs/Silicon/Borgs/robots.rsi/` — 213 состояний (все спрайты из robots.dmi)
- `ADT/Mobs/Silicon/Borgs/robot_items.rsi/` — 11 состояний

### 2.14 Сборка киборга (Construction)

ADT использует собственную 7-стадийную систему сборки (не vanilla construction graph):

| Стадия | Действие | Материалы |
|--------|----------|-----------|
| **None** | Применить отвёртку к CyborgEndoskeleton | — |
| **Frame** | Вставить 6 частей с тегами (BorgLArm, BorgRArm, BorgLLeg, BorgRLeg, BorgTorso, BorgHead) | Конечности: 10 стали/шт, Торс: 50 стали, Голова: 50 стали |
| **LimbsInstalled** | Применить отвёртку | Нужен установленный кабель (CableCoil) |
| **ChestInstalled** | Применить отвёртку | Нужны 2 установленные вспышки (Flash) |
| **HeadInstalled** | Мультитул: циклическая настройка (LawSync/AiSync/Locomotion/PanelLock). Ручка: имя. | — |
| **Configuration** | Мультитул: настройка. Отвёртка: подтвердить. | — |
| **MMIInserted** | Вставить MMI/MMIRadioEnabled/RoboticBrain/PositronicBrain | — |
| **Complete** | Применить отвёртку. Борг готов. | — |

**Механизм материалов:**
- Игрок кликает по сборке стальными листами → сталь потребляется и накапливается в `SteelDeposited`
- Игрок кликает кабелем → потребляется 1 кабель, `CableInserted = true`
- Игрок кликает вспышкой → потребляется, `FlashesInserted` инкрементируется
- При вставке конечности: проверяется `SteelDeposited >= нужное_количество`
- При этапе Chest: проверяется `CableInserted`
- При этапе Head: проверяется `FlashesInserted >= 2`

**Деконструкция:**
1. Открыть крышку (screwdriver)
2. Вытащить батарею (crowbar, если крышка открыта)
3. Открыть провода (screwdriver, если батареи нет)
4. Перерезать ВСЕ провода (wirecutters)
5. Вытащить MMI (crowbar) — каркас разваливается, части выпадают

**Файлы:**
- `Content.Shared/ADT/Silicons/Borgs/Core/Components/BorgConstructionComponent.cs` — компонент + enum ConstructionStage
- `Content.Server/ADT/Silicons/Borgs/Core/Systems/BorgConstructionSystem.cs` — 375 строк, полная логика
- `Resources/Prototypes/Entities/Objects/Specific/Robotics/endoskeleton.yml` — CyborgEndoskeleton с BorgConstruction
- `Resources/Prototypes/ADT/Silicons/Borgs/borg_base.yml` — ADTBorgBase с BorgConstruction (для деконструкции)
- `Resources/Locale/*/ADT/silicons/borg-construction.ftl` — локализация
- `ADT/Interface/Hud/robot_hud.rsi/` — 73 состояния (HUD-иконки)
- `ADT/Objects/Specific/Robotics/parts.rsi/` — 13 состояний (части для сборки)
- `ADT/Objects/Specific/Robotics/components.rsi/` — 8 состояний (компоненты-органы)
- `ADT/Objects/Specific/Robotics/items.rsi/` — 24 состояния (инструменты)
- `ADT/Objects/Specific/Robotics/storage.rsi/` — 3 состояния

Все проверены: 345+ RSI состояний на диске, ни одной заглушки.

### 2.14 HUD

`BorgHudWindow` отображает:
- 4 слота с иконками предметов
- Прогресс-бар заряда
- Здоровье (агрегированный урон по частям)
- Статус (перезагрузка/отключён/заблокирован/активен)
- Имя активного модуля
- Текущий набор законов

### 2.15 Самодиагностика

`BorgSelfDiagnosisWindow` показывает:
- Системный статус: батарея (цветовая маркировка), законы (красный при оверрайде), блокировка
- Компоненты: список частей с процентом прочности (цветовая маркировка)
- Провода: статус (перерезан/цел) для каждого провода

### 2.16 Звуки

| Событие | Файл | Источник |
|---------|------|----------|
| Смерть | `/Audio/ADT/Borgs/borg_deathsound.ogg` | SS13 reference |
| Оглашение законов | `/Audio/ADT/Borgs/biamthelaw.ogg` | SS13 reference |
| Крик | `/Audio/ADT/Borgs/robot_scream.ogg` | SS13 goonstation |
| (fallback death) | `/Audio/ADT/Borgs/borg_deathsound_ref.ogg` | SS13 reference |

Звуковые коллекции: `ADTBorgDeath`, `ADTBorgScream`, `ADTBorgBiamTheLaw` (YAML: `Resources/Prototypes/ADT/SoundCollections/borg_sounds.yml`)
EmoteSounds: `BorgEmotes` (YAML: `Resources/Prototypes/ADT/Voice/speech_emote_sounds.yml`)

### 2.17 Клиентские визуалы

`BorgVisualsSystem` (Client) реализует визуальные эффекты на спрайтах борга:

| Эффект | Условие | Описание |
|--------|---------|----------|
| **Эмаг** | `BorgEmagComponent.Emagged == true` | Красный tint на слое `BorgVisualLayers.Body` |
| **Низкий заряд** | Заряд < 15% макс. | Мигание глаз (`BorgVisualLayers.Light`) оранжевым цветом, частота 4Hz |
| **Ребут** | `BorgRebootComponent.IsRebooting == true` | Серый цвет тела + пульсация глаз 6Hz |
| **Активный слот** | Смена `BorgSlotComponent.CurrentSlot` | Подсветка `BorgVisualLayers.LightStatus` пропорционально номеру слота |

### 2.18 Дополнительные ADT chassis

| Шасси | Описание | Модуль |
|-------|----------|--------|
| `ADTBorgChassisSec` | Security борт (BorgChassisSelectable) | security |
| `ADTBorgChassisKerfusNT` | Kerfus NT (BaseBorgChassisNT) | служба/медицина |
| `ADTBorgChassisSyndicateAssault` | Штурмовой синдикат (BaseBorgChassisSyndicate) | LMG, EnergySword, GrenadeFrag |
| `ADTBorgChassisSyndicateMedical` | Медик синдиката (BaseBorgChassisSyndicate) | EnergySword, Hypospray, BorgHypo |
| `ADTBorgChassisSyndicateSaboteur` | Саботажник синдиката (BaseBorgChassisSyndicate) | ChameleonProjector, RCD, EnergySword |
| `ADTBorgChassisDestroyer` | Destroyer (BorgChassisSelectable) | Stunbaton, WeaponDisabler, Handcuffs |

---

## 3. Структура прототипов

### 3.1 YAML-файлы

```
Resources/Prototypes/ADT/Silicons/Borgs/
├── borg_base.yml              # ADTBorgBase + наборы законов
├── abilities.yml              # 8 способностей (иконки из screen_robot.rsi)
├── borg_module_types.yml      # 9 типов модулей (borgModuleType)
├── borg_wires_layout.yml      # 4 провода для WiresSystem
├── Laws/                      # законы
├── Modules/
│   ├── engineering.yml        # 6 департаментных модулей
│   └── combat_modules.yml     # 6 боевых/синдикатских модулей
└── Upgrades/
    └── borg_upgrades.yml      # 17 апгрейдов

Resources/Prototypes/ADT/Entities/Mobs/Cyborgs/
├── borg_chassis.yml           # ADT chassis (Sec, Kerfus, 3x Syndicate, Destroyer)
├── borg_subtypes.yml          # Chassis субтайпов
├── borg_subtype.yml           # Прототипы субтайпов
├── borg_subtype_ss13.yml      # SS13 chassis
├── borg_subtypes_ss13.yml     # SS13 chassis subtypes
└── Core/
    └── borg_entities.yml      # 6 департаментных боргов

Resources/Prototypes/ADT/Actions/
└── borg_actions.yml           # ActionBorgHeadlamp, StateLaws, SelfDiagnosis,
                               # SensorMode, Magpulse, Thrusters, ReturnToAI
```

### 3.2 Текстуры

```
Resources/Textures/Interface/Actions/actions_borg.rsi/
  ├── 63 модульных иконки + state-laws, no-action, select-type
  ├── headlamp, self-diagnosis, sensor-mode, magpulse, thrusters (из screen_robot.rsi)
  ├── radio, pda (из screen_robot.rsi)
  └── meta.json (все состояния)

Resources/Textures/ADT/Interface/Actions/actions_borg.rsi/
  ├── rpd-module, salvage-module, state-info
  └── meta.json

Resources/Textures/ADT/Mobs/Silicon/Borgs/
├── cyborg_kerfus.rsi/         # Спрайты Kerfus NT (6 состояний, 4 направления)
└── Borg_subtype/
    ├── Generic/      (6 RSIs)
    ├── Engineering/  (6 RSIs)
    ├── Medical/      (7 RSIs)
    ├── Security/     (7 RSIs)
    ├── Mining/       (5 RSIs)
    ├── Janitor/      (5 RSIs)
    └── Service/      (8 RSIs)
    Всего: 44 RSI директории

Resources/Textures/ADT/Mobs/Silicon/Borgs/ (конвертированные из SS13 .dmi → .rsi)
├── robots.rsi/               # 213 состояний — все спрайты боргов из robots.dmi
└── robot_items.rsi/          # 11 состояний — предметы боргов

Resources/Textures/ADT/Interface/Hud/
└── robot_hud.rsi/            # 73 состояния — HUD иконки (screen_robot.dmi)

Resources/Textures/ADT/Objects/Specific/Robotics/
├── parts.rsi/                # 13 состояний — части для сборки
├── components.rsi/           # 8 состояний — компоненты-органы
├── items.rsi/                # 24 состояния — инструменты боргов
└── storage.rsi/              # 3 состояния — хранилища

_SS14_Borg_Port/SS13_Reference/icons/ (оригинальные экспортированные png)
├── mob/robots/               # 204 PNG — все спрайты боргов
├── mob/robot_items/          # 12 PNG — предметы
├── mob/screen_robot/         # 74 PNG — HUD
├── mob/screen_ai/            # AI HUD
├── mob/ai/                   # AI спрайты
├── mob/pai/                  # pAI спрайты
├── obj/items_cyborg/         # Инструменты
├── obj/robotics/             # Робототехника
├── obj/robot_component/      # Компоненты
├── obj/robot_parts/          # Части
├── obj/robot_storage/        # Хранилище
├── obj/module_ai/            # AI модули
└── obj/machines/ai_machinery/ # AI машины
```

### 3.3 Аудио

```
Resources/Audio/ADT/Borgs/
├── borg_deathsound.ogg        # Звук смерти (из SS13 reference)
├── borg_deathsound_ref.ogg    # Fallback звук смерти (дубль из SS13)
├── biamthelaw.ogg             # "I am the law" (из SS13 reference)
└── robot_scream.ogg           # Крик робота (из goonstation)

Resources/Prototypes/ADT/SoundCollections/
└── borg_sounds.yml            # Звуковые коллекции:
                               #   ADTBorgDeath     — borg_deathsound + ref
                               #   ADTBorgScream    — robot_scream + robotscream_1-3
                               #   ADTBorgBiamTheLaw — biamthelaw
```

---

## 4. Сборка и зависимости

### 4.1 Зависимости от существующей системы

Рework использует и расширяет существующую chassis-систему:
- `BorgChassisComponent` — базовый компонент
- `BorgChassisPrototype` — прототипы chassis
- `BorgSwitchableTypeSystem` — смена типа (из `Content.Shared/Silicons/Borgs/`)
- `BorgVisualLayers` (Body, Light, LightStatus)
- Существующие entity-прототипы: `BorgChassisGeneric`, `BorgChassisEngineer`, и т.д.
- `WiresSystem` / `WiresComponent` — стандартный UI проводов

### 4.2 Сборка

```
dotnet build Content.Server   → 0 errors
dotnet build Content.Client   → 0 errors
```

### 4.3 Известные LSP-ошибки (не блокируют сборку)

Это Roslyn LSP noise — не влияет на компиляцию:
- `AiRemoteControlSystem.cs` — IntrinsicRadioTransmitterComponent/ActiveRadioComponent не найдены LSP
- `BorgSystem.cs` — BorgRebootComponent неоднозначен (Server vs Shared)
- `BorgHudSystem.cs` — PlayerAttachedEvent.Session удалён
- `BorgAbilitiesSystem.cs` — HandheldLightComponent не найден LSP
- `BorgInteractionSystem.cs` — ActivateInHandEvent, SharedBorgSlotSystem, SharedBorgWiresSystem не найдены LSP

---

## 5. Граф зависимостей систем

```
SharedBorgSystem
  +-- SharedBorgSlotSystem (цикл слотов)
  +-- SharedBorgModuleSystem (данные модулей)
  +-- SharedBorgLawSystem (законы)
  +-- SharedBorgWiresSystem (провода)
  +-- SharedBorgAiLinkSystem (связь с ИИ)
  +-- SharedBorgAbilitiesSystem (способности, абстрактные)

Server (наследуют Shared, 19 систем):
  BorgSystem, BorgSlotSystem, BorgModuleSystem, BorgLawSystem,
  BorgWiresSystem, BorgAiLinkSystem, BorgAbilitiesSystem,
  BorgBatterySystem, BorgDeathSystem, BorgDamageSystem,
  BorgComponentPartSystem, BorgInteractionSystem,
  BorgEnergyRechargeSystem, BorgUpgradeSystem,
  BorgSelfRepairSystem, BorgRebootSystem, BorgEmagSystem,
  BorgGripperSystem, BorgConstructionSystem

Client (6 систем):
  BorgSystem (запросы), BorgHudSystem, BorgSelfDiagnosisSystem,
  BorgSwitchableSubtypeSystem (смена спрайтов),
  BorgVisualsSystem (визуальные эффекты: эмаг, низкий заряд, ребут, подсветка слота),
  BorgSlotSystem (клиентская работа со слотами)
```

---

## 6. Статус имплементации

| Компонент | Статус |
|-----------|--------|
| 19 серверных систем C# | ✅ |
| 7 shared систем C# | ✅ |
| 6 клиентских систем C# (HUD, SelfDiag, Subtype, Visuals, Slot) | ✅ |
| Wire hacking UI (WiresSystem) | ✅ |
| 17 апгрейдов (все заполнены) | ✅ |
| 9 borgModuleType prototypes | ✅ |
| 12+ модулей (items + chassis) | ✅ |
| 6 наборов законов | ✅ |
| AI Link + Return to AI | ✅ |
| EMAG | ✅ |
| Сборка/разборка (7 стадий + материалы) | ✅ |
| Gripper (chassis-level) | ✅ |
| 345+ борт RSI спрайты в Resources (40 ADT + 7 новых RSI-пакетов из SS13) | ✅ |
| 68 action иконок (с добавленными из screen_robot) | ✅ |
| Звуки (4 файла, 3 коллекции, BorgEmotes) | ✅ |
| Locale en-US + ru-RU (все строки) | ✅ |
| Entity ID модульных предметов (все корректны) | ✅ |
| abilities.yml (underscore→hyphen, radio+pda icons) | ✅ |
| Build: Server 0 err, Client 0 err | ✅ |

**Пропущено по плану:**
- Hunter module (не требуется)
- VOX (1338 файлов, не связано с боргами напрямую)

---

## 7. Исправления (Bug Fixes)

### 7.1 Actuator — замедление при поломке

**Проблема**: Actuator part спавнился с HP и мог сломаться, но никак не влиял на геймплей.

**Фикс**: Создана система `BorgPartEffectSystem`, которая подписывается на новые события `BorgComponentPartBrokenEvent` и `BorgComponentPartRepairedEvent`. При поломке Actuator — скорость движения снижается на 50%. При починке — восстанавливается.

**Файлы**:
- `Content.Shared/ADT/Silicons/Borgs/Core/Events/BorgComponentPartStateChangedEvent.cs` — новое событие `BorgComponentPartBrokenEvent`
- `Content.Shared/ADT/Silicons/Borgs/Core/Events/BorgComponentPartDamagedEvent.cs` — добавлен `BorgPartType PartType` в `BorgComponentPartRepairedEvent`
- `Content.Server/ADT/Silicons/Borgs/Core/Systems/BorgPartEffectSystem.cs` — новая система эффектов
- `Content.Server/ADT/Silicons/Borgs/Core/Systems/BorgComponentPartSystem.cs` — вызов событий при поломке/починке
- `Content.Shared/ADT/Silicons/Borgs/Core/Components/BorgComponentPartComponent.cs` — добавлен `OwnerBorg` для связи части с боргом
- `Content.Server/ADT/Silicons/Borgs/Core/Systems/BorgSystem.cs` — установка `OwnerBorg` при спавне частей

### 7.2 Camera — слепота при поломке

**Проблема**: Camera part не давала никакого эффекта при поломке.

**Фикс**: В `BorgPartEffectSystem` добавлен обработчик `HandleCameraBroken` — устанавливает `EyeDamage = MaxDamage` (полная слепота) через `BlindableSystem.AdjustEyeDamage`. При починке — сбрасывает урон и удаляет `BlindableComponent`.

### 7.3 Radio — отключение радио при поломке

**Проблема**: Radio part не влиял на работу радио.

**Фикс**: При поломке Radio part — удаляются `ActiveRadioComponent` и `IntrinsicRadioTransmitterComponent`. При починке — восстанавливаются с каналами Binary + Common.

### 7.4 BinaryCommunication — отключение бинарного канала

**Проблема**: BinaryCommunication part не отключал бинарный канал при поломке.

**Фикс**: При поломке — удаляется канал "Binary" из `ActiveRadio` и `IntrinsicRadioTransmitter`. При починке — канал возвращается.

### 7.5 Слияние borgLawSet в siliconLawset

**Проблема**: Две параллельные системы законов: `borgLawSet` (ADT-борги) и `siliconLawset` (ИИ/ванильные борги). Не синхронизированы, разный формат.

**Фикс**:
- Удалён `BorgLawSetPrototype.cs` — больше не нужен
- `BorgLawComponent.LawSet` изменён с `ProtoId<BorgLawSetPrototype>` на `ProtoId<SiliconLawsetPrototype>`
- `SharedBorgLawSystem.GetLaws()` теперь использует `SiliconLawsetPrototype` и `SiliconLawPrototype` через `IPrototypeManager`, резолвит текст через `Loc.GetString()`
- Добавлены новые `siliconLaw` прототипы: `ADTAsimov1-3`, `ADTSyndicateOverride1-4`
- Добавлены новые `siliconLawset` прототипы: `ADTAsimov`, `ADTSyndicateOverride`
- Добавлены локализации (ru-RU + en-US) для всех новых законов
- Удалены все `borgLawSet` YAML-определения из `borg_base.yml`
- `BorgLawSystem.ApplySyndicateOverride()` теперь использует `"ADTSyndicateOverride"`

**Файлы**:
- `Resources/Prototypes/ADT/silicon_laws.yml` — ADTAsimov, ADTSyndicateOverride
- `Resources/Locale/ru-RU/ADT/station-laws/laws.ftl` — локализация
- `Resources/Locale/en-US/station-laws/laws.ftl` — локализация

### 7.6 HoloStretcher — УДАЛЁН

Апгрейд HoloStretcher полностью удалён:
- Удалён прототип `ADTBorgUpgradeHoloStretcher`
- Удалён entity прототип `ADTHoloStretcher`
- Удалён `HandleHoloStretcher()` из `BorgUpgradeSystem`
- Удалены locale-строки

### 7.7 Construction — интеграция с construction graph

**Проблема**: `BorgConstructionSystem` (7-стадийная сборка) и construction graph `cyborg.yml` (Endoskeleton → BorgChassisSelectable) были двумя параллельными механизмами, не знающими друг о друге.

**Фикс**:
- Добавлен `BorgConstructionComponent` к `CyborgEndoskeleton` — теперь construction system работает на эндоскелете
- Добавлен `BorgConstructionComponent` к `ADTBorgBase` — деконструкция работает на ADT-боргах

**Файлы**:
- `Resources/Prototypes/Entities/Objects/Specific/Robotics/endoskeleton.yml`
- `Resources/Prototypes/ADT/Silicons/Borgs/borg_base.yml`

### 7.8 Fabricator — рецепты для component parts

**Проблема**: Component parts (Armour, Actuator, Radio, BinaryCommunication, Camera, DiagnosisUnit) нельзя было напечатать в экзофабе.

**Фикс**:
- Созданы 6 item-прототипов (`ADTBorgPartArmour`, `ADTBorgPartActuator`, `ADTBorgPartRadio`, `ADTBorgPartBinary`, `ADTBorgPartCamera`, `ADTBorgPartDiagnostics`) с компонентом `BorgComponentPart`
- Созданы 6 lathe-рецептов
- Создан пак `ADTBorgComponentParts`
- Пак добавлен в экзофаб

**Файлы**:
- `Resources/Prototypes/ADT/Entities/Objects/Specific/Robotics/borg_parts.yml` — новые прототипы
- `Resources/Prototypes/ADT/Recipes/Lathes/robotics.yml` — рецепты
- `Resources/Prototypes/ADT/Recipes/Lathes/Packs/robotics.yml` — пак
- `Resources/Prototypes/Entities/Structures/Machines/lathe.yml` — экзофаб
