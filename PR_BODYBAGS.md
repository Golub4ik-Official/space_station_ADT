## Описание PR

Добавлены новые типы мешков для трупов (блюспейс, защитные, для заключённых, стазис, длительного хранения тел), голографическая каталка, обновления механик каталок, а также системы для замедления гниения, защиты от огня и заполнения воздухом.

## Почему / Баланс

**Цель изменений:**
- Дать медицинскому персоналу больше инструментов для работы с телами (разные типы мешков для разных ситуаций)
- Добавить антагонистам (Синдикат) инструменты для похищения (мешок Синдиката) в аплинк
- Улучшить баланс игры (замедление буксировки загруженных мешков/каталок)
- Исправить визуальные проблемы с пристёгнутыми пациентами (DrawDepth)

**Влияние на баланс:**
- **Блюспейс мешки:** удобство для медиков (помогают убрать много тел), но требуют исследований
- **Защитные мешки:** защита от атмосферных явлений и огня, но не защитят от прямого урона, требуют исследований
- **Стазис-мешки:** мощный инструмент для сохранения тел, но одноразовый, требует исследований
- **Мешок Синдиката:** даёт Синдикату способ похищать жертв без атаки, стоит 4 ТК в аплинке
- **Голо-каталка:** компактная альтернатива обычной каталке для тех, у кого нет места, требует исследований
- **Замедление при буксировке:** делает невозможным быстро уносить тела

## Техническая информация

### Добавленные файлы (Server):
- `Content.Server/ADT/Medical/BodyBags/BodyBagGasServerSystem.cs` — заполнение воздуха при закрытии мешков
- `Content.Server/ADT/Medical/BodyBags/BodyBagFireProtectionSystem.cs` — тушение огня внутри защитных мешков
- `Content.Server/ADT/Medical/BodyBags/StasisBodyBagServerSystem.cs` — серверная часть стазис-мешков
- `Content.Server/ADT/Medical/BodyBags/LostCrewBodyBagSystem.cs` — спавн лута в мешках LostCrew

### Добавленные файлы (Shared):
- `Content.Shared/ADT/Medical/BodyBags/BodyBagRotSlowComponent.cs` — компонент замедления гниения
- `Content.Shared/ADT/Medical/BodyBags/BodyBagOccupantHealthSystem.cs` — осмотр здоровья через мешок (удалён, но компонент остался)
- `Content.Shared/ADT/Medical/BodyBags/BodyBagOccupantHealthComponent.cs` — компонент осмотра здоровья (не используется)
- `Content.Shared/ADT/Medical/BodyBags/BodyBagGasComponent.cs` — компонент заполнения воздуха
- `Content.Shared/ADT/Medical/BodyBags/BodyBagGasSystem.cs` — клиентская часть заполнения воздуха
- `Content.Shared/ADT/Medical/BodyBags/BodyBagCinchComponent.cs` — компонент стяжек
- `Content.Shared/ADT/Medical/BodyBags/BodyBagCinchSystem.cs` — система стяжек
- `Content.Shared/ADT/Medical/BodyBags/BodyBagEliteComponent.cs` — компонент элитной защиты (огонь + разрушаемость)
- `Content.Shared/ADT/Medical/BodyBags/BluespaceBodyBagComponent.cs` — компонент блюспейс мешка
- `Content.Shared/ADT/Medical/BodyBags/BluespaceBodyBagSystem.cs` — система блюспейс мешков (размер, складывание, побег)
- `Content.Shared/ADT/Medical/BodyBags/StasisBodyBagComponent.cs` — компонент стазис-мешка
- `Content.Shared/ADT/Medical/BodyBags/StasisBodyBagOccupantComponent.cs` — компонент стазиса для occupant'а
- `Content.Shared/ADT/Medical/BodyBags/StasisBodyBagSystem.cs` — система стазис-мешков
- `Content.Shared/ADT/Medical/BodyBags/LostCrewBodyBagComponent.cs` — компонент мешка LostCrew
- `Content.Shared/ADT/Medical/BodyBags/PaperLabelBodyBagSystem.cs` — система подписей мешков бумажками
- `Content.Shared/ADT/Medical/BodyBags/RollerBedPullSlowComponent.cs` — компонент замедления буксировки каталок
- `Content.Shared/ADT/Medical/BodyBags/RollerBedPullSlowSystem.cs` — система замедления буксировки
- `Content.Shared/ADT/Medical/BodyBags/BodyBagStretcherComponent.cs` — компонент размещения мешков на каталках
- `Content.Shared/ADT/Medical/BodyBags/BodyBagStretcherSystem.cs` — система размещения мешков на каталках
- `Content.Shared/ADT/Medical/BodyBags/BodyBagStretcherDoAfterEvent.cs` — ивент doAfter для размещения мешка

### Новые типы мешков (прототипы):
1. **BodyBagBluespace** — вмещает 15 тел, меняет размер при наполнении, нельзя сложить если >50% заполнен или содержит контейнеры, побег из сложенного мешка
2. **BodyBagEnvironmental** — герметичный, защита от атмосферных явлений, заполнение воздухом O₂+N₂, защита от огня
3. **BodyBagEnvironmentalNanotrasen** — элитная версия защитного мешка, заполняется MedicalHeal газом, полностью защищён от огня и разрушения
4. **BodyBagPrisoner** — защитный мешок + механизм стяжек (Cinch/Uncinch), затянутый мешок не открывается без ослабления
5. **BodyBagSyndicate** — мешок для заключённых + заполнение N₂O+O₂ (усыпляющий газ), доступен в аплинке за 4 ТК
6. **BodyBagStasis** — одноразовый, тушит огонь, поддерживает -60°C, замедляет метаболизм ×10, разрушается через ~10 минут
7. **Мешок для длительного хранения тел (BodyBagLostCrew)** — спавнит тело и лут при первом открытии, вмещает 15 тел

### Новые предметы:
- **HoloStretcher** — голографическая каталка, помещается в карман (Small), быстрое пристёгивание (2 сек), получает исследование ADTHoloRollerBed (Tier 3, 7500)

### Новые прототипы:
- **Ресурсы/Прототипы/ADT/Entities/Objects/Specific/Medical/morgue.yml** — все новые типы мешков
- **Ресурсы/Прототипы/ADT/Entities/Structures/Furniture/holo_stretchers.yml** — голографическая каталка
- **Ресурсы/Прототипы/ADT/Entities/Markers/Spawners/Random/lost_crew_loot.yml** — таблицы лута для мешков LostCrew
- **Ресурсы/Прототипы/ADT/Catalog/Fills/Boxes/medical.yml** — коробки для всех типов мешков
- **Ресурсы/Прототипы/ADT/Catalog/uplink_catalog.yml** — мешок Синдиката в аплинке
- **Ресурсы/Прототипы/ADT/Recipes/Lathes/medical.yml** — рецепты печати мешков
- **Ресурсы/Прототипы/ADT/Recipes/Lathes/Packs/medical.yml** — паки рецептов для лата
- **Ресурсы/Прототипы/ADT/Research/biochemical.yml** — исследования для новых мешков и голо-каталки
- **Ресурсы/Прототипы/ADT/Recipes/Crafting/bodybag_box.yml** — крафт коробки мешков
- **Ресурсы/Прототипы/ADT/Recipes/Crafting/Graphs/storage/bodybag_box.yml** — граф крафта коробки

### Исследования для новых предметов:

| Предмет | Исследование | Тир | Стоимость |
|---------|--------------|-----|----------|
| **Защитный мешок для выживания** (BodyBagEnvironmental) | Продвинутая медицина (ADTAdvancedMedicalCare) | 2 | 10000 |
| **Мешок для транспортировки заключённых** (BodyBagPrisoner) | Продвинутая медицина (ADTAdvancedMedicalCare) | 2 | 7500 |
| **Стазисный мешок для тела** (BodyBagStasis) | Криогенный стазис (ADTCryoTech) | 3 | 10000 |
| **Блюспейс мешок для тела** (BodyBagBluespace) | Блюспейс-химия (ADTBluespaceChemistry) | 3 | 10000 |
| **Голографическая каталка** (HoloStretcher) | Голографическая каталка (ADTHoloRollerBed) | 3 | 5000 |

### Замедление при буксировке:

| Ситуация | Скорость ходьбы | Скорость бега |
|----------|----------------|--------------|
| **Тащить тело без мешка** | 1.0 (нет замедления) | 1.0 (нет замедления) |
| **Тащить тело в мешке** | 0.55 | 0.65 |
| **Тащить тело на обычной каталке** | 0.7 | 0.8 |
| **Тащить тело на голо-каталке** | 1.0 (нет замедления) | 1.0 (нет замедления) |
| **Тащить тело в мешке на обычной каталке** | 0.7 | 0.8 |
| **Тащить тело в мешке на голо-каталке** | 1.0 (нет замедления) | 1.0 (нет замедления) |

**Примечание:** Замедление каталки (0.7/0.8) имеет приоритет над замедлением мешка (0.55/0.65). Если мешок лежит на обычной каталке, применяется замедление каталки. Если мешок лежит на голо-каталке, замедления нет.

### Изменения в существующих системах:
- **SharedBuckleSystem.Buckle.cs** — каталки всегда требуют doAfter (привязано к BodyBagStretcherComponent)
- **FlammableSystem.cs (Server)** — добавлен GetFireProtectionEvent для проверки защиты от огня (BodyBagFireProtectionSystem)
- **RottingSystem.cs (Server)** — поддерживает BodyBagRotSlowComponent для замедления гниения
- **PullingSystem.cs (Shared)** — замедление при буксировке загруженных мешков (0.55/0.65) и каталок (RollerBedPullSlowComponent)
- **Resources/Prototypes/Entities/Structures/Furniture/rollerbeds.yml** — добавлены BodyBagStretcherComponent, RollerBedPullSlowComponent, DrawDepth DeadMobs, размер Huge
- **Resources/Prototypes/Entities/foldable.yml** — canFoldInsideContainer: true для разворачиваемых объектов
- **Resources/Prototypes/Guidebook/medical.yml** — добавлены гайды BodyBags и Stretchers
- **Resources/Prototypes/Entities/Objects/Specific/Medical/morgue.yml** — BodyBag заменён на BodyBagRotSlow, добавлен Buckle, Flammable, Damageable, Injurable, Destructible

### Изменения в наполнении шкафов:
- **ADTLockerParamedicFilledNoMod** — добавлен BoxBodyBagStasis
- **LockerParamedicFilled** — добавлен BoxBodyBagStasis
- **ADTLockerPathologistFilled** — добавлено 2 коробки BoxBodyBag
- **LockerBrigmedicFilled** — добавлен BoxBodyBagPrisoner
- **LockerBrigmedicFilledHardsuit** — наследует LockerBrigmedicFilled

### Изменения в аплинках:
- **ADTBaseUplinkBOBERT (категория Прочее)** — добавлены:
  - BoxBodyBagEnvironmentalNanotrasen — 2 ЕТ
  - HoloStretcherFolded — 2 ЕТ

### Изменения спрайтов:
- **holo_stretcher.rsi** — обновлены in-hand спрайты (LHand/RHand)
- **meta.json** — добавлен кредит автору egor2444 за in-hand спрайты

### Критические изменения:
- Изменение DrawDepth в BuckleSystem — персонажи теперь рендерятся над каталками/мешками (вместо под ними)
- BodyBagFireProtectionSystem тушит огонь внутри мешков при изменении OnFire
- RottingSystem теперь поддерживает BodyBagRotSlowComponent для замедления гниения
- PullingSystem применяет замедление при буксировке загруженных мешков (0.55/0.65) и каталок
- Все мешки теперь наследуют BodyBagRotSlowComponent (DecayMultiplier = 0.33) вместо AntiRottingContainer (кроме стазис и LostCrew)

### Локализация:
- `Resources/Locale/en-US/ADT/bluespace-body-bag.ftl` — локализация блюспейс мешка
- `Resources/Locale/en-US/ADT/body-bag-cinch.ftl` — локализация стяжек
- `Resources/Locale/en-US/ADT/stasis-body-bag.ftl` — локализация стазис мешка
- `Resources/Locale/en-US/ADT/lost-crew-body-bag.ftl` — локализация мешка LostCrew
- `Resources/Locale/en-US/ADT/paper/paper-label-cut.ftl` — локализация срезания метки
- `Resources/Locale/ru-RU/ADT/bluespace-body-bag.ftl` — русская локализация блюспейс мешка
- `Resources/Locale/ru-RU/ADT/body-bag-cinch.ftl` — русская локализация стяжек
- `Resources/Locale/ru-RU/ADT/stasis-body-bag.ftl` — русская локализация стазис мешка
- `Resources/Locale/ru-RU/ADT/paper/paper-label-cut.ftl` — русская локализация срезания метки
- Обновлены файлы `Resources/Locale/en-US/ss14-ru/prototypes/entities/objects/specific/medical/morgue.ftl` и `Resources/Locale/ru-RU/ss14-ru/prototypes/entities/objects/specific/medical/morgue.ftl` с описаниями всех новых мешков

### Гайдбук:
- `Resources/ServerInfo/Guidebook/Medical/BodyBags.xml` — гайд по мешкам
- `Resources/ServerInfo/Guidebook/Medical/Stretchers.xml` — гайд по каталкам

### Спрайты:
- Новые спрайты для всех типов мешков в `Resources/Textures/Objects/Specific/Medical/Morgue/bodybags.rsi/`
- Спрайты голографической каталки в `Resources/Textures/ADT/Structures/Furniture/holo_stretcher.rsi/`

## Медиа

[TODO: Добавить скриншоты новых мешков, каталок, гайдов и т.д.]

## Чейнджлог

:cl: WikiHampter
- add: Добавлены новые типы мешков для трупов (блюспейс, защитные, для заключённых, стазис, LostCrew)
- add: Добавлена голографическая каталка (HoloStretcher)
- add: Добавлена система замедления гниения тел внутри мешков (BodyBagRotSlowComponent)
- add: Добавлена система защиты от огня для мешков (BodyBagFireProtectionSystem)
- add: Добавлена система заполнения воздуха при закрытии мешков (BodyBagGasSystem)
- add: Добавлена система стяжек для мешков заключённых (BodyBagCinchSystem)
- add: Добавлена система стазиса для стазис-мешков (StasisBodyBagSystem)
- add: Добавлена система спавна лута для мешков LostCrew (LostCrewBodyBagSystem)
- add: Добавлена система размещения мешков на каталках (BodyBagStretcherSystem)
- add: Добавлена система замедления буксировки загруженных каталок (RollerBedPullSlowSystem)
- add: Добавлен BoxBodyBagStasis в шкафы парамедиков (ADTLockerParamedicFilledNoMod, LockerParamedicFilled)
- add: Добавлено 2 коробки BoxBodyBag в шкаф патологоанатома (ADTLockerPathologistFilled)
- add: Добавлен BoxBodyBagPrisoner в шкафы бриг медика (LockerBrigmedicFilled, LockerBrigmedicFilledHardsuit)
- add: Добавлены BoxBodyBagEnvironmentalNanotrasen и HoloStretcherFolded в аплинк Б.О.Б.Р.Т. (категория Прочее, 2 ЕТ каждый)
- tweak: Исправлен DrawDepth в BuckleSystem — персонажи теперь рендерятся над каталками
- tweak: Замедление при буксировке загруженных мешков (0.55/0.65 скорости)
- tweak: Замедление при буксировке загруженных каталок (RollerBedPullSlowComponent)
- tweak: Обычный мешок теперь замедляет гниение в 3 раза (DecayMultiplier = 0.33)
- tweak: Время пристёгивания к каталкам: 2 секунды для всех типов
- tweak: Размер обычной каталки: Huge
- tweak: Обновлены in-hand спрайты голографической каталки (автор: egor2444)
