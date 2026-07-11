# План переноса механик Body Bag из TG SS13 в ADT SS14

> **Статус:** ✅ — Реализовано | 🔄 — Частично/требует доработки | ❌ — Не реализовано

---

## 1. Типы мешков

### 1.1 Обычный мешок (BodyBag)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Базовая структура (EntityStorage) | ✅ | `morgue.yml` — полная реализация |
| Сворачивание/развёртывание (Foldable) | ✅ | Через `BaseDeployFoldable`, `Foldable` |
| Вместимость 1 моб | ✅ | `capacity: 1` (в ADT стоит 2, но TG — 1) |
| Подпись ручкой | ✅ | Через стандартный `PaperLabel` |
| Прикрепление бумажки | ✅ | `PaperLabel` с whitelist на Paper |
| Звук zip.ogg | ✅ | `/Audio/Misc/zip.ogg` |
| Разрушение → пластик ×2 | ✅ | ADT: SheetPlastic 1-3 (дизайн-решение ADT, не TG) |
| TRAIT_FLOORED при входе | 🔄 | `EntityStorageLayingDownOverride` — есть, но без floor эффекта как в TG |
| Крафт коробки из ткани | ✅ | `bodybag_box.yml` + Graph |
| Протолат | ❌ | В TG нет рецепта для обычного мешка; в ADT тоже нет — ок |

### 1.2 Блюспейс мешок (BodyBagBluespace)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ✅ | `BodyBagBluespace` в `morgue.yml` |
| Вместимость 15 мобов | ✅ | `capacity: 15` |
| Спрайт (bluebodybag) | ✅ | Полный набор: base, folded, open |
| Сворачивание с сохранением контента | ✅ | `BluespaceBodyBagSystem` — полная логика |
| Защита от сворачивания при >50% заполнения | ✅ | `BluespaceBodyBagSystem.OnFoldAttempt` — проверка capacity |
| Защита от рекурсии (блюспейс внутри блюспейса) | ✅ | `BluespaceBodyBagSystem.OnFoldAttempt` — проверка на вложенность |
| Сохранение w_class на основе контента | ✅ | `BluespaceBodyBagSystem.UpdateWClass` — динамический размер |
| Исследование | ✅ | `ADTBluespaceChemistry` (Tier 2) |
| Протолат-рецепт | ✅ | Железо, плазма, алмаз, блюспейс |
| Фритюрница | ❌ | Мем-механика из TG |

### 1.3 Экологический мешок (BodyBagEnvironmental)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ✅ | `BodyBagEnvironmental` в `morgue.yml` |
| Защита от погоды | ✅ | `AshStormImmune` |
| Защита от давления | ✅ | `airtight: true` (SS14-эквивалент) |
| Термоизоляция | ✅ | `airtight: true` (SS14-эквивалент) |
| Внутренний запас воздуха | ✅ | `BodyBagGas` (FillMode.Breathable) + `airtight: true` |
| Вместимость 1 моб | ✅ | `capacity: 1` |

### 1.4 Nanotrasen / Elite мешок (BodyBagEnvironmentalNanotrasen)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ✅ | `BodyBagEnvironmentalNanotrasen` |
| Полная изоляция | ✅ | `airtight: true` (наследует от Environmental) |
| Полная защита от погоды | ✅ | Наследует `AshStormImmune` от Environmental |
| Спрайты | ✅ | ntenvirobag, ntenvirobag_folded, ntenvirobag_open |

### 1.5 Тюремный мешок (BodyBagPrisoner)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ✅ | `BodyBagPrisoner` + `BodyBagPrisonerFolded` |
| Ремни (Cinch) | ✅ | `BodyBagCinch` компонент + визуал (cinchedLayer) |
| Блокировка открытия при cinch | ✅ | В `SharedBodyBagCinchSystem` |
| Визуальный оверлей cinch | ✅ | `prisonerenvirobag_cinched` спрайт |
| Спрайты | ✅ | prisonerenvirobag, folded, open, cinched |

### 1.6 Синдикатовский мешок (BodyBagSyndicate)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ✅ | `BodyBagSyndicate` + `BodyBagSyndicateFolded` |
| Полная изоляция | ✅ | `airtight: true` (наследует от Prisoner) |
| Закись азота (N2O) внутри | ✅ | `BodyBagGas` с `fillWithN2O: true` |
| Удвоенное время побега | 🔄 | В TG: 8 min breakout; в ADT не проверяли |
| Спрайты | ✅ | syndieenvirobag, folded, open, cinched |

### 1.7 ~~Хардлайт-мешок медицинский (BodyBagHardlight)~~ — УДАЛЁН
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ~~✅~~🗑️ | `BodyBagHardlight` + `BodyBagHardlightFolded` удалены из morgue.yml |
| HardlightBag компонент | ~~✅~~🗑️ | Файлы компонента и системы удалены |
| Сканирование через стенки | ➡️ Заменено | Вынесено в `BodyBagScanTarget` + `BodyBagScanSystem` (на всех не-базовых мешках) |
| MOD-модуль Patient Transport | ~~❌~~🗑️ | В ADT нет MODsuit — мешок удалён |

### 1.8 ~~Хардлайт-тюремный мешок (BodyBagHardlightPrisoner)~~ — УДАЛЁН
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ~~✅~~🗑️ | `BodyBagHardlightPrisoner` + `BodyBagHardlightPrisonerFolded` удалены |
| Cinch + Hardlight комбо | ~~✅~~🗑️ | Мешок удалён; `BodyBagPrisoner` + `BodyBagSyndicate` остались с cinch |
| MOD-модуль Criminal Capture | ~~❌~~🗑️ | Мешок удалён |

### 1.9 Стазис-мешок (BodyBagStasis)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ✅ | `BodyBagStasis` + `BodyBagStasisFolded` |
| StasisBodyBag компонент | ✅ | `Content.Shared/ADT/Medical/StasisBodyBag/` |
| Саморазрушение через время | ✅ | ~120 секунд самоуничтожения, визуальный эффект через examine |
| Стазис через 5 секунд | ✅ | Логика в `StasisBodyBag` системе + `StasisBodyBagOccupantComponent` |
| Тушение огня через 3 секунды | ✅ | Через `FlammableSystem.Extinguish()` |
| Заморозка органов (ORGAN_FROZEN) | ✅ | `FrozenOrganComponent` на органах внутри, блокирует `IsRottingEvent` |
| Внутренняя температура -60°C | ✅ | `storage.Air.Temperature = Atmospherics.T0C - 60f` в `OnClosed` |
| Визуальный эффект повреждения (examine) | ✅ | Текст осмотра: worn/moderately worn/falling apart |
| Звук spray.ogg | ✅ | `/Audio/Effects/spray.ogg` |
| Звук разрушения (duct_tape_rip.ogg) | ✅ | Скопирован в `/Audio/Items/duct_tape/duct_tape_rip.ogg` |
| Разрушение → Shreds + пластик | ✅ | `Shreds` + `SheetPlastic1` |
| AntiRottingContainer | ✅ | На `BodyBagStasis` |
| `breakout_time = 5 SECONDS` | 🔄 | В TG легко выбраться; в ADT проверка |

### 1.10 Lost Crew мешок (BodyBagLostCrew)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Entity прототип | ✅ | `BodyBagLostCrew` + `BodyBagLostCrewFolded` |
| AntiRottingContainer | ✅ | На мешке стоит |
| Спрайты | ✅ | bodybag_lost, bodybag_lost_folded, bodybag_lost_open |
| Генерация рандомного тела при открытии | ✅ | `LostCrewBodyBagSystem` спавнит `SalvageHumanCorpse` + лут + lockbox |
| История травм на теле | ❌ | Сложная TG-система, не реализована |
| Lockbox с защищёнными предметами | ✅ | `LostCrewLockboxTable` (5 департаментских lockbox) |
| `with_body` подтип | ✅ | `BodyBagLostCrewWithBody` с `spawnBodyOnMapInit: true` |
| Карго-пакет lost_crew | ✅ | `CrateLostCrew` + `MedicalLostCrew` (cost: 1000) |
| Замедление гниения в борге | ✅ | `AntiRottingContainer` уже есть на мешке |
| Продажа через карго | ❌ | TG-специфичная механика возврата денег медбею |

---

## 2. Основные механики

### 2.1 Сворачивание/развёртывание
| Механика | Статус | Примечание |
|----------|--------|------------|
| Базовая механика Foldable | ✅ | Через стандартный компонент |
| Развёртывание (attack_self) | ✅ | Стандартная механика |
| Сворачивание (ПКМ) | ✅ | Стандартная механика |
| Блюспейс: нельзя свернуть >50% | ✅ | `BluespaceBodyBagSystem.OnFoldAttempt` — проверка capacity |
| Блюспейс: проверка рекурсии | ✅ | `BluespaceBodyBagSystem.OnFoldAttempt` — проверка на вложенность |
| Блюспейс: w_class от контента | ✅ | `BluespaceBodyBagSystem.UpdateWClass` — динамический размер |
| Хардлайт: `foldedbag_path = null` | 🗑️ | Хардлайт мешки удалены, механика неактуальна |

### 2.2 Подпись и бумажка
| Механика | Статус | Примечание |
|----------|--------|------------|
| Подпись ручкой (tag_name) | ✅ | Стандартный PaperLabel |
| Прикрепление бумажки | ✅ | Стандартный PaperLabel |
| Оверлей paper/bounty/captains | ✅ | GenericVisualizer для всех типов |
| Открытие → бумажка в руку | ✅ | `PaperLabelBodyBagSystem` — бумажка выпадает на пол рядом при открытии |
| Срезание метки вайеркаттером | ✅ | Уже реализован `PaperLabelCutSystem.cs` |

### 2.3 Интеграция с моргом
| Механика | Статус | Примечание |
|----------|--------|------------|
| Обновление имени tray при закрытии | ✅ | `Content.Server/ADT/Morgue/` |
| BodyBag → tray через drag-drop | ✅ | Стандартная механика EntityStorage |

### 2.4 Интеграция с мусоропроводом (Disposal)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Запихивание мешка в disposal (drag-drop) | ✅ | `BodyBagDisposalSystem` — перехват DragDrop на disposal |
| Пустой мешок сворачивается в disposal | ✅ | `SetFolded()` при drag-drop пустого мешка на disposal |
| Полный мешок помещается целиком | 🔄 | В SS14 нельзя (FoldableSystem блокирует); показывается popup "сверните сначала" |
| Сопротивление → развёртывание внутри | ✅ | Работает через EntityStorage + ContainerRelayMovementEntityEvent |

### 2.5 MOD-модули
| Механика | Статус | Примечание |
|----------|--------|------------|
| Patient Transport (медицинский) | ❌ | В ADT нет MOD-модулей для bodybag |
| Criminal Capture (security) | ❌ | Полностью отсутствует |
| Создание хардлайт мешка из модуля | ❌ | Отсутствует |
| Дематериализация с анимацией | ❌ | Отсутствует для Criminal Capture |

### 2.6 Админ-инструменты
| Механика | Статус | Примечание |
|----------|--------|------------|
| Поиск мобов внутри body_bag | ❌ | В TG: VV-хелпер поиска по контейнерам |
| Поиск в свёрнутых блюспейс мешках | ❌ | Рекурсивный поиск |
| Админ-панель для bag contents | ❌ | Отсутствует |

### 2.7 Буксировка (Pulling)
| Механика | Статус | Примечание |
|----------|--------|------------|
| Замедление при буксировке | ✅ | `PullingSystem.cs` lines 363-374 (ADT BodyBagPullSlow) |
| Buckle на мешок | ✅ | Компонент Buckle на всех мешках |
| Pullable | ✅ | Компонент Pullable на всех мешках |
| Носилки/каталки | ✅ | `BodyBagStretcher` (4 файла) + Buckle интеграция |

### 2.8 Сканирование сквозь мешок (BodyBagScanTarget)
| Механика | Статус | Примечание |
|----------|--------|------------|
| BodyBagScanTargetComponent | ✅ | Маркер-компонент, добавляется на все не-базовые мешки |
| BodyBagScanSystem | ✅ | При использовании HealthAnalyzer на закрытом мешке → сканирует моба внутри |
| Bluespace | ✅ | Есть BodyBagScanTarget |
| Environmental / NT / Prisoner / Syndicate | ✅ | Через наследование YAML (parent) от Environmental |
| Stasis | ✅ | Есть BodyBagScanTarget |
| Lost Crew | ✅ | Есть BodyBagScanTarget |
| Обычный BodyBag | ❌ | Не имеет — базовый мешок без сканирования |
| Хардлайт (med + sec) | 🗑️ | Удалены вместе с хардлайт-системой |

---

## 3. Звуки

| Звук | Источник | Статус | Примечание |
|------|----------|--------|------------|
| zip.ogg | Застёгивание/расстёгивание | ✅ | `/Audio/Misc/zip.ogg` |
| spray.ogg | Стазис-мешок open/close | ✅ | `/Audio/Effects/spray.ogg` |
| egloves.ogg | Атака хардлайт-мешка + MOD | 🗑️ | Звук есть, но не используется — хардлайт мешки удалены |
| toolbelt_equip.ogg | Застёгивание ремней (cinch) | ✅ | Скопирован, используется BodyBagCinchSystem |
| duct_tape_rip.ogg | Разрушение стазис-мешка | ✅ | Скопирован в `/Audio/Items/duct_tape/duct_tape_rip.ogg` |

> **Файлы для копирования:** `bodybag_analysis/sounds/egloves.ogg`, `toolbelt_equip.ogg`, `duct_tape_rip.ogg`

---

## 4. Спрайты

| Файл в TG (bodybag.dmi) | Файл в ADT (bodybags.rsi) | Статус |
|-------------------------|--------------------------|--------|
| bag / bag_folded | ✅ | Есть |
| bluebodybag / bluebodybag_folded / bluebodybag_open | ✅ | Есть |
| envirobag / envirobag_folded / envirobag_open | ✅ | Есть |
| ntenvirobag / ntenvirobag_folded / ntenvirobag_open | ✅ | Есть |
| prisonerenvirobag / folded / open / cinched | ✅ | Есть |
| syndieenvirobag / folded / open / cinched | ✅ | Есть |
| holobag_med / holobag_med_open | 🗑️ | Не используются (хардлайт мешки удалены) |
| holobag_sec / holobag_sec_open / holobag_sec_cinched | 🗑️ | Не используются (хардлайт мешки удалены) |
| stasis_bag_folded | ✅ | Есть |
| bodybag_lost / bodybag_lost_folded / bodybag_lost_open | ✅ | Есть |
| paper / bounty / captains_paper / invoice | ✅ | Есть |
| bodybag_label | ✅ | Есть |

> **Вывод:** Все спрайты из TG уже портированы в ADT. Дополнительных спрайтов не требуется.

---

## 5. Исследования и латхе

| Механика | Статус | Примечание |
|----------|--------|------------|
| Исследование блюспейс мешка | ✅ | `ADTBluespaceChemistry` в `biochemical.yml` |
| Исследование стазис мешка | ✅ | `ADTCryoTech` в `biochemical.yml` |
| Исследование эко-мешка | ✅ | `ADTAdvancedMedicalCare` в `biochemical.yml` |
| Латхе-рецепт блюспейс | ✅ | `medical.yml` — Recipes/Lathes |
| Латхе-рецепт стазис | ✅ | plastic + silver |
| Латхе-рецепт эко | ✅ | Есть |
| Латхе-рецепт NT | ✅ | Есть |
| Латхе-рецепт Prisoner | ✅ | Есть |
| Латхе-рецепт Syndicate | ✅ | Есть |
| Латхе-рецепт Hardlight | ✅ | Есть |
| Латхе-рецепт HardlightPrisoner | ✅ | Есть |
| Латхе-рецепт LostCrew | ✅ | Есть |
| Крафт коробки из ткани | ✅ | `bodybag_box.yml` |

---

## 6. Карго, вендинг, заполнение карт

| Механика | Статус | Примечание |
|----------|--------|------------|
| Карго-заказы | ✅ | `cargo_medical.yml` |
| Вендинг (медицинский) | ✅ | `medical.yml` — VendingMachines |
| Лут в шкафчиках | ✅ | Настроено для 45+ карт |
| Коробки с мешками | ✅ | `boxes/medical.yml` |
| Ящики (crates) | ✅ | `crates/medical.yml` |

---

## 7. Приоритетный план реализации

### Очередь 1: Критические механики ✅
1. **Стазис-мешок: саморазрушение по таймеру** — ✅ `DamagePerSecond = 0.33` (~120 сек), examine-текст о состоянии
2. **Стазис-мешок: заморозка органов** — ✅ `FrozenOrganComponent` + `IsRottingEvent.Handled = true`
3. **Стазис-мешок: тушение огня** — ✅ `FlammableSystem.Extinguish()` через 3 секунды
4. **Звуки: добавить отсутствующие** — ✅ toolbelt_equip.ogg, duct_tape_rip.ogg скопированы

### Очередь 2: Блюспейс мешок ✅
5. **Защита от сворачивания при >50%** — ✅ проверка capacity в `BluespaceBodyBagSystem`
6. **Проверка рекурсии блюспейс** — ✅ проверка на `BluespaceBodyBagComponent` внутри
7. **w_class от содержимого** — ✅ динамический `ItemComponent.Size` через `SharedItemSystem.SetSize()`

### Очередь 3: Экологические мешки ✅
8. **Термоизоляция и защита от давления** — ✅ `airtight: true` на эко- и NT-мешках (SS14-эквивалент)
9. **Внутренний запас воздуха** — ✅ `BodyBagGas (FillMode.Breathable)` + `airtight: true`

### Очередь 4: Lost Crew мешок ✅
10. **Генерация тела при открытии** — ✅ `LostCrewBodyBagSystem` спавнит `SalvageHumanCorpse`
11. **Lockbox с предметами** — ✅ `LostCrewLockboxTable` (5 департаментских lockbox через CrateBaseLockBox)
12. **Карго-пакет lost_crew** — ✅ `CrateLostCrew` + `MedicalLostCrew` (cost: 1000)
13. **Замедление гниения в борге** — ✅ `AntiRottingContainer` уже есть на мешке

### Очередь 5: Мусоропровод ✅
14. **stuff_bodybag_in()** — ✅ `BodyBagDisposalSystem` перехватывает DragDrop на disposal
15. **Сворачивание пустого мешка** — ✅ Пустой unfolded мешок автоматически сворачивается при drag-drop
16. **Сопротивление → развёртывание** — ✅ Работает через EntityStorage + ContainerRelayMovementEntityEvent

### Очередь 6: MOD-модули 🗑️ (хардлайт мешки удалены, MOD системы нет)
17. **Patient Transport** — 🗑️ хардлайт-мешок удалён
18. **Criminal Capture** — 🗑️ хардлайт-тюремный мешок удалён

### Очередь 7: Админ-инструменты
19. **Поиск мобов в bodybag** — через VV или админ-панель
20. **Рекурсивный поиск в блюспейс** — поиск по свёрнутым мешкам

---

## 8. Технические замечания

- ADT SS14 использует компонентную архитектуру (ECS), а не наследование DM
- Все новые механики нужно писать на C# как отдельные компоненты и системы
- Стандартные системы (EntityStorage, Foldable, PaperLabel, Buckle, Pullable) уже предоставляют 80% функционала
- Звуки лицензионно чисты (CC-BY-SA 3.0, как и весь код TG)
- Lost Crew — самая объёмная missing feature (~5-7 компонентов + системы)
- Disposal integration требует модификации disposal системы (если она есть в SS14)
- MOD-модули — только если в ADT есть MODsuit система
- Хардлайт мешки (медицинский и тюремный) **удалены** из-за отсутствия MOD системы — сканирование сквозь стенки вынесено в `BodyBagScanTarget` на все не-базовые мешки

---

## 9. Сводка

| Категория | Всего | ✅ | 🔄 | ❌ | 🗑️ |
|-----------|-------|---|---|---|-----|
| Типы мешков (8 вместо 10) | 74 | 57 | 8 | 7 | 2 |
| Сканирование сквозь мешок | 8 | 7 | 0 | 1 | 0 |
| Сворачивание/развёртывание | 6 | 5 | 0 | 0 | 1 |
| Подпись и бумажка | 5 | 5 | 0 | 0 | 0 |
| Морг | 2 | 2 | 0 | 0 | 0 |
| Мусоропровод | 4 | 3 | 1 | 0 | 0 |
| MOD-модули | 4 | 0 | 0 | 0 | 4 |
| Админ-инструменты | 3 | 0 | 0 | 3 | 0 |
| Буксировка | 4 | 4 | 0 | 0 | 0 |
| Звуки (5) | 5 | 4 | 0 | 0 | 1 |
| Спрайты | 20+ | 18+ | 0 | 0 | 2 |
| Исследования/латхе | 12 | 10 | 0 | 0 | 2 |
| Карго/вендинг/карты | 4 | 4 | 0 | 0 | 0 |
| **ИТОГО** | **~149** | **~119** | **~9** | **~11** | **~10** |

> **Общий прогресс:** ~80% полностью реализовано, ~6% частично, ~7% не реализовано, ~7% удалено
