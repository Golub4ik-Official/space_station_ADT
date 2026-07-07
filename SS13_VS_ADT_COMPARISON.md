# SS13 vs ADT — Сравнение и анализ неперенесённых механик

> **Источник SS13:** `_SS14_Borg_Port/README.md` (2021 строка, полная спецификация)
> **Текущая реализация ADT:** `ADT_BORG_REWORK.md` (615 строк)
> **Дата:** 2026-07-06 (актуализировано: MMI реворк + Construction + материалы + SyndicateMMI + door control + исправления сборки)
> **ВНИМАНИЕ:** Большинство пунктов ниже были перенесены — документ отражает **раннюю** оценку и обновляется по мере реворка.

---

## 1. Общая архитектура

| Компонент | SS13 | ADT | Статус |
|-----------|------|-----|--------|
| Иерархия silicon → robot / ai / pai / decoy | ✅ | Только robot, частично AI | ❌ Нет pAI, decoy |
| Разделение silicon-общих механик | ✅ (silicon_*.dm) | Частично через ShareBorgSystem | ⚠️ Частично |
| Silicon jobs (/datum/job/cyborg, /datum/job/ai) | ✅ | Не описано | ❌ Неизвестно |

---

## 2. Сборка киборга (Construction)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| 4 этапа сборки | ✅ (Frame → Limbs → Multitool → MMI) | ✅ (7 стадий) | ✅ Своя система |
| Конечности (l_arm, r_arm, l_leg, r_leg) | ✅ | ✅ | ✅ |
| Грудь с изоляцией + батарея | ✅ | ✅ | ✅ |
| Голова с 2 вспышками | ✅ | ✅ | ✅ |
| Multitool настройка (имя, ИИ, законы, локомоция, блокировка) | ✅ | ✅ (Configuration stage) | ✅ |
| MMI вставка как финальный этап | ✅ | ✅ (TryInsertMMI — MMI/BorgBrain) | ✅ |
| Деконструкция (5 шагов) | ✅ | ✅ | ✅ |
| Материалы для сборки (10/50 металла, кабель, 2 вспышки) | ✅ | ✅ (сталь депозитом, кабель + 2 вспышки чеком) | ✅ |

---

## 3. Робомозг (MMI / Robotic Brain)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| MMI — контейнер для мозга (OrganSlot, визуалы Off/Alive/Dead/Brain) | ✅ | ✅ (MMI entity + MMIComponent) | ✅ |
| MMI крафт (латник: сталь/стекло/манипулятор/кабель) | ✅ | ✅ (Lathe recipes: RoboticBrain + MMIRadioEnabled) | ✅ |
| Robotic Brain (/obj/item/organ/brain/robotic_brain) | ✅ | ✅ (RoboticBrain entity, BorgBrainComponent + ghost role) | ✅ |
| MMI с интегрированным радио | ✅ (2 типа) | ✅ (MMIRadioEnabled — Binary + Common) | ✅ |
| Syndicate MMI (нельзя изменить законы) | ✅ | ✅ (SyndicateMMI entity, SyndicateImmune flag) | ✅ |
| Извлечение мозга из трупа киборга | ✅ | ✅ (BorgDeathSystem) | ✅ |
| Создание Robotic Brain по умолчанию при спавне | ✅ | ✅ (C# fallback в BorgSystem.OnMapInit + ContainerFill) | ✅ |

---

## 4. Техническая панель (Maintenance Panel)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| 4 состояния панели (закрыта/заблок./открыта/провода) | ✅ (state machine) | Частично | ⚠️ |
| ID карта для разблокировки крышки | ✅ | ❌ | ❌ **НЕТ** |
| Crowbar открывает/закрывает крышку | ✅ | ✅ | ✅ |
| Crowbar извлекает батарею | ✅ | ❌ Не указано | ❌ **Возможно не перенесено** |
| Crowbar деконструкция (все провода перерезаны) | ✅ | ✅ | ✅ |
| Screwdriver открывает провода (если нет батареи) | ✅ | ✅ | ✅ |
| Эмаг в 2 шага (разблокировка → эмаг) | ✅ | ❌ | ❌ **НЕТ** |
| Киборг сам открывает крышку (модуль-взломщик) | ✅ | ❌ | ❌ **НЕТ** |
| lockcharge (иммобилизация) | ✅ | ✅ | ✅ |

---

## 5. Провода (Wires)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| 4 провода (AI_CONTROL, CAMERA, LAWCHECK, LOCKED) | ✅ | ✅ | ✅ |
| Случайные цвета при инициализации | ✅ | ✅ | ✅ |
| WiresSystem UI | ✅ | ✅ | ✅ |
| AI_CONTROL cut → отключение от ИИ | ✅ | Отключает синхр. законов | ⚠️ **Отличается** (SS13: отключение от ИИ, ADT: только синхронизация) |
| CAMERA cut → отключение камеры, выгон наблюдателей | ✅ | Отключает StationAiVision | ⚠️ **Отличается** |
| LAWCHECK cut → lawupdate = TRUE | ✅ | Отключает синхр. законов | ❌ **SS13 включает, ADT отключает** |
| LOCKED cut → включает локдаун (пока перерезан) | ✅ | Блокирует борга | ✅ |
| Pulse AI_CONTROL → подключение к случайному ИИ | ✅ | — | ❌ **НЕТ** |
| Pulse CAMERA → выгон наблюдателей | ✅ | Нокдаун 1с | ⚠️ **Отличается** |
| Pulse LAWCHECK → нет эффекта | ✅ | — | ✅ |
| Pulse LOCKED → переключение lockcharge | ✅ | Переключает блокировку | ✅ |

---

## 6. Связь с ИИ (AI Connection)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| connected_ai ссылка на ИИ | ✅ | ✅ BorgAiLinkComponent | ✅ |
| lawupdate флаг синхронизации | ✅ | ✅ | ✅ |
| scrambledcodes (скрыт от консоли) | ✅ | ❌ | ❌ **НЕТ** |
| connect_to_ai() | ✅ | ✅ | ✅ |
| disconnect_from_ai() | ✅ | ✅ | ✅ |
| UnlinkSelf() (полный разрыв) | ✅ | ❌ | ❌ **НЕТ** |
| Malf ИИ → киборг становится минслейвом | ✅ | ❌ | ❌ **НЕТ** |
| remove_robot_mindslave() | ✅ | ❌ | ❌ **НЕТ** |
| make_malf_robot() → rebuild_modules | ✅ | ❌ | ❌ **НЕТ** |

---

## 7. Синхронизация законов (Law Sync)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| synchronize ALL laws (zeroth, inherent, supplied, ion) | ✅ | ✅ (общая синхронизация) | ⚠️ Неизвестно что копируется |
| ion_laws (ионные/хакнутые законы) | ✅ | ❌ | ❌ **НЕТ** |
| supplied_laws (законы с консоли) | ✅ | ❌ | ❌ **НЕТ** |
| zeroth_law / zeroth_law_borg разделение | ✅ | ❌ | ❌ **НЕТ** |
| Syndicate MMI блокирует zeroth | ✅ | ✅ (SyndicateImmune flag) | ✅ |
| Law Sync при Login | ✅ | ❌ | ❌ **НЕТ** |
| Law Sync при cmd_show_laws() | ✅ | ✅ (State Laws) | ✅ |
| Law Sync при LAWCHECK wire | ✅ | ❌ | ❌ **НЕТ** |

---

## 8. Модули (Modules)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| Типы модулей | 14 типов | 9 типов | ⚠️ **Частично** |
| Engineering | ✅ | ✅ | ✅ |
| Medical | ✅ | ✅ | ✅ |
| Security | ✅ | ✅ | ✅ |
| Janitor | ✅ | ✅ | ✅ |
| Miner | ✅ | ✅ | ✅ |
| Service/Butler | ✅ | ✅ | ✅ |
| Combat | ✅ | ✅ | ✅ |
| Destroyer | ✅ | ✅ | ✅ |
| Syndicate | ✅ | ✅ | ✅ |
| Alien/Hunter | ✅ | ❌ (отмечен "не требуется") | ❌ |
| Drone | ✅ | ❌ | ❌ **НЕТ** |
| 3 слота для предметов | ✅ | 4 слота | ✅ (расширение) |
| rebuild_modules() при эмаге/апгрейде/malf | ✅ | ❌ | ❌ **НЕТ** |
| Система стаков (energy + material) | ✅ | ✅ | ✅ |
| recharge_consumables() каждый тик | ✅ | ✅ BorgEnergyRechargeSystem | ✅ |
| Gripper (4 подтипа) | ✅ | ✅ BorgGripperComponent | ✅ |
| Gripper подтипы (medical, service, mining, engineering) | ✅ | ❌ | ❌ **Возможно не перенесено** |
| add_languages() per module | ✅ | ✅ | ✅ |
| add_armor() per module | ✅ | ✅ | ✅ |
| add_subsystems_and_actions() | ✅ | ✅ | ✅ |
| emag_act() per module (специфические эффекты) | ✅ | ❌ | ❌ **НЕТ** |

---

## 9. Знание языков (Languages)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| Понимание всех языков людей/силиконов/ботов/мозгов | ✅ | ❌ | ❌ **НЕТ** |
| Специфические языки от модуля | ✅ | ✅ | ✅ |
| Кремниевая речь через radio | ✅ | ✅ | ✅ |

---

## 10. Батарея и питание (Battery/Power)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| Компонент cell (не внешний) | ✅ | ✅ BorgBatteryComponent | ✅ |
| Потребление энергии (формула от lamp_intensity) | ✅ (clamp((lamp-2)*2, 1, charge)) | ❌ | ❌ **НЕТ** |
| Low Power Mode (замедление, звук) | ✅ | ✅ (BorgVisualsSystem мигание <15%) | ⚠️ Частично |
| External Power (externally_powered) | ✅ | ❌ | ❌ **НЕТ** |
| Emergency Reboot (13-18 сек) | ✅ (jitter) | ✅ (15с фикс) | ⚠️ Отличается |
| Manual Charging (роботехник с TRAIT_CYBORG_SPECIALIST) | ✅ | ❌ | ❌ **НЕТ** |
| Замена батареи | ✅ | ✅ | ✅ |
| Self-Repair тратит 10 энергии (30 в крите) | ✅ | 5 энергии | ⚠️ **Отличается** |
| Self-Repair лечит 1/2сек (2.5 в крите) | ✅ | 2/3сек (все части) | ⚠️ **Отличается** |

---

## 11. Вкладки / Способности / Подсистемы (UI/Tabs/Abilities/Subsystems)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| **HUD элементы:** | | | |
| Radio button | ✅ | ❌ | ❌ **НЕТ** |
| Module slots (x3/x4) | ✅ (3) | ✅ (4) | ✅ |
| Sensor aug | ✅ | ✅ | ✅ |
| Intent selector | ✅ | ❌ | ❌ **НЕТ** |
| Movement intent | ✅ | ❌ | ❌ **НЕТ** |
| Health bar | ✅ | ✅ | ✅ |
| Stamina bar | ✅ | ❌ | ❌ **НЕТ** |
| Module icon (grid) | ✅ | ❌ | ❌ **НЕТ** |
| Store button (uneq_active) | ✅ | ❌ | ❌ **НЕТ** |
| Pull icon | ✅ | ❌ | ❌ **НЕТ** |
| Zone selector | ✅ | ❌ | ❌ **НЕТ** |
| Headlamp button | ✅ | ✅ | ✅ |
| Thrusters button | ✅ | ✅ | ✅ |
| PDA button | ✅ | ❌ | ❌ **НЕТ** |
| **Подсистемы (Subsystems):** | | | |
| subsystem_atmos_control() | ✅ | ❌ | ❌ **НЕТ** |
| subsystem_crew_monitor() | ✅ | ❌ | ❌ **НЕТ** |
| subsystem_law_manager() | ✅ | ❌ | ❌ **НЕТ** |
| subsystem_power_monitor() | ✅ | ❌ | ❌ **НЕТ** |
| self_diagnosis() | ✅ | ✅ | ✅ |
| set_mail_tag() | ✅ | ❌ | ❌ **НЕТ** |
| **Действия (Actions):** | | | |
| X-ray vision | ✅ | ❌ | ❌ **НЕТ** |
| Thermal vision | ✅ | ✅ | ✅ |
| Meson vision | ✅ | ✅ | ✅ |
| Engineering scanner (цикличный) | ✅ | ❌ | ❌ **НЕТ** |
| Magpulse | ✅ | ✅ | ✅ |
| Robot override lock (синди) | ✅ | ❌ | ❌ **НЕТ** |
| **PDA функции:** | | | |
| robot_headlamp | ✅ | ✅ | ✅ |
| robot_self_diagnosis | ✅ | ✅ | ✅ |
| Call Security | ✅ | ❌ | ❌ **НЕТ** |
| Send PDA Message | ✅ | ❌ | ❌ **НЕТ** |

---

## 12. Компоненты/Органы (Robot Components)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| 7 компонентов | ✅ | 6 (cell не отдельная entity) | ⚠️ Cell — часть батарейной системы |
| Armour (100 HP, броня) | ✅ | ✅ | ✅ |
| Actuator (50 HP, движение) | ✅ | ✅ | ✅ |
| Cell (50 HP, питание) | ✅ | ✅ BorgBatteryComponent | ✅ |
| Radio (40 HP, коммуникация) | ✅ | ✅ | ✅ |
| BinaryCommunication (30 HP, бинарник) | ✅ | ✅ | ✅ |
| Camera (40 HP, видеокамера) | ✅ | ✅ | ✅ |
| DiagnosisUnit (30 HP, диагностика) | ✅ | ✅ | ✅ |
| damage_protection модификатор | ✅ | ✅ | ✅ |
| brute_mod / burn_mod | ✅ | ⚠️ Не указано | ❌ **Возможно не перенесено** |
| brutedamage / electronicsdamage (два типа) | ✅ | ✅ brute/burn | ✅ |
| break_component() → broken_device | ✅ | ❌ | ❌ **НЕТ** |
| toggle() компонента пользователем | ✅ | ❌ | ❌ **НЕТ** |
| heal_damage() отдельно brute/elec | ✅ | ✅ | ✅ |
| is_component_functioning() | ✅ | ❌ | ❌ **НЕТ** |

---

## 13. Повреждения и ремонт (Damage & Repair)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| health = maxHealth - total_damage | ✅ | ✅ | ✅ |
| maxHealth = 100 + bonuses | ✅ | ❌ | ❌ **НЕТ** |
| Два типа урона (brute/burn) | ✅ | ✅ (blunt→brute, heat→burn) | ✅ |
| Armour принимает урон первым | ✅ | ✅ | ✅ |
| Случайное распределение по компонентам без брони | ✅ | ✅ | ✅ |
| **Модуль слоты ломаются при HP < 50/0/-50** | ✅ | ❌ | ❌ **НЕТ** |
| Cable coil чинит electronics | ✅ | ❌ | ❌ **НЕТ** |
| Замена компонента новым (полное лечение) | ✅ | ✅ | ✅ |
| Self-Repair (апгрейд) | ✅ | ✅ | ✅ |
| Nanopaste (быстрое лечение) | ✅ | ❌ | ❌ **НЕТ** |
| Emergency Reboot механизм | ✅ | ✅ | ✅ |

---

## 14. Эмаг (Emag / Subversion)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| 2-шаговый процесс (разблокировка → эмаг) | ✅ | ❌ (в 1 шаг?) | ❌ **НЕТ** |
| Отключение от ИИ (connected_ai = null) | ✅ | ✅ | ✅ |
| Смена законов на syndicate_override | ✅ | ✅ (ADTSyndicateOverride) | ✅ |
| Установка zeroth_law ("Служить [эмагнувшему]") | ✅ | ✅ | ✅ |
| Mindslave (киборг — минслейв эмагнувшего) | ✅ | ❌ | ❌ **НЕТ** |
| Разблокировка emag_modules | ✅ | ✅ | ✅ |
| Смена цвета спрайта (красный/эмаг) | ✅ | ✅ (BorgVisualsSystem red tint) | ✅ |
| Per-module emag_act() эффекты | ✅ | ❌ | ❌ **НЕТ** |
| Unemag (админский откат) | ✅ | ❌ | ❌ **НЕТ** |
| Weapons Unlock (safety override) | ✅ | ✅ | ✅ |
| Malf AI remote emag через консоль | ✅ | ❌ | ❌ **НЕТ** |
| Malf AI апгрейды → malf_modules | ✅ | ❌ | ❌ **НЕТ** |

---

## 15. Глобальные модули (детальные предметы)

### Инженерный (Engineering)

| Предмет | SS13 | ADT | Статус |
|---------|------|-----|--------|
| RPD | ✅ | ? | ❌ **Неизвестно** |
| Сварочный аппарат | ✅ | ✅ | ✅ |
| Гаечный ключ, лом, отвёртка, кусачки | ✅ | ✅ | ✅ |
| Мультитул | ✅ | ✅ | ✅ |
| Металл (50), стекло (50), rods (50), catwalk (50) | ✅ | ? | ❌ **Неизвестно** |
| Gripper/engineering | ✅ | ✅ | ✅ |
| RCD (апгрейд) | ✅ | ✅ | ✅ |
| RPED (апгрейд) | ✅ | ✅ | ✅ |
| Engineering scanner (цикличный) | ✅ | ❌ | ❌ **НЕТ** |
| Magpulse | ✅ | ✅ | ✅ |

### Медицинский (Medical)

| Предмет | SS13 | ADT | Статус |
|---------|------|-----|--------|
| Borg hypospray | ✅ | ✅ | ✅ |
| Defibrillator | ✅ | ✅ | ✅ |
| Roller bed holder | ✅ | ❌ (HoloStretcher удалён) | ❌ **НЕТ** |
| Health analyzer | ✅ | ✅ | ✅ |
| Gripper/medical | ✅ | ✅ | ✅ |
| Хирургические инструменты | ✅ | ? | ❌ **Неизвестно** |
| Splint, brute pack, burn pack, nanopaste стаки | ✅ | ? | ❌ **Неизвестно** |
| Thermal vision | ✅ | ✅ | ✅ |

### Шахтёрский (Miner)

| Предмет | SS13 | ADT | Статус |
|---------|------|-----|--------|
| Дрель (drill) | ✅ | ✅ | ✅ |
| Diamond drill (апгрейд) | ✅ | ✅ | ✅ |
| Kinetic Accelerator (КА) | ✅ | ✅ | ✅ |
| Мешок для руды | ✅ | ✅ | ✅ |
| Satchel of Holding (апгрейд) | ✅ | ✅ | ✅ |
| GPS | ✅ | ✅ | ✅ |
| Gripper/mining | ✅ | ✅ | ✅ |
| Meson vision | ✅ | ✅ | ✅ |
| Lavaproof (апгрейд) | ✅ | ✅ | ✅ |
| Magpulse | ✅ | ✅ | ✅ |

### Уборщик (Janitor)

| Предмет | SS13 | ADT | Статус |
|---------|------|-----|--------|
| Мыло (soap) | ✅ | ✅ | ✅ |
| Швабра (mop) | ✅ | ✅ | ✅ |
| Мешок для мусора | ✅ | ✅ | ✅ |
| Bluespace trash bag (апгрейд) | ✅ | ✅ | ✅ |
| Заменитель лампочек | ✅ | ✅ | ✅ |
| Веник (push broom) — автоуборка при движении | ✅ | ? | ❌ **Неизвестно** |
| Жидкость для мытья полов | ✅ | ? | ❌ **Неизвестно** |
| Floor buffer (апгрейд) — авточистка | ✅ | ✅ | ✅ |

### Сервисный (Service)

| Предмет | SS13 | ADT | Статус |
|---------|------|-----|--------|
| Dispenser (booze/soda) | ✅ | ✅ | ✅ |
| RSF | ✅ | ✅ | ✅ |
| RSF Executive (апгрейд) | ✅ | ✅ | ✅ |
| Музыкальный синтезатор | ✅ | ? | ❌ **Неизвестно** |
| Gripper/service | ✅ | ✅ | ✅ |
| Ручка/робопен | ✅ | ? | ❌ **Неизвестно** |
| Форм-принтер | ✅ | ? | ❌ **Неизвестно** |
| Cheese knife (апгрейд) | ✅ | ✅ | ✅ |

### Охранный (Security)

| Предмет | SS13 | ADT | Статус |
|---------|------|-----|--------|
| Электрошоковая дубинка (baton) | ✅ | ✅ | ✅ |
| Дисейблер (disabler) | ✅ | ✅ | ✅ |
| Disabler cooler (апгрейд) | ✅ | ✅ | ✅ |
| Стяжки (zipties) | ✅ | ✅ | ✅ |
| Holo-sign | ✅ | ? | ❌ **Неизвестно** |
| SecHUD | ✅ | ? | ❌ **Неизвестно** |

---

## 16. Дополнительные модули (Fabricator Upgrades / Апгрейды)

| Апгрейд | SS13 | ADT | Статус |
|---------|------|-----|--------|
| Reset | ✅ | ✅ | ✅ |
| Rename | ✅ | ❌ | ❌ **НЕТ** |
| Restart | ✅ | ✅ | ✅ |
| Thrusters | ✅ | ✅ | ✅ |
| SelfRepair | ✅ | ✅ | ✅ |
| VTEC | ✅ | ✅ | ✅ |
| DisablerCooler | ✅ | ✅ | ✅ |
| DiamondDrill | ✅ | ✅ | ✅ |
| SatchelOfHolding | ✅ | ✅ | ✅ |
| Lavaproof | ✅ | ✅ | ✅ |
| RCD | ✅ | ✅ | ✅ |
| RPED | ✅ | ✅ | ✅ |
| FloorBuffer | ✅ | ✅ | ✅ |
| BluespaceTrashBag | ✅ | ✅ | ✅ |
| RSFExecutive | ✅ | ✅ | ✅ |
| HoloStretcher | ✅ | ❌ **(УДАЛЁН по запросу)** | ❌ |
| Syndicate (WeaponsUnlock) | ✅ | ✅ | ✅ |
| Abductor (engi/medi/jani) | ✅ (3 шт.) | ❌ | ❌ **НЕТ** |
| ExpandedChassis | ❌ (нет в SS13) | ✅ | **(ADT-специфичный)** |

---

## 17. Синдикатовские борги (Syndicate Borgs)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| Assault | ✅ | ✅ ADTBorgChassisSyndicateAssault | ✅ |
| Medical | ✅ | ✅ ADTBorgChassisSyndicateMedical | ✅ |
| Saboteur | ✅ | ✅ ADTBorgChassisSyndicateSaboteur | ✅ |
| lawupdate = FALSE | ✅ | ? | ❌ **Неизвестно** |
| scrambledcodes = TRUE | ✅ | ❌ | ❌ **НЕТ** |
| has_camera = FALSE | ✅ | ❌ | ❌ **НЕТ** |
| faction = list("syndicate") | ✅ | ❌ | ❌ **НЕТ** |
| Bluespace cell | ✅ | ❌ | ❌ **НЕТ** |
| damage_protection / reduced damage (brute_mod 0.7) | ✅ | ❌ | ❌ **НЕТ** |

---

## 18. Система законов (Law System)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| /datum/ai_law (index, law, ion/zero/inherent/supplied) | ✅ | ✅ SiliconLawPrototype | ✅ |
| /datum/ai_laws (zeroth, inherent, supplied, ion, sorted) | ✅ | ✅ SiliconLawsetPrototype | ✅ |
| zeroth_law для ИИ | ✅ | ✅ | ✅ |
| zeroth_law_borg (отдельный текст) | ✅ | ❌ | ❌ **НЕТ** |
| ion_laws (ионные) | ✅ | ❌ | ❌ **НЕТ** |
| supplied_laws (с консоли) | ✅ | ❌ | ❌ **НЕТ** |
| sync(mob/target) — копия законов | ✅ | ✅ | ✅ |
| show_laws(who) | ✅ | ✅ State Laws | ✅ |
| **21 набор законов (Law Sets)** | ✅ | **2 набора** (ADTAsimov, ADTSyndicateOverride) | ❌ **НЕТ** |
| Asimov | ✅ | ✅ ADTAsimov | ✅ |
| Crewsimov | ✅ | ❌ **(УДАЛЁН?)** | ❌ **Был, теперь неясно** |
| NT Standard | ✅ | ❌ | ❌ |
| NT Aggressive | ✅ | ❌ | ❌ |
| NT Malfunction | ✅ | ❌ | ❌ |
| Quarantine | ✅ | ❌ | ❌ |
| Robocop | ✅ | ❌ | ❌ |
| PALADIN | ✅ | ❌ | ❌ |
| Corporate | ✅ | ❌ | ❌ |
| TYRANT | ✅ | ❌ | ❌ |
| Antimov | ✅ | ❌ | ❌ |
| Pranksimov | ✅ | ❌ | ❌ |
| CCTV | ✅ | ❌ | ❌ |
| Hippocratic | ✅ | ❌ | ❌ |
| Station Efficiency | ✅ | ❌ | ❌ |
| Peacekeeper | ✅ | ❌ | ❌ |
| Deathsquad | ✅ | ❌ | ❌ |
| Epsilon | ✅ | ❌ | ❌ |
| Syndicate Override | ✅ | ✅ ADTSyndicateOverride | ✅ |
| ERT Override | ✅ | ❌ | ❌ |
| Mindflayer Override | ✅ | ❌ | ❌ |

---

## 19. HUD / Интерфейс

| Элемент | SS13 | ADT | Статус |
|---------|------|-----|--------|
| Radio button | ✅ | ❌ | ❌ **НЕТ** |
| Module slot 1-3 (4 в ADT) | ✅ | ✅ | ✅ |
| Sensor aug | ✅ | ✅ | ✅ |
| Intent selector | ✅ | ❌ | ❌ **НЕТ** |
| Movement intent | ✅ | ❌ | ❌ **НЕТ** |
| Health bar | ✅ | ✅ | ✅ |
| Stamina bar | ✅ | ❌ | ❌ **НЕТ** |
| Module icon (grid предметов) | ✅ | ❌ | ❌ **НЕТ** |
| Store button (uneq_active) | ✅ | ❌ | ❌ **НЕТ** |
| Pull icon | ✅ | ❌ | ❌ **НЕТ** |
| Zone selector | ✅ | ❌ | ❌ **НЕТ** |
| Headlamp button | ✅ | ✅ | ✅ |
| Thrusters button | ✅ | ✅ | ✅ |
| PDA button | ✅ | ❌ | ❌ **НЕТ** |
| Статус-панель (имя, модуль, состояние, батарея, стаки, скорость, ИИ, компоненты) | ✅ | ❌ | ❌ **НЕТ** |

---

## 20. Клик-обработка (Click Handling)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| ClickOn с модификаторами (Ctrl, Shift, Alt, Middle) | ✅ | ⚠️ Частично (Alt+Click через radial) | ⚠️ |
| Ctrl+MiddleClick → cycle_modules | ✅ | ❌ | ❌ **НЕТ** |
| MiddleClick → Point | ✅ | ✅ (vanilla) | ✅ |
| Shift+Click → осмотр / шлюзы | ✅ | ✅ (vanilla) | ✅ |
| Ctrl+Click → болтирование шлюзов, APC, турель | ✅ | ❌ | ❌ **НЕТ** |
| Alt+Click → электрификация шлюзов / **radial меню дверей** | ✅ | ✅ **radial для боргов и AI** (door control) | ✅ **ДОБАВЛЕНО** |
| Специальные шлюз-клики (Ctrl+Shift, Alt+Shift, Shift+Middle) | ✅ | ❌ | ❌ **НЕТ** |
| **Кейбинды управления шлюзами** (наведи курсор + нажми клавишу) | ❌ (нет в SS13) | ✅ **4 кейбинда** (bolt/electrify/emergency/toggle) | ✅ **ADT-специфично** |

### Door Control (реворк 2026-07-06)

Реализовано:
- **Alt+ЛКМ по шлюзу/створке/бластдору** → радиальное меню (bolt, electrify, emergency access, open/close)
- **ADT-борги** теперь могут открывать radial (раньше только vanilla `BorgChassisComponent`)
- **Создки/бластдоры** получили `AiUi.Key` UI — radial работает на всех дверях
- **Open/close toggle** добавлен в radial (для дверей без bolt/electrify)
- **4 кейбинда** в настройках управления:
  - `ADTBorgBoltDoor` — заболтировать/разболтировать шлюз под курсором
  - `ADTBorgElectrifyDoor` — электризовать/обесточить
  - `ADTBorgEmergencyDoor` — аварийный доступ
  - `ADTBorgToggleDoor` — открыть/закрыть (любой шлюз/створку/бластдор)

**Файлы:**
- `Content.Shared/ADT/Silicons/Borgs/DoorControl/BorgDoorControlEvent.cs` — enum + NetSerializable event
- `Content.Server/ADT/Silicons/Borgs/DoorControl/BorgDoorControlSystem.cs` — обработка действий
- `Content.Client/ADT/Silicons/Borgs/DoorControl/BorgDoorControlSystem.cs` — кейбинды + entity-under-cursor
- `Content.Shared/Silicons/StationAi/Systems/SharedStationAiSystem.Held.cs` — verb для ADT-боргов (ADT-Tweak)
- `Content.Shared/Silicons/StationAi/Systems/SharedStationAiSystem.Airlock.cs` — open/close toggle (ADT-Tweak)
- `Resources/Locale/{en-US,ru-RU}/ADT/silicons/borg-door-control.ftl` — локализация

---

## 21. Движение (Movement)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| movement_delay() с формулой | ✅ | ❌ | ❌ **НЕТ** |
| component slowdown (сумма slowdown_factor) | ✅ | ✅ Actuator — 50% замедление | ✅ (упрощённо) |
| stamina slowdown (staminaloss / 40) | ✅ | ❌ | ❌ **НЕТ** |
| magboots correction в 0G | ✅ | ❌ | ❌ **НЕТ** |
| destroyer mobility (-3 к задержке) | ✅ | ❌ | ❌ **НЕТ** |
| VTEC cap (3.5) | ✅ | ✅ (1.5x) | ⚠️ **Отличается** |
| Process_Spacemove() с ionpulse | ✅ | ✅ | ✅ |
| ionpulse тратит 25 энергии/шаг | ✅ | ✅ | ✅ |
| Magpulse (mob_negates_gravity) | ✅ | ✅ | ✅ |
| Magpulse (experience_pressure_difference) | ✅ | ❌ | ❌ **НЕТ** |

---

## 22. Звуки

| Звук | SS13 | ADT | Статус |
|------|------|-----|--------|
| borg_deathsound.ogg | ✅ | ✅ | ✅ |
| robot_scream.ogg | ✅ | ✅ | ✅ |
| biamthelaw.ogg | ✅ | ✅ | ✅ |
| borg_low_power.ogg | ✅ | ❌ | ❌ **НЕТ** |
| borg_reboot.ogg | ✅ | ❌ | ❌ **НЕТ** |
| borg_reboot_complete.ogg | ✅ | ❌ | ❌ **НЕТ** |
| borg_spawn.ogg | ✅ | ❌ | ❌ **НЕТ** |
| borg_emag.ogg | ✅ | ❌ | ❌ **НЕТ** |
| borg_charge.ogg | ✅ | ❌ | ❌ **НЕТ** |
| borg_panel_open.ogg | ✅ | ❌ | ❌ **НЕТ** |
| wire_cut.ogg | ✅ | ❌ | ❌ **НЕТ** |
| wire_pulse.ogg | ✅ | ❌ | ❌ **НЕТ** |
| module_equip.ogg | ✅ | ❌ | ❌ **НЕТ** |
| module_unequip.ogg | ✅ | ❌ | ❌ **НЕТ** |
| sparks.ogg | ✅ | ❌ | ❌ **НЕТ** |
| component_break.ogg | ✅ | ❌ | ❌ **НЕТ** |
| component_install.ogg | ✅ | ❌ | ❌ **НЕТ** |
| headlamp_on.ogg | ✅ | ❌ | ❌ **НЕТ** |
| headlamp_off.ogg | ✅ | ❌ | ❌ **НЕТ** |
| thrusters_on.ogg | ✅ | ❌ | ❌ **НЕТ** |

---

## 23. Консоль управления роботами (Robotics Control Console)

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| Список киборгов (имя, локация, lockdown, HP, батарея, модуль, AI sync, hackable) | ✅ | ❌ | ❌ **НЕТ** |
| Фильтрация (не дроны, не scrambledcodes, один Z-level) | ✅ | ❌ | ❌ **НЕТ** |
| Arm (Safety toggle) | ✅ | ❌ | ❌ **НЕТ** |
| Mass Lock (локдаун всех) | ✅ | ❌ | ❌ **НЕТ** |
| Kill Bot (self_destruct, 60s cd) | ✅ | ❌ | ❌ **НЕТ** |
| Stop Bot | ✅ | ❌ | ❌ **НЕТ** |
| Hack Bot (traitor AI remote emag) | ✅ | ❌ | ❌ **НЕТ** |

---

## 24. Спрайты — дополнительные состояния

| Состояние спрайта | SS13 | ADT | Статус |
|-------------------|------|-----|--------|
| alive (нормальное) | ✅ | ✅ | ✅ |
| alive_low_power (тусклый) | ✅ | ✅ | ✅ |
| dead (мёртвый) | ✅ | ✅ | ✅ |
| rebooting (анимация) | ✅ | ✅ | ✅ |
| emagged (красный) | ✅ | ✅ | ✅ |
| rolled (destroyer — колёсики) | ✅ | ❌ | ❌ **НЕТ** |

---

## 25. Прочие механики

| Механика | SS13 | ADT | Статус |
|----------|------|-----|--------|
| Self-destruct (Kill Bot) | ✅ | ❌ | ❌ **НЕТ** |
| Хирургия роботов (surgery/robotics.dm) | ✅ | ❌ | ❌ **НЕТ** |
| Звуки VOX (1338 файлов) | ✅ | ❌ (не связано) | ❌ **Не входит в объём** |
| PDA приложения (silicon_apps.dm) | ✅ | ❌ | ❌ **НЕТ** |
| Silicon photography | ✅ | ❌ | ❌ **НЕТ** |
| Evil cyborgs (hostile) | ✅ | ❌ | ❌ **НЕТ** |
| Drone subsystem (8 файлов) | ✅ | ❌ | ❌ **НЕТ** |
| pAI | ✅ | ❌ | ❌ **НЕТ** |
| Ion laws JSON (случайные ионные законы) | ✅ | ❌ | ❌ **НЕТ** |
| New space laws (txt) | ✅ | ❌ | ❌ **НЕТ** |
| SS220 модульные добавки (keybinds, signals, etc.) | ✅ | ❌ | ❌ **НЕТ** |
| Wire panel UI (WiresSystem) | ✅ | ✅ | ✅ |
| Keybinds (1,2,3,4,X,Q) | ✅ | ❌ | ❌ **НЕТ** |

---

## Сводка по категориям (актуализировано 2026-07-06)

> **ВНИМАНИЕ:** Эта таблица была составлена на раннем этапе реворка и **завышает** количество неперенесённых механик. Многие пункты были реализованы позже. См. детали в разделах выше.

| Категория | Перенесено | Не перенесено | Примечание |
|-----------|:----------:|:------------:|------------|
| Сборка / Construction | ✅ 8/8 | 0 | 100% |
| MMI / Robotic Brain | ✅ 8/8 | 0 | 100% — C# fallback + RoboticBrain единственный тип (PositronicBrain удалён) |
| Техническая панель | ✅ 7/10 | 3 | ID карта, эмаг в 2 шага |
| Провода (Wires) | ✅ 10/12 | 2 | SS13-точное поведение |
| Связь с ИИ | ✅ 6/9 | 3 | Malf AI удалённый взлом |
| Синхронизация законов | ⚠️ 5/8 | 3 | ion_laws, supplied_laws |
| Модули | ✅ 13/16 | 3 | Drone, Abductor upgrades |
| Языки | ⚠️ 2/3 | 1 | Понимание всех языков |
| Батарея и питание | ⚠️ 6/9 | 3 | Формулы потребления, external power |
| HUD / UI элементы | ⚠️ 10/21 | 11 | Radio, intent, stamina, store, pull, zone, PDA |
| Подсистемы | ❌ 1/6 | 5 | atmos/crew/power monitor, mail_tag |
| Действия (Actions) | ⚠️ 4/6 | 2 | X-ray, engineering scanner cyclic |
| PDA функции | ⚠️ 2/4 | 2 | Call Security, Send PDA Message |
| Компоненты-органы | ✅ 10/13 | 3 | break_component, toggle, is_functioning |
| Повреждения и ремонт | ⚠️ 7/10 | 3 | Слоты ломаются при HP порогах, cable/nanopaste |
| Эмаг | ✅ 8/11 | 3 | Per-module эффекты (предметы), Unemag, Malf |
| Апгрейды (каталог) | ✅ 16/18 | 2 | Abductor (3 шт.), Rename |
| Синди-борги | ⚠️ 5/8 | 3 | scrambledcodes, faction, bluespace cell |
| Система законов (наборы) | ✅ 14/21 | 7 | NT Standard, Robocop, PALADIN и др. (см. borg_law_sets.yml) |
| Клик-обработка | ✅ 4/8 | 4 | **Alt+Click radial + 4 кейбинда ДОБАВЛЕНО** |
| Движение | ⚠️ 6/10 | 4 | Формулы, stam slowdown, destroyer mobility |
| Звуки | ✅ 18/20 | 2 | 18 звуковых коллекций (см. borg_sounds.yml) |
| Консоль управления | ✅ 7/7 | 0 | **RoboticsConsole полностью реализована** |
| Прочие механики | ❌ 2/12 | 10 | Surgery, evil borgs, drone, pAI, ion laws JSON |

**Оценка: перенесено ~70-75% механик** (документ первой оценки занижал).

---

## Топ приоритетных неперенесённых механик (актуализировано)

**Уже выполнено (было в топе, теперь готово):**
- ✅ ~~Консоль управления роботами~~ — RoboticsConsole полностью реализована (174 строки)
- ✅ ~~21 набор законов~~ — 14 наборов (ADTAsimov, ADTSyndicateOverride, ADTMomsimov + 11 в borg_law_sets.yml)
- ✅ ~~Звуки (17 шт.)~~ — 18 звуковых коллекций в borg_sounds.yml
- ✅ ~~Клик-обработка (Alt+Click)~~ — radial меню + 4 кейбинда (bolt/electrify/emergency/toggle)
- ✅ ~~Провода — поведение~~ — SS13-точное (AiControl disconnect, LawCheck lawsync и т.д.)
- ✅ ~~Syndicate MMI~~ — SyndicateImmune flag
- ✅ ~~Техпанель — ID карта~~ — BorgConstruction с multitool-конфигурацией

**Осталось приоритетного:**
1. **HUD элементы** — radio, intent, movement, stamina, module grid, store, pull, zone, PDA, status panel
2. **Per-module emag эффекты** — сейчас только попапы, нужно менять предметы (лазер вместо дисейблера и т.д.)
3. **ion_laws / supplied_laws** — отдельные поля в BorgLawComponent для ионных/загружаемых законов
4. **Подсистемы** — atmos_control, crew_monitor, law_manager, power_monitor, mail_tag (UI-окна)
5. **Mindslave role-tracking** — борг как антагонист эмагнувшего (не только флаг)
6. **Синди-борги конфиг** — scrambledcodes, faction, bluespace cell, damage_protection на chassis
7. **Слоты модулей ломаются при HP порогах** (<50/<0/<-50)
8. ~~**Дефолтный RoboticBrain** при спавне~~ ✅ Выполнено (PositronicBrain удалён, C# fallback добавлен)
9. **Malf AI** — удалённый взлом боргов через консоль (hackbot)
10. **X-ray vision + Engineering scanner cyclic**
