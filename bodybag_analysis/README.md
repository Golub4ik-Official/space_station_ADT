# Анализ системы мешков для трупов (Body Bags) в tgstation

## Общая архитектура

Система построена на двух типах объектов, переключаемых через свёртывание/развёртывание:

- **`/obj/item/bodybag`** — сложенный мешок (предмет, хранится в инвентаре)
- **`/obj/structure/closet/body_bag`** — развёрнутый мешок (структура, можно положить тело)

Они соединяются через переменные `unfoldedbag_path` (у предмета) и `foldedbag_path` (у структуры). При развёртывании структура сохраняет ссылку на предмет через `foldedbag_instance`, что позволяет свернуть мешок обратно в тот же самый предмет.

---

## Типы мешков

### 1. Обычный мешок

- **Структура:** `/obj/structure/closet/body_bag` — "body bag" — _"A plastic bag designed for the storage and transportation of cadavers."_
- **Предмет:** `/obj/item/bodybag` — "body bag" — _"A folded bag designed for the storage and transportation of cadavers."_
- Вмещает **1 моб**
- Можно подписать ручкой (`tag_name`)
- Можно прикрепить бумажку (`pinned`)
- Сворачивается (если пуст)
- Шум застёгивания: `sound/items/zip/zip.ogg`
- При разрушении выпадает: **ткань ×2 листа** (`material_drop_amount = 2` по умолчанию)
- Входящие мобы получают `TRAIT_FLOORED`
- **Протолат:** нет рецепта
- **Крафт:** 1 лист ткани → коробка с 7 мешками

### 2. Блюспейс мешок

- **Структура:** `/obj/structure/closet/body_bag/bluespace` — "bluespace body bag" — _"A bluespace body bag designed for the storage and transportation of cadavers."_
- **Предмет:** `/obj/item/bodybag/bluespace` — "bluespace body bag" — _"A folded bluespace body bag designed for the storage and transportation of cadavers."_
- Вмещает **15 мобов**
- При сворачивании все содержимое перемещается внутрь предмета (в т.ч. живые мобы)
- Нельзя свернуть если внутри >50% вместимости, если внутри блюспейс-мешок (рекурсия), или если внутри сам сворачивающий
- Предмет сохраняет `w_class` на основе самого тяжёлого содержимого
- Исследуется в техвебе `"bluespacebodybag"` (Tier 2, Bluespace Research) (Для моего сервер ADT SS14 нужно добавить его в Biochemical Блюспейс-химия (по хорошему переименовать технологию в Блюспейс-медицина).
  )
- **Протолат:** железо ×150 (1.5 листа) + плазма ×100 (1 лист) + алмаз ×50 (0.5 листа) + блюспейс ×50 (0.5 листа)
- Можно пожарить во фритюрнице (глупый мем)

### 3. Экологический мешок

- **Структура:** `/obj/structure/closet/body_bag/environmental` — "environmental protection bag" — _"An insulated, reinforced bag designed to protect against exoplanetary storms and other environmental factors."_
- **Предмет:** `/obj/item/bodybag/environmental` — "environmental protection bag" — _"A folded, reinforced bag designed to protect against exoplanetary environmental storms."_
- Защищает от погодных условий: ashstorm, radstorm, snowstorm
- Вмещает **1 моба**
- `contents_pressure_protection = 0.8`
- `contents_thermal_insulation = 0.5`
- Содержит внутренний запас воздуха (кислород + азот, 50 литров, 20°C)
- Поддерживает API `return_air()`, `remove_air()`, `return_analyzable_air()`

#### 3a. Elite / Nanotrasen

- **Структура:** `/obj/structure/closet/body_bag/environmental/nanotrasen` — "elite environmental protection bag" — _"A heavily reinforced and insulated bag, capable of fully isolating its contents from external factors."_
- **Предмет:** `/obj/item/bodybag/environmental/nanotrasen` — "elite environmental protection bag" — _"A folded, heavily reinforced, and insulated bag, capable of fully isolating its contents from external factors."_
- Полная изоляция: `contents_pressure_protection = 1`, `contents_thermal_insulation = 1`
- `TRAIT_WEATHER_IMMUNE`

#### 3b. Тюремный мешок

- **Структура:** `/obj/structure/closet/body_bag/environmental/prisoner` — "prisoner transport bag" — _"Intended for transport of prisoners through hazardous environments, this environmental protection bag comes with straps to keep an occupant secure."_
- **Предмет:** `/obj/item/bodybag/environmental/prisoner` — "prisoner transport bag" — _"Intended for transport of prisoners through hazardous environments, this folded environmental protection bag comes with straps to keep an occupant secure."_
- Имеет ремни (`cinched`), которые блокируют открытие
- Время застёгивания: 10 секунд
- `breakout_time = 4 MINUTES`
- При попытке открыть застёгнутый мешок — ремни блокируют
- Побег через `container_resist_act` с проверкой на `cinched`
- `bust_open()` снимает ремни и открывает

#### 3c. Синдикатовский

- **Структура:** `/obj/structure/closet/body_bag/environmental/prisoner/pressurized/syndicate` — "syndicate prisoner transport bag" — _"An alteration of Nanotrasen's environmental protection bag which has been used in several high-profile kidnappings. Designed to keep a victim unconscious, alive, and secured during transport."_
- **Предмет:** `/obj/item/bodybag/environmental/prisoner/syndicate` — "syndicate prisoner transport bag" — _"An alteration of Nanotrasen's environmental protection bag which has been used in several high-profile kidnappings. Designed to keep a victim unconscious, alive, and secured until they are transported to a required location."_
- Полная изоляция как у Nanotrasen
- Внутри вместо азота — **закись азота (N2O)** — усыпляет жертву
- `breakout_time = 8 MINUTES` (в 2 раза дольше)
- `cinch_time = 20 SECONDS` (в 2 раза дольше)

#### 3d. Хардлайт-мешок

- **Структура:** `/obj/structure/closet/body_bag/environmental/hardlight` — "hardlight body bag" — _"A hardlight bag for storing bodies. Resistant to space."_
- `foldedbag_path = null` — не сворачивается (создаётся MOD-модулем)
- `resistance_flags = LAVA_PROOF | FIRE_PROOF | ACID_PROOF`
- Можно сканировать через хелс-анализатор (`can_scan_through = TRUE`)
- Свой звук атаки: `egloves.ogg`
- Используется в MOD `patienttransport` модуле

#### 3e. Хардлайт-тюремный

- **Структура:** `/obj/structure/closet/body_bag/environmental/prisoner/hardlight` — "hardlight prisoner body bag" — _"A hardlight bag for storing bodies. Resistant to space, can be cinched to prevent escape."_
- Комбинация тюремного и хардлайт
- Используется в MOD `criminalcapture` модуле

### 4. Стазис-мешок

- **Структура:** `/obj/structure/closet/body_bag/environmental/stasis` — "stasis body bag" — _"A disposable bodybag designed to keep its contents in stasis, preventing decay and further injury. The bag itself cannot maintain stasis for long, and will eventually fall apart."_
- **Предмет:** `/obj/item/bodybag/stasis` — (те же name/desc, наследуются через атрибуты: `name = /obj/structure/closet/body_bag/environmental/stasis::name`)
- Временный мешок, который саморазрушается
- `max_integrity = 300`, самоуничтожается через процесс (~2 минуты)
- **Стазис**: после 3 секунд тушит огонь, после 5 секунд накладывает `TRAIT_STASIS`
- Замораживает органы (`ORGAN_FROZEN`)
- Внутренняя температура: -60°C
- Визуальный эффект: цветовой фильтр меняется по мере повреждения
- При разрушении выпадает: `shreds` + **пластик ×2 листа** (`material_drop = /obj/item/stack/sheet/plastic`)
- `breakout_time = 5 SECONDS` (легко выбраться из стазиса)
- При открытии/закрытии звук: `spray.ogg` (пониженная частота)
- Исследуется в техвебе `"stasis_bodybag"` (Tier 4, Cryostasis) (Для моего сервер ADT SS14 нужно добавить его в Biochemical Криогенный стазис)
- **Протолат:** пластик ×1000 (10 листов) + серебро ×50 (0.5 листа)

### 5. Lost Crew мешок

- **Структура:** `/obj/structure/closet/body_bag/lost_crew` — "long-term body bag" — _"A plastic bag designed for the long-term storage and transportation of cadavers."_
- **Предмет:** `/obj/item/bodybag/lost_crew` — "long-term body bag" — _"A folded bag designed for the long-term storage and transportation of cadavers."_
- Для механики "потерянного экипажа" (долгосрочное хранение)
- При открытии создаёт рандомное тело с историей травм
- Содержит предметы восстановленные с тела + lockbox с защищёнными предметами
- `with_body` подтип: создаёт тело в `PopulateContents()`, нельзя свернуть пока тело не извлечено
- Продаётся через карго (pack `lost_crew`)
  ДЛЯ МОЕГО СЕРВЕРА ADT SS14 НУЖНО ПЕРЕДЕЛАТЬ ЭТОТ МЕШОК В КАЧЕСТВЕ МЕШКА ДЛЯ ДЛИТЕЛЬНОГО ХРАНЕНИЕ, ЗАМЕДЛЯТЬ ГНИЕНИЕ ЕСЛИ МЕШОК В БОРГЕ

---

## Основные механики

### Сворачивание/развёртывание

- **Развёртывание**: `attack_self()` или `interact_with_atom()` на тайле
- **Сворачивание**: ПКМ по развёрнутому мешку → `attempt_fold()` → `perform_fold()` → `undeploy_bodybag()` → `qdel(структура)`
- `undeploy_bodybag()` возвращает `obj/item/bodybag` (либо существующий `foldedbag_instance`, либо новый)

### Подпись и бумажка

- Ручкой: `nameformat()` → `tag_name`, оверлей `bodybag_label`
- Бумажка: `item_interaction()` с `/obj/item/paper` → `pinned`, оверлей `paper`/`paper_written`
- Открытие мешка с бумажкой: `before_open()` вынимает бумажку в свободную руку
- Срезание метки: wirecutter или острый предмет

### Взаимодействие с моргом

- `morgue.dm` отслеживает `Entered`/`Exited` body_bag → обновляет название tray на основе `tag_name`
- `tray` принимает body_bag через `mouse_drop_receive()`

### Взаимодействие с мусоропроводом

- `disposal.dm` позволяет запихнуть мешок (`stuff_bodybag_in()`)
- Пустой мешок сворачивается и уничтожается
- Полный мешок помещается внутрь disposal целиком
- Сопротивление внутри disposal развёртывает мешок

### MOD-модули

- **Patient Transport** (`modules_medical.dm`): создаёт хардлайт-мешок (медицинский)
- **Criminal Capture** (`modules_security.dm`): создаёт хардлайт-тюремный мешок, отслеживает дистанцию, дематериализует через 0.5 секунд с анимацией

### Крафт

- **1 лист ткани** (cloth sheet) → коробка с 7 мешками (`/obj/item/storage/box/bodybags`)
- Рецепт: `body bag box` в категории CAT_CONTAINERS

### Админ-инструменты

- `admingame.dm`: поиск мобов внутри body_bag (включая свёрнутые блюспейс-мешки внутри других контейнеров)

---

## Файлы

### Основной код

| Файл                                                                         | Описание                                                                       |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| `code/game/objects/items/tools/medical/bodybag.dm`                           | Предмет (сложенный мешок) — `/obj/item/bodybag` и все его подтипы              |
| `code/game/objects/structures/crates_lockers/closets/bodybag.dm`             | Структура (развёрнутый мешок) — `/obj/structure/closet/body_bag` и все подтипы |
| `code/modules/lost_crew/body_bags.dm`                                        | Lost crew мешки                                                                |
| `code/game/objects/items/storage/boxes/medical_boxes.dm` (стр. 96-104)       | Коробка с мешками                                                              |
| `code/game/objects/items/stacks/sheets/sheet_types.dm` (стр. 695)            | Крафт коробки из ткани                                                         |
| `code/modules/research/designs/medical_designs.dm` (стр. 155-179)            | Дизайны для протолата                                                          |
| `code/modules/research/techweb/nodes/research_nodes.dm` (стр. 48)            | Техвеб-нода блюспейс мешка                                                     |
| `code/modules/research/techweb/nodes/medbay_nodes.dm` (стр. 105)             | Техвеб-нода стазис мешка                                                       |
| `code/game/objects/structures/morgue.dm` (стр. 337-360, 580-589)             | Интеграция с моргом                                                            |
| `code/modules/recycling/disposal/bin.dm` (стр. 195-263)                      | Интеграция с мусоропроводом                                                    |
| `code/modules/mapping/mapping_helpers.dm` (стр. 959-967)                     | Хелпер для карт                                                                |
| `code/modules/mod/modules/modules_medical.dm` (стр. 220-233)                 | MOD patient transport                                                          |
| `code/modules/mod/modules/modules_security.dm` (стр. 206-267)                | MOD criminal capture                                                           |
| `code/modules/antagonists/pirate/pirate_roles.dm` (стр. 199-210)             | Пиратский спальник (использует спрайт bodybag)                                 |
| `code/game/objects/effects/spawners/random/medical.dm` (стр. 145)            | Рандомный спавн                                                                |
| `code/_globalvars/lists/maintenance_loot.dm` (стр. 146)                      | Лут в техтоннелях                                                              |
| `code/modules/cargo/packs/medical.dm` (стр. 103, 215)                        | Карго-заказы                                                                   |
| `code/modules/vending/wardrobes.dm` (стр. 376)                               | Торговые автоматы                                                              |
| `code/modules/jobs/job_types/coroner.dm` (стр. 32, 54)                       | Лодаут коронера                                                                |
| `code/modules/clothing/outfits/ert.dm` (стр. 623)                            | Экипировка ERT                                                                 |
| `code/modules/economy/account.dm` (стр. 283)                                 | Упоминание в контексте звука карты                                             |
| `code/modules/food_and_drinks/machinery/deep_fryer.dm` (стр. 3)              | Блюспейс мешок можно пожарить                                                  |
| `code/modules/unit_tests/closets.dm` (стр. 10)                               | Исключение lost_crew из теста закрытия                                         |
| `code/modules/admin/verbs/admingame.dm` (стр. 168-205)                       | Админ-поиск внутри мешков                                                      |
| `code/modules/shuttle/mobile_port/variants/emergency/pods.dm` (стр. 162-163) | Спавн экологических мешков в подах                                             |

### Спрайты

| Файл                            | Описание                                                                                                               |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `icons/obj/medical/bodybag.dmi` | Все спрайты мешков (обычный, блюспейс, эко, NT-эко, тюремный, синдикат, стазис, хардлайт, сложенные варианты, оверлеи) |

### Звуки

| Файл                                      | Описание                                 |
| ----------------------------------------- | ---------------------------------------- |
| `sound/items/zip/zip.ogg`                 | Звук застёгивания/расстёгивания мешка    |
| `sound/items/weapons/egloves.ogg`         | Звук атаки хардлайт-мешков + MOD         |
| `sound/effects/spray.ogg`                 | Звук стазис-мешка при открытии/закрытии  |
| `sound/items/equip/toolbelt_equip.ogg`    | Звук застёгивания ремней тюремного мешка |
| `sound/items/duct_tape/duct_tape_rip.ogg` | Звук разрушения стазис-мешка             |

---

## Иерархия типов

```
/obj/item/bodybag (предмет)
├── /obj/item/bodybag/bluespace
├── /obj/item/bodybag/environmental
│   ├── /obj/item/bodybag/environmental/nanotrasen
│   └── /obj/item/bodybag/environmental/prisoner
│       ├── /obj/item/bodybag/environmental/prisoner/pressurized
│       └── /obj/item/bodybag/environmental/prisoner/syndicate
├── /obj/item/bodybag/stasis
└── /obj/item/bodybag/lost_crew

/obj/structure/closet/body_bag (структура, extends /obj/structure/closet)
├── /obj/structure/closet/body_bag/bluespace
├── /obj/structure/closet/body_bag/environmental
│   ├── /obj/structure/closet/body_bag/environmental/nanotrasen
│   ├── /obj/structure/closet/body_bag/environmental/prisoner
│   │   ├── /obj/structure/closet/body_bag/environmental/prisoner/hardlight
│   │   └── /obj/structure/closet/body_bag/environmental/prisoner/pressurized/syndicate
│   ├── /obj/structure/closet/body_bag/environmental/hardlight
│   └── /obj/structure/closet/body_bag/environmental/stasis
├── /obj/structure/closet/body_bag/lost_crew
│   └── /obj/structure/closet/body_bag/lost_crew/with_body
│       └── /obj/structure/closet/body_bag/lost_crew/with_body/debug
```
