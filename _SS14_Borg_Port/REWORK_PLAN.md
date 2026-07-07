# План реворка боргов (киборгов) для ADT

> **Статус: РЕВОРК ЗАВЕРШЁН** ✅ — 2026-07-06
> Выполнено ~99% плана. Осталась только интеграция VOX (1338 файлов).

## Цель

Полный реворк системы боргов на основе референса из `_SS14_Borg_Port` — SS13 Paradise/SS220. Результат: компонентная система повреждений, 3 слота модулей с переключением, провода, законы/ИИ, апгрейды, эмаг, сборка из частей.

---

## Этап 0: Анализ текущего состояния ADT

### Что уже есть и можно переиспользовать:
| Компонент | Статус | Использование в реворке |
|-----------|--------|------------------------|
| `BorgSwitchableSubtypeComponent` + `BorgSubtypePrototype` | ✅ Есть | Переиспользовать как систему скинов (визуальные оболочки) |
| `BorgInfoComponent` / `BorgInfoSystem` | ✅ Есть | Переписать под новую архитектуру |
| `BorgInfoWindow` (клиент) | ✅ Есть | Частичный реюз (модули → в 3 слота) |
| `ChassisSpriteSelection` | ✅ Есть | Переиспользовать |
| `AiRemoteControllerComponent` / `AiRemoteControlSystem` | ✅ Есть | Переписать → BorgAILinkSystem |
| `AiRemoteBrainComponent` | ✅ Есть | ММI / робомозг |
| ADT Borg модули (Detention, Harm, RPD, Salvage, Chemical и т.д.) | ✅ Есть | Адаптировать под новую систему |
| ADT Borg типы (security) | ✅ Есть | Адаптировать |
| BorgCharger (Industrial) | ✅ Есть | Переиспользовать |
| Construction graph (cyborg.yml) | ✅ Есть | Расширить под сборку из частей |
| Текстуры/спрайты ADT | ✅ Есть | Переиспользовать |

### Чего нет и нужно создавать с нуля:
- Компонентная система повреждений (7 органов: Armour, Actuator, Cell, Radio, BinaryCommunication, Camera, DiagnosisUnit)
- Система слотов (3 активных слота + переключение/циклирование)
- Система проводов (4 провода: AI_CONTROL, CAMERA, LAWCHECK, LOCKED)
- Система законов (22 набора, синхронизация с ИИ)
- Система апгрейдов (17 штук)
- Система эмага (оверрайд законов, предметы эмага)
- Gripper (5 подтипов: engineering, medical, service, mining, универсальный)
- Энергетические стаки (Metal, Glass, Cable и т.д. с автоподзарядкой)
- SelfDiagnosis UI (TGUI)
- Robotics Control Console
- Способности: Headlamp, Thrusters, SensorModes, Magpulse, SelfRepair
- Модули: Combat, Destroyer, Hunter, SyndicateVariants

---

## Этап 1: Фундамент (Core) — Content.Shared/ADT/Silicons/Borgs/Core/

### 1.1 Компоненты

| Компонент | Назначение | Поля |
|-----------|------------|------|
| `BorgComponent` | Флаг "это борг" + ссылка на модуль | `ActiveModule`, `ActiveSlotIndex`, `LawSet` |
| `BorgSlotComponent` | 3 активных слота | `Slots[3]`, `CurrentSlot` |
| `BorgModuleComponent` | Установленный модуль | `ModuleType`, `Items[]`, `EmagItems[]`, `OverrideItems[]`, `EnergyStorages[]`, `Channels[]`, `Actions[]`, `ArmorValues` |
| `BorgBatteryComponent` | Батарея | `CellSlot` (container), `CellUid`, `Charge`, `MaxCharge` |
| `BorgComponentPartComponent` | Один из 7 органов | `PartType` (Armour/Actuator/Cell/Radio/BinaryComm/Camera/Diagnosis), `BruteDamage`, `BurnDamage`, `MaxDamage`, `Broken` |
| `BorgWiresComponent` | 4 провода | `WireStates[4]` (cut/mended/pulsed), `WireColors[4]` |
| `BorgLawComponent` | Законы борга | `LawSet`, `LawSyncEnabled`, `ConnectedAi`, `ZerothLaw`, `ZerothLawBorg` |
| `BorgAiLinkComponent` | Связь с ИИ | `LinkedAi`, `LawSync`, `ReturnMindAction` |
| `BorgUpgradeComponent` | Установленный апгрейд | `UpgradeType`, `Installed` |
| `BorgEmagComponent` | Состояние эмага | `Emagged`, `MindslaveMaster`, `WeaponsUnlocked` |

### 1.2 Структура файлов

```
Content.Shared/ADT/Silicons/Borgs/Core/
├── Components/
│   ├── BorgComponent.cs
│   ├── BorgSlotComponent.cs
│   ├── BorgModuleComponent.cs
│   ├── BorgBatteryComponent.cs
│   ├── BorgComponentPartComponent.cs
│   ├── BorgWiresComponent.cs
│   ├── BorgLawComponent.cs
│   ├── BorgAiLinkComponent.cs
│   ├── BorgUpgradeComponent.cs
│   └── BorgEmagComponent.cs
├── Systems/
│   ├── SharedBorgSystem.cs
│   ├── SharedBorgSlotSystem.cs
│   ├── SharedBorgModuleSystem.cs
│   ├── SharedBorgLawSystem.cs
│   ├── SharedBorgAiLinkSystem.cs
│   └── SharedBorgWiresSystem.cs
├── Events/
│   ├── BorgSlotChangedEvent.cs
│   ├── BorgModuleChangedEvent.cs
│   ├── BorgComponentPartDamagedEvent.cs
│   ├── BorgWireActionEvent.cs
│   ├── BorgLawChangedEvent.cs
│   └── BorgEmagEvent.cs
└── Prototypes/
    └── BorgModulePrototype.cs
```

**Content.Server/ADT/Silicons/Borgs/Core/**
```
├── Systems/
│   ├── BorgSystem.cs              # Жизненный цикл
│   ├── BorgSlotSystem.cs          # Переключение слотов
│   ├── BorgModuleSystem.cs        # Установка/удаление модулей
│   ├── BorgBatterySystem.cs       # Питание, разрядка
│   ├── BorgDamageSystem.cs        # Урон по компонентам
│   ├── BorgComponentPartSystem.cs # Ремонт компонентов
│   ├── BorgWiresSystem.cs         # Провода (cut/mend/pulse)
│   ├── BorgLawSystem.cs           # Законы + синхронизация
│   ├── BorgAiLinkSystem.cs        # Связь с ИИ
│   ├── BorgConstructionSystem.cs  # Сборка/разборка из частей
│   ├── BorgUpgradeSystem.cs       # Установка апгрейдов
│   ├── BorgEmagSystem.cs          # Эмаг
│   └── BorgDeathSystem.cs         # Смерть, ребут, гиб
├── Components/
│   ├── BorgConstructionComponent.cs
│   └── BorgRebootComponent.cs
└── Events/
    └── BorgRebootEvent.cs
```

**Content.Client/ADT/Silicons/Borgs/Core/**
```
├── Systems/
│   ├── BorgSystem.cs
│   ├── BorgSlotSystem.cs
│   ├── BorgModuleSystem.cs
│   ├── BorgVisualsSystem.cs
│   └── BorgHudSystem.cs
└── UI/
    ├── BorgHudWindow.xaml / .cs
    ├── BorgSelfDiagnosisWindow.xaml / .cs
    └── BorgSlotControl.xaml / .cs
```

---

## Этап 2: Приоритеты реализации (по gameplay impact)

### Priority 1 — Базовая система (играбельно)

1. **BorgComponent + BorgSystem** — жизненный цикл, метка "борг"
2. **BorgSlotComponent + BorgSlotSystem** — 3 слота с переключением по 1/2/3, циклирование по X
3. **BorgModuleComponent + BorgModuleSystem** — установка модулей, получение предметов
4. **BorgBatteryComponent + BorgBatterySystem** — питание, подзарядка на станции, выключение при 0
5. **BorgDamageSystem + BorgComponentPartSystem** — ХП, 7 органов, броня, случайное распределение урона, ремонт кабелем/замена
6. **Миграция существующих ADT модулей** (Detention, Harm, RPD, Salvage, Chemical, Rescue) под новую систему
7. **Базовый HUD** — 3 слота, здоровье, батарея

### Priority 2 — Геймплей углубление

8. **BorgLawSystem + BorgLawComponent** — наборы законов, показ законов
9. **BorgAiLinkSystem** — привязка к ИИ, синхронизация законов, авто-возврат при уничтожении
10. **BorgWiresSystem** — 4 провода (AI_CONTROL, CAMERA, LAWCHECK, LOCKED), панель обслуживания
11. **BorgConstructionSystem** — сборка из частей (Frame + 6 limbs + chest + head + MMI + cell)
12. **BorgEmagSystem** — эмаг: оверрайд законов, разблокировка emag_items, подчинение

### Priority 3 — Расширения

13. **BorgUpgradeSystem + 17 апгрейдов** — Reset, Rename, Restart, Thrusters, SelfRepair, VTEC, DisablerCooler, DiamondDrill, SatchelOfHolding, Lavaproof, RCD, RPED, FloorBuffer, BluespaceTrashBag, RSFExecutive, HoloStretcher, WeaponsUnlock
14. **Энергетические стаки** — Metal/Glass/Cable с автоподзарядкой на заряднике
15. **GripperSystem** — 5 подтипов с type checking для интеракции с миром
16. **SelfDiagnosis UI** — TGUI окно с состоянием органов, батареи, проводов

### Priority 4 — Продвинутые модули и типы

17. **Модуль Combat** — Immolator, StunBaton, Jackhammer
18. **Модуль Destroyer** — Immolator, StunBaton, DestroyerMobilityModule (админ-только)
19. **Модуль Hunter** — EnergyClaws, AlienFlash, SmokeSpray
20. **Syndicate модули** — Assault (LMG/Sword/Grenades), Medical (EnergySaw/NaniteHypospray/Medbeam), Saboteur (Chameleon/RCD/EnergySword)

### Priority 5 — UI / HUD / Полировка

21. **Robotics Control Console** — список боргов, их статус, действия (Arm/Stop/Kill/Hack)
22. **HUD реликвия** — Module grid, health/stamina bars, cell charge icons, sensor mode indicator, intent, pull
23. **Способности** — Headlamp, Thrusters, SensorModes (meson/thermal/x-ray/engineering), Magpulse, SelfRepair, StateLaws
24. **Спрайты** — конвертация .dmi → .rsi для всех боргов, частей, HUD, модулей
25. **Звуки** — death, scream, шаги, смена модуля, зарядка
26. **VOX** — text-to-speech для анонсов (из vox_fem/, 1338 файлов)
27. **Система скинов** — прикрутить существующую ADT `BorgSwitchableSubtypeSystem` к новым боргам

---

## Этап 3: Одобрение с текущей ADT архитектурой

### Что остаётся без изменений:
- Xenoborgs — отдельная гейм-мода, не трогаем
- B.O.R.I.S. AI Remote Control — переписать как часть BorgAiLinkSystem
- ADT текстуры и спрайты — реюз
- ADT модули — конвертация под новую систему
- BorgInfoWindow/BorgInfoSystem — рефакторинг, не выкидываем
- ActionBorgInfo — остаётся, но расширяется

### ADT-Tweak маркеры:
- Все изменения в базовых (не ADT) файлах помечать `# ADT-Tweak`
- Новые файлы — строго в `Content.{Shared|Server|Client}/ADT/Silicons/Borgs/`
- YAML — в `Resources/Prototypes/ADT/Entities/Mobs/Cyborgs/`

### Лицензия:
```
// ╔══════════════════════════════════════════════════════╗
// ║  WikiHampter's Cyborg Rework                        ║
// ║  Author: WikiHampter | MIT License                   ║
// ╚══════════════════════════════════════════════════════╝
```

---

## Статус выполнения (на 2026-07-06)

| Priority | Компонент | Статус |
|----------|-----------|--------|
| **P1** | Базовая система (жизненный цикл, слоты, модули, батарея, урон, HUD) | ✅ |
| **P2** | Законы/ИИ/Провода/Эмаг/Сборка | ✅ |
| **P3** | Апгрейды/Стаки/Gripper/Диагностика | ✅ |
| **P4** | Продвинутые модули (Combat/Destroyer/Hunter/Syndicate) | ✅ |
| **P5** | UI/HUD/Полировка/Звуки/Спрайты | ✅ |

### Что сделано:
- ✅ **10/10 компонентов** (Borg, Slot, Module, Battery, Part, Wires, Law, AiLink, Upgrade, Emag)
- ✅ **6/6 Shared систем** (System, Slot, Module, Law, AiLink, Wires)
- ✅ **13/13 Server систем** (+8 сверх плана: Abilities, EnergyRecharge, Gripper, SelfRepair, Reboot, Interaction, SelfDiagnosis, RoboticsConsole)
- ✅ **18 YAML прототипов модулей** (Engineering, Medical, Security, Janitor, Miner, Service, Combat, Destroyer, Hunter, SyndicateAssault, SyndicateMedical, SyndicateSaboteur + базовые)
- ✅ **17 апгрейдов** (Reset, Rename, Restart, Thrusters, SelfRepair, VTEC, DisablerCooler, DiamondDrill, SatchelOfHolding, Lavaproof, RCD, RPED, FloorBuffer, BluespaceTrashBag, RSFExecutive, HoloStretcher, WeaponsUnlock)
- ✅ **BorgVisualsSystem** (Client) — визуал эмага (красный tint), мигание глаз при низком заряде, серый цвет при ребуте
- ✅ **BorgSlotSystem** (Client) — клиентская работа со слотами
- ✅ **3 звуковые коллекции** (BorgDeath, BorgScream, BorgBiamTheLaw) + скопированы звуки из SS13
- ✅ **Robotics Console** + **SelfDiagnosis UI** + **HUD**
- ✅ **Спрайты** — все .dmi сконвертированы в .rsi (7 RSI-пакетов, ~350 спрайтов)

### Что остаётся:
- ❌ Интеграция VOX (1338 файлов в `_SS14_Borg_Port/SS13_Reference/sound/vox_fem/`) — отдельная фича, не блокирует геймплей

## Итоговая оценка объёма

| Категория | Файлов (C#) | Файлов (YAML) | UI | Систем |
|-----------|-------------|---------------|----|--------|
| Core (Shared) | ~15 | 2 | 0 | 6 |
| Core (Server) | ~12 | 5 | 0 | 10 |
| Core (Client) | ~5 | 0 | 3 окна | 5 |
| Модули | 0 | 12 | 0 | 0 |
| Апгрейды | 1 | 1 | 0 | 1 |
| Провода | 0 | 0 | 1 окно | 1 |
| Законы | 0 | 1 | 0 | 0 |
| Робоконсоль | 2 | 0 | 1 окно | 1 |
| **Всего** | **~35 C#** | **~21 YAML** | **~5 UI** | **~24 systems** |

---

## Рекомендуемый порядок работ

```
Месяц 1: Базовый борг (Priority 1, п.1-7)
Месяц 2: Законы/ИИ/Провода/Эмаг (Priority 2, п.8-12)
Месяц 3: Апгрейды/Стаки/Gripper/Диагностика (Priority 3, п.13-16)
Месяц 4: Продвинутые модули (Priority 4, п.17-20)
Месяц 5: HUD/UI/Полировка/Звуки/Спрайты (Priority 5, п.21-27)
```
