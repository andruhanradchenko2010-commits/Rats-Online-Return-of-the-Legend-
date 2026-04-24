# Rats Online: Return of the Legend

## 📋 Содержание

- [Технические характеристики](#технические-характеристики)
- [Архитектура проекта](#архитектура-проекта)
- [Игровые системы](#игровые-системы)
- [Структура проекта](#структура-проекта)
- [Установка и запуск](#установка-и-запуск)
- [Разработка](#разработка)
- [API Reference](#api-reference)

---

## 🎮 Технические характеристики

- **Engine:** Unity 6000.4.0f1
- **Language:** C# (.NET Standard 2.1)
- **Architecture:** Manager-based Singleton pattern
- **UI Framework:** Unity UI + TextMeshPro
- **Data Persistence:** PlayerPrefs
- **Lines of Code:** ~4,163

---

## 🏗️ Архитектура проекта

### Паттерны проектирования

**1. Singleton Pattern**
Все менеджеры используют Singleton для глобального доступа:
```csharp
public static GameManager Instance;
```

**2. Event-Driven Architecture**
Менеджеры используют события для связи с UI:
```csharp
public event Action<int> OnCheeseChanged;
public event Action<Rat> OnRatAdded;
```

**3. Manager Pattern**
Разделение ответственности по менеджерам:
- `GameManager` - главный координатор
- `CurrencyManager` - управление валютами
- `RatManager` - управление крысами
- `BattleManager` - боевая система
- `ArenaManager` - мультиплеер арена
- `DungeonManager` - система подземелья
- `DailyChestManager` - ежедневные задания
- `InventoryManager` - предметы и экипировка

**4. Data-Driven Design**
Все данные крыс и предметов описаны через enum и extension methods:
```csharp
public enum RatType { Gray, Royal, Angel, Devil, Vampire, Joker, BatRat }
public static int GetMinLevel(this RatType type) { ... }
```

### Преимущества архитектуры

✅ **Масштабируемость** - легко добавлять новые менеджеры и системы
✅ **Разделение ответственности** - каждый менеджер отвечает за свою область
✅ **Слабая связанность** - компоненты общаются через события
✅ **DontDestroyOnLoad** - менеджеры сохраняются между сценами
✅ **Тестируемость** - логика отделена от UI

### Потенциальные улучшения

⚠️ **PlayerPrefs** - рекомендуется заменить на JSON/Binary сериализацию для сложных данных
⚠️ **Dependency Injection** - можно добавить DI контейнер для лучшей тестируемости
⚠️ **ScriptableObjects** - использовать для конфигурации крыс и предметов
⚠️ **Addressables** - для оптимизации загрузки ассетов

---

## 🎯 Игровые системы

### 1. Система крыс

#### 7 видов крыс (эволюция)

| Вид | Уровни | Опыт/кормежка | Стоимость прокачки |
|-----|--------|---------------|-------------------|
| Серая крыса | 1-5 | +1 | 228 сыра |
| Царская крыса | 6-10 | +3 | 850 сыра |
| Ангельская крыса | 11-15 | +10 | 1,750 сыра |
| Дьявольская крыса | 16-20 | +20 | 6,918 сыра |
| Вампир | 21-25 | +30 | 34,785 сыра |
| Джокер | 26-40 | +50 | 832,720 сыра |
| Bat Rat | 41-55+ | +70 | 999,999 сыра |

#### 4 умения крыс

1. **Воровство** - для кражи сыра у других игроков
2. **Добыча** - для сбора ресурсов
3. **Защита** - защита от атак врагов
4. **Атака** - для нападения на вражеских крыс

**Важно:** Каждой крысе можно прокачать только ОДНО умение, но при скрещивании умения родителей суммируются.

#### Состояния крыс

- **Healthy** - здоровая, может участвовать в битвах
- **Beaten** - прибитая, требует лечения Кушеткой или Чудо-бинтом
- **Overfed** - закормленная, требует ДФР, умрет через 3 дня
- **Dead** - мертвая, требует Зелье оживления

#### Прокачка крыс

```
1. Крыса участвует в битве → становится голодной
2. Игрок кормит крысу сыром (количество = уровень крысы)
3. Игрок получает опыт (зависит от вида крысы)
4. После N кормежек крыса переходит на следующий уровень
```

#### Скрещивание

```
Требования:
- 2 крысы ОДНОГО вида
- Обе крысы максимального уровня для своего вида
- 1+ Любовный эликсир

Результат:
- Крыса следующего вида
- Уровень = минимальный для вида + бонус от оставшихся кормежек
- Умения родителей СУММИРУЮТСЯ
- Родители удаляются
```

**Пример:**
```
2x Серая крыса (5 lvl) + 1 эликсир → Царская крыса (6 lvl)
2x Царская крыса (10 lvl) + 1 эликсир → Ангельская крыса (11 lvl)
```

---

### 2. Валюты

| Валюта | Назначение | Как получить |
|--------|-----------|--------------|
| **Сыр** | Кормление крыс, прокачка умений | Воровство, турниры, задания |
| **Любовные эликсиры** | Скрещивание крыс | Закормка врагов, задания |
| **Души** | Создание новых крыс | Закормка врагов, турниры |
| **Крысобаксы** | Премиум предметы | +13 за каждый уровень игрока |

---

### 3. Боевая система

#### PvP битвы

```
Механика:
1. Выбираются 5 случайных здоровых крыс игрока
2. Генерируется AI противник с крысами
3. Расчет силы:
   - Атакующий: theftSkill + attackSkill + level
   - Защитник: defenseSkill + level
4. Шанс победы = playerPower / (playerPower + enemyPower)
5. Победа → кража сыра, шанс закормить врагов
6. Поражение → крысы прибиты (50% шанс)
7. Все крысы становятся голодными
```

#### Закармливание

- Крыса может закормить только крысу **своего уровня или ниже**
- Серая крыса (1-5) не может закормить Царскую (6+)
- За закормку: души + любовные эликсиры
- Закормленная крыса умрет через 3 дня без лечения

#### Турниры

| Лига | Уровни игрока | Награды |
|------|---------------|---------|
| Бронзовая | 1-15 | Базовые |
| Серебряная | 15-50 | Средние |
| Золотая | 51-150 | Хорошие |
| Алмазная | 151+ | Лучшие |

**Механика турнира:**
- 10 раундов против AI противников
- Награды зависят от итогового места (1-10)
- 1 место: 500 сыра, 50 душ, 10 эликсиров

---

### 4. Арена (Мультиплеер)

**Режим игры:**
- До 8 игроков одновременно
- Вид сверху (2D)
- Цель: собрать больше всего еды за 3 минуты
- Еда появляется каждые 2 секунды (макс. 20 на поле)

**Управление:**
- Движение крысы по арене
- Автоматический сбор еды при приближении
- Ограничение границами арены

**Награды:**
- Зависят от места в таблице лидеров
- 1 место: ~200 сыра
- Последнее место: ~50 сыра

**Текущая реализация:** AI игроки (сетевой код требует доработки)

---

### 5. Подземелье

**Механика:**
- Сетка 10x10 клеток
- 70% клеток имеют стены
- Стартовая клетка (0,0) - без стены

**Ломание стен:**
```
Требования:
- Рядом должен быть проложенный путь
- Стоимость = 50 + (x + y) * 10 сыра

Награды:
- Случайное количество сыра (10-50)
- Случайное количество душ (1-5)
```

---

### 6. Ежедневный сундук

**Открытие:**
- Доступен с 6 уровня игрока
- Раз в 24 часа
- Дает 3 случайных задания

**Типы заданий:**
- Украсть N сыра
- Покормить N крыс
- Скрестить 2 крысы
- Выиграть N битв
- Прокачать N умений

**Награды:**
- За каждое задание: сыр + души + эликсиры
- За все 3 задания: +200 сыра, +10 крысобаксов

---

### 7. Предметы и экипировка

#### Лечебные предметы

| Предмет | Назначение | Стоимость |
|---------|-----------|-----------|
| Чудо-бинт | Мгновенное лечение прибитой крысы | 10 крысобаксов |
| Простой ДФР | Лечение закормленной (нужно несколько) | 5 крысобаксов |
| СуперДФР | Мгновенное лечение закормленной | 20 крысобаксов |
| Зелье оживления | Воскрешение мертвой крысы | 50 крысобаксов |

#### Экипировка

| Предмет | Эффект | Стоимость |
|---------|--------|-----------|
| Коричневая перчатка | +10 к атаке | 100 крысобаксов |
| Шляпка | +15 к воровству | 150 крысобаксов |

#### Постройки

| Постройка | Эффект | Стоимость |
|-----------|--------|-----------|
| Кушетка | +50% скорость лечения за уровень | 5,000 сыра |

---

## 📁 Структура проекта

```
Rats-Online-Return-of-the-Legend/
├── Assets/
│   ├── Scenes/
│   │   ├── MainWindow.unity          # Главное меню
│   │   ├── StealGame.unity           # Сцена воровства
│   │   └── SampleScene.unity         # Тестовая сцена
│   │
│   ├── Scripts/
│   │   ├── Data/
│   │   │   ├── Rat.cs                # Класс крысы
│   │   │   └── RatType.cs            # Типы, состояния, умения
│   │   │
│   │   ├── Managers/
│   │   │   ├── CurrencyManager.cs    # Управление валютами
│   │   │   ├── RatManager.cs         # Управление крысами
│   │   │   ├── BattleManager.cs      # Боевая система
│   │   │   ├── ArenaManager.cs       # Мультиплеер арена
│   │   │   ├── DungeonManager.cs     # Система подземелья
│   │   │   ├── DailyChestManager.cs  # Ежедневные задания
│   │   │   └── InventoryManager.cs   # Предметы и экипировка
│   │   │
│   │   ├── UI/
│   │   │   ├── MainMenuUI.cs         # Главное меню
│   │   │   ├── BattleUI.cs           # UI битв
│   │   │   ├── BreedingUI.cs         # UI скрещивания
│   │   │   ├── RatsListUI.cs         # Список крыс
│   │   │   ├── ShopUI.cs             # Магазин
│   │   │   ├── TournamentUI.cs       # Турниры
│   │   │   ├── ArenaUI.cs            # Арена
│   │   │   ├── DungeonUI.cs          # Подземелье
│   │   │   └── DailyChestUI.cs       # Ежедневный сундук
│   │   │
│   │   ├── Helpers/
│   │   │   └── SpriteHelper.cs       # Утилиты для спрайтов
│   │   │
│   │   ├── GameManager.cs            # Главный менеджер
│   │   ├── GameInitializer.cs        # Инициализация игры
│   │   ├── Scene1Controller.cs       # Контроллер сцены 1
│   │   ├── Scene2Controller.cs       # Контроллер сцены 2
│   │   └── BarrelRewardController.cs # Награды из бочки
│   │
│   ├── Sprite/
│   │   ├── Location/                 # Спрайты локаций
│   │   ├── Бочка сыра.png           # Бочка с сыром
│   │   └── ...                       # Другие спрайты
│   │
│   ├── Sounds/
│   │   ├── Фоновая музыка.mp3
│   │   ├── Музыка при атаке.mp3
│   │   └── Звук лечения крысы.mp3
│   │
│   ├── Prefabs/
│   │   └── CheesePrefab.prefab       # Префаб сыра
│   │
│   └── InputSystem_Actions.inputactions  # Input System
│
├── ProjectSettings/                   # Настройки Unity
├── Packages/                          # Unity пакеты
├── .gitignore                         # Git ignore
└── README.md                          # Этот файл
```

---

## 🚀 Установка и запуск

### Требования

- Unity 6000.4.0f1 или новее
- Git
- 2 GB свободного места

### Установка

```bash
# Клонировать репозиторий
git clone https://github.com/andruhanradchenko2010-commits/Rats-Online-Return-of-the-Legend-.git

# Открыть проект в Unity Hub
# File → Open Project → Выбрать папку проекта
```

### Первый запуск

1. Открыть сцену `Assets/Scenes/MainWindow.unity`
2. Нажать Play в Unity Editor
3. Стартовые ресурсы:
   - 100 сыра
   - 5 любовных эликсиров
   - 10 душ
   - 50 крысобаксов
   - 3 серые крысы (1-2 уровня)

---

## 🛠️ Разработка

### Добавление нового вида крысы

1. Добавить в `RatType` enum:
```csharp
public enum RatType {
    Gray, Royal, Angel, Devil, Vampire, Joker, BatRat,
    NewRatType  // Новый вид
}
```

2. Обновить extension methods в `RatTypeExtensions`:
```csharp
public static int GetMinLevel(this RatType type) {
    switch (type) {
        case RatType.NewRatType: return 56;
        // ...
    }
}
```

3. Добавить спрайт в `Assets/Sprite/`

### Добавление нового предмета

1. Добавить в `ItemType` enum:
```csharp
public enum ItemType {
    WonderBandage, SimpleDefib, SuperDefib, RevivalPotion,
    BrownGlove, Hat, Couch,
    NewItem  // Новый предмет
}
```

2. Обновить конструктор `Item`:
```csharp
case ItemType.NewItem:
    name = "Название";
    description = "Описание";
    ratBucksCost = 100;
    break;
```

3. Добавить логику использования в `InventoryManager.UseItem()`

### Добавление нового менеджера

```csharp
public class NewManager : MonoBehaviour
{
    public static NewManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

Добавить инициализацию в `GameManager.InitializeManagers()`

### Сохранение данных

Использовать `PlayerPrefs` для простых данных:
```csharp
PlayerPrefs.SetInt("Key", value);
PlayerPrefs.GetInt("Key", defaultValue);
PlayerPrefs.Save();
```

Для сложных данных использовать `JsonUtility`:
```csharp
string json = JsonUtility.ToJson(data);
PlayerPrefs.SetString("Key", json);
```

---

## 📚 API Reference

### GameManager

```csharp
// Singleton
GameManager.Instance

// Методы
void AddCheese(int amount)
int GetCheeseCount()
void SetPendingReward(int amount)
bool ShouldShowBarrel()
void TriggerBarrelReward()
```

### CurrencyManager

```csharp
// Singleton
CurrencyManager.Instance

// Валюты
void AddCheese(int amount)
bool SpendCheese(int amount)
int GetCheese()

void AddLoveElixirs(int amount)
bool SpendLoveElixirs(int amount)
int GetLoveElixirs()

void AddSouls(int amount)
bool SpendSouls(int amount)
int GetSouls()

void AddRatBucks(int amount)
bool SpendRatBucks(int amount)
int GetRatBucks()

// Опыт и уровень
void AddPlayerExp(int amount)
int GetPlayerLevel()
int GetPlayerExp()
int GetExpForNext()
int GetBaseRatLevel()

// События
event Action<int> OnCheeseChanged
event Action<int> OnLoveElixirsChanged
event Action<int> OnSoulsChanged
event Action<int> OnRatBucksChanged
event Action<int, int> OnPlayerExpChanged
event Action<int> OnPlayerLevelUp
```

### RatManager

```csharp
// Singleton
RatManager.Instance

// Создание крыс
Rat CreateRat(RatType type, int level)
Rat CreateRatFromSouls()
int GetCurrentSoulCost()

// Скрещивание
Rat BreedRats(Rat rat1, Rat rat2, int elixirCost = 1)

// Прокачка
bool UpgradeSkill(Rat rat, RatSkill skill, int points)
bool FeedRat(Rat rat)

// Битвы
List<Rat> GetRandomBattleRats(int count = 5)

// Лечение
bool HealBeatenRat(Rat rat, bool instant = false)
bool HealOverfedRat(Rat rat, bool superDFR = false)
bool ReviveRat(Rat rat)
void CheckOverfedRats()

// Управление
void RemoveRat(Rat rat)
List<Rat> GetAllRats()
int GetRatCount()
int GetHealthyRatCount()

// События
event Action<Rat> OnRatAdded
event Action<Rat> OnRatRemoved
event Action<Rat> OnRatUpdated
```

### BattleManager

```csharp
// Singleton
BattleManager.Instance

// PvP
BattleResult AttackPlayer(List<Rat> enemyRats, int enemyCheese)
BattleResult DefendAgainstAttack(List<Rat> attackerRats)
List<Rat> GenerateEnemyRats(int playerLevel)

// Турниры
TournamentResult PlayTournament()

// Классы результатов
class BattleResult {
    bool victory
    int cheeseStolen
    int soulsGained
    int elixirsGained
    List<Rat> beatenRats
    List<Rat> overfedEnemies
}

class TournamentResult {
    int place
    int cheeseReward
    int soulsReward
    int elixirsReward
}
```

### ArenaManager

```csharp
// Singleton
ArenaManager.Instance

// Управление раундом
void StartRound()
bool IsRoundActive()
float GetRemainingTime()

// Движение
void MoveLocalPlayer(Vector2 direction)

// Данные
List<ArenaPlayer> GetPlayers()
List<FoodItem> GetFoodItems()
Vector2 GetArenaSize()

// Классы
class ArenaPlayer {
    string id
    string name
    Vector2 position
    int score
    Color color
    bool isLocalPlayer
}

class FoodItem {
    Vector2 position
    int value
    bool collected
}
```

### DungeonManager

```csharp
// Singleton
DungeonManager.Instance

// Управление
bool BreakWall(int x, int y)
void ResetDungeon()

// Данные
DungeonCell GetCell(int x, int y)
int GetWidth()
int GetHeight()
int GetBreakCost(int x, int y)

// Класс
class DungeonCell {
    int x
    int y
    bool hasWall
    bool isPath
    int cheeseReward
    int soulsReward
}
```

### DailyChestManager

```csharp
// Singleton
DailyChestManager.Instance

// Управление
bool CanOpenChest()
void OpenChest()
void CheckChestAvailability()
void CompleteQuest(int questIndex)

// Данные
List<DailyQuest> GetCurrentQuests()
TimeSpan GetTimeUntilNextChest()

// События
event Action<List<DailyQuest>> OnQuestsUpdated
event Action OnChestOpened

// Класс
class DailyQuest {
    string description
    bool completed
    int cheeseReward
    int soulsReward
    int elixirsReward
}
```

### InventoryManager

```csharp
// Singleton
InventoryManager.Instance

// Покупка и использование
bool BuyItem(ItemType type, int quantity = 1)
bool UseItem(ItemType type, Rat rat = null)

// Экипировка
bool EquipItem(string ratId, ItemType type)
void UnequipItem(string ratId)
ItemType? GetEquippedItem(string ratId)

// Данные
int GetItemQuantity(ItemType type)
Item GetItem(ItemType type)
Dictionary<ItemType, Item> GetAllItems()

// Постройки
int GetCouchLevel()
float GetHealSpeedMultiplier()

// События
Action OnInventoryChanged
```

### Rat (Data Class)

```csharp
// Конструктор
Rat(RatType ratType, int startLevel)

// Свойства
string id
RatType type
int level
RatState state

// Умения
int theftSkill
int miningSkill
int defenseSkill
int attackSkill

// Прогресс
int feedsRemaining
bool isHungry

// Лечение
float healStartTime
int defibrillatorCount
float overfedTime

Sprite sprite

// Методы
int GetTotalPower()
bool CanLevelUp()
bool CanEvolve()
void Feed(int cheeseAmount)
void SetHungry()
void Beat()
void Overfeed()
void Kill()
void Heal()
bool CanFight()
bool CanOverfeed(Rat enemy)
```

---

## 🎨 Ассеты

### Звуки

- `Фоновая музыка.mp3` - основная музыка игры
- `Музыка при атаке.mp3` - музыка во время битв
- `Звук лечения крысы.mp3` - звук при лечении

### Спрайты

- Локации (папка `Location/`)
- Бочка сыра с анимацией
- Спрайты крыс (требуется добавить для всех видов)

---

## 🐛 Известные проблемы

1. **Мультиплеер арены** - реализован только AI, требуется сетевой код
2. **Спрайты крыс** - не все виды крыс имеют уникальные спрайты
3. **Сохранение** - PlayerPrefs не подходит для больших объемов данных
4. **UI** - некоторые UI компоненты требуют доработки

---

## 🔮 Планы развития

### Краткосрочные (v1.0)
- [ ] Добавить все спрайты крыс
- [ ] Реализовать полный UI для всех систем
- [ ] Добавить звуковые эффекты
- [ ] Балансировка игровой экономики

### Среднесрочные (v2.0)
- [ ] Реальный мультиплеер (Photon/Mirror)
- [ ] Система достижений
- [ ] Больше видов турниров
- [ ] Система гильдий/кланов

### Долгосрочные (v3.0)
- [ ] Мобильная версия (iOS/Android)
- [ ] Система рейтингов
- [ ] Сезонные события
- [ ] Кастомизация крыс

---

## 📄 Лицензия

Проект создан в образовательных целях для воссоздания классической игры "Крысы Online".

---

## 👥 Авторы

- **Воссоздание:** Rats Online: Return of the Legend Team

---
