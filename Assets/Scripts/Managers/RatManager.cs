using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class RatManager : SingletonManager<RatManager>
{
    private List<Rat> rats = new List<Rat>();

    // Отложенное сохранение: мутации крыс лишь помечают флаг, а тяжёлая сериализация
    // всего списка выполняется пачкой (см. FlushRats).
    private bool ratsDirty = false;
    private const float RATS_FLUSH_INTERVAL = 3f;

    [Header("Heal Settings")]
    [Tooltip("Время восстановления подбитой крысы в секундах")]
    [SerializeField] private float healTimeSeconds = GameConfig.NORMAL_HEAL_TIME;

    [Header("Test Settings")]
    [Tooltip("Включить быстрое лечение для тестирования")]
    [SerializeField] private bool fastHealMode = false;

    [Tooltip("Время быстрого лечения в секундах")]
    [SerializeField] private float fastHealTimeSeconds = GameConfig.FAST_HEAL_TIME;

    // Текущее время лечения с учётом тестового режима
    public float GetCurrentHealTime() => fastHealMode ? fastHealTimeSeconds : healTimeSeconds;

    public event Action<Rat> OnRatAdded;
    public event Action<Rat> OnRatRemoved;
    public event Action<Rat> OnRatUpdated;

    protected override void OnInitialize()
    {
        ApplyTestModeSettings();
        LoadRats();
        InvokeRepeating(nameof(CheckAndHealBeatenRats), 1f, 1f);
        InvokeRepeating(nameof(CheckOverfedRats), 60f, 60f);
        InvokeRepeating(nameof(FlushRats), RATS_FLUSH_INTERVAL, RATS_FLUSH_INTERVAL);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CancelInvoke(nameof(CheckAndHealBeatenRats));
        CancelInvoke(nameof(CheckOverfedRats));
        CancelInvoke(nameof(FlushRats));
        FlushRats();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) FlushRats();
    }

    private void OnApplicationQuit()
    {
        FlushRats();
    }

    private void ApplyTestModeSettings()
    {
        if (GameConfig.IsTestMode)
        {
            fastHealMode = true;
            fastHealTimeSeconds = GameConfig.FAST_HEAL_TIME;
            GameLog.Log($"🎮 RatManager: ТЕСТОВЫЙ РЕЖИМ (лечение {fastHealTimeSeconds} сек)");
        }
        else
        {
            fastHealMode = false;
            GameLog.Log($"⚔️ RatManager: НОРМАЛЬНЫЙ РЕЖИМ (лечение {healTimeSeconds} сек)");
        }
    }

    private void CheckAndHealBeatenRats()
    {
        float healTime = fastHealMode ? fastHealTimeSeconds : healTimeSeconds;

        foreach (var rat in rats)
        {
            if (rat.state == RatState.Beaten)
            {
                if (Time.time - rat.healStartTime >= healTime)
                {
                    rat.Heal();
                    OnRatUpdated?.Invoke(rat);
                }
            }
        }
    }

    // Создание новой крысы
    public Rat CreateRat(RatType type, int level)
    {
        Rat rat = new Rat(type, level);
        rats.Add(rat);
        OnRatAdded?.Invoke(rat);
        SaveRats();
        return rat;
    }

    // Создание крысы за души
    public Rat CreateRatFromSouls()
    {
        int baseLevel = CurrencyManager.Instance.GetBaseRatLevel();
        int soulCost = GetSoulCostForLevel(baseLevel);

        if (CurrencyManager.Instance.SpendSouls(soulCost))
        {
            Rat rat = CreateRat(RatType.Gray, baseLevel);
            CurrencyManager.Instance.AddPlayerExp(10); // Опыт за создание крысы
            return rat;
        }
        return null;
    }

    private int GetSoulCostForLevel(int level)
    {
        return 10 + (level * 5); // Чем выше базовый уровень, тем больше душ
    }

    public int GetCurrentSoulCost()
    {
        return GetSoulCostForLevel(CurrencyManager.Instance.GetBaseRatLevel());
    }

    // Скрещивание крыс
    public Rat BreedRats(Rat rat1, Rat rat2, int elixirCost = 1)
    {
        // Проверки
        if (rat1.type != rat2.type)
        {
            Debug.LogError("Нельзя скрещивать крыс разных видов!");
            return null;
        }

        if (rat1.level != rat1.type.GetMaxLevel() || rat2.level != rat2.type.GetMaxLevel())
        {
            Debug.LogError("Обе крысы должны быть максимального уровня для своего вида!");
            return null;
        }

        if (rat1.type == RatType.BatRat)
        {
            Debug.LogError("Bat Rat - максимальная эволюция!");
            return null;
        }

        if (!CurrencyManager.Instance.SpendLoveElixirs(elixirCost))
        {
            Debug.LogError("Недостаточно любовных эликсиров!");
            return null;
        }

        // Создаем новую крысу следующего вида
        RatType newType = (RatType)((int)rat1.type + 1);
        int newLevel = newType.GetMinLevel();

        // Суммируем оставшиеся кормежки родителей
        int totalFeeds = rat1.feedsRemaining + rat2.feedsRemaining;
        newLevel += Mathf.Min(totalFeeds, newType.GetMaxLevel() - newLevel);

        Rat newRat = CreateRat(newType, newLevel);

        // Суммируем умения родителей
        newRat.theftSkill = rat1.theftSkill + rat2.theftSkill;
        newRat.miningSkill = rat1.miningSkill + rat2.miningSkill;
        newRat.defenseSkill = rat1.defenseSkill + rat2.defenseSkill;
        newRat.attackSkill = rat1.attackSkill + rat2.attackSkill;

        // Удаляем родителей
        RemoveRat(rat1);
        RemoveRat(rat2);

        CurrencyManager.Instance.AddPlayerExp(50); // Опыт за скрещивание

        return newRat;
    }

    // Прокачка умения
    public bool UpgradeSkill(Rat rat, RatSkill skill, int points)
    {
        int cost = rat.type.GetUpgradeCost() * points;

        if (!CurrencyManager.Instance.SpendCheese(cost))
            return false;

        switch (skill)
        {
            case RatSkill.Theft:
                rat.theftSkill += points;
                break;
            case RatSkill.Mining:
                rat.miningSkill += points;
                break;
            case RatSkill.Defense:
                rat.defenseSkill += points;
                break;
            case RatSkill.Attack:
                rat.attackSkill += points;
                break;
        }

        OnRatUpdated?.Invoke(rat);
        SaveRats();
        return true;
    }

    // Кормление крысы
    public bool FeedRat(Rat rat)
    {
        if (!rat.isHungry)
            return false;

        int cheeseNeeded = rat.level;
        if (!CurrencyManager.Instance.SpendCheese(cheeseNeeded))
            return false;

        rat.Feed(cheeseNeeded);

        // Добавляем опыт игроку
        int expGained = rat.type.GetExpPerFeed();
        CurrencyManager.Instance.AddPlayerExp(expGained);

        OnRatUpdated?.Invoke(rat);
        SaveRats();
        return true;
    }

    // Получение случайных крыс для битвы
    public List<Rat> GetRandomBattleRats(int count = 5)
    {
        var healthyRats = rats.Where(r => r.CanFight()).ToList();

        if (healthyRats.Count <= count)
            return healthyRats;

        // Перемешиваем и берем первые count крыс
        return healthyRats.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
    }

    // Лечение
    public bool HealBeatenRat(Rat rat, bool instant = false)
    {
        if (rat.state != RatState.Beaten)
            return false;

        if (instant)
        {
            // Чудо-бинт (5 бинтов = 10 крысобаксов)
            if (CurrencyManager.Instance.SpendRatBucks(10))
            {
                rat.Heal();
                OnRatUpdated?.Invoke(rat);
                SaveRats();
                return true;
            }
        }
        else
        {
            // Проверяем, прошло ли достаточно времени
            if (Time.time - rat.healStartTime >= healTimeSeconds)
            {
                rat.Heal();
                OnRatUpdated?.Invoke(rat);
                SaveRats();
                return true;
            }
        }

        return false;
    }

    public bool HealOverfedRat(Rat rat, bool superDFR = false)
    {
        if (rat.state != RatState.Overfed)
            return false;

        if (superDFR)
        {
            // СуперДФР - мгновенное лечение
            if (CurrencyManager.Instance.SpendRatBucks(20))
            {
                rat.Heal();
                OnRatUpdated?.Invoke(rat);
                SaveRats();
                return true;
            }
        }
        else
        {
            // Простой ДФР - нужно несколько штук
            if (rat.defibrillatorCount > 0)
            {
                rat.defibrillatorCount--;
                if (rat.defibrillatorCount <= 0)
                {
                    rat.Heal();
                }
                OnRatUpdated?.Invoke(rat);
                SaveRats();
                return true;
            }
        }

        return false;
    }

    public bool ReviveRat(Rat rat)
    {
        if (rat.state != RatState.Dead)
            return false;

        // Зелье оживления
        if (CurrencyManager.Instance.SpendRatBucks(50))
        {
            rat.Heal();
            OnRatUpdated?.Invoke(rat);
            SaveRats();
            return true;
        }

        return false;
    }

    // Проверка смерти закормленных крыс
    public void CheckOverfedRats()
    {
        float deathTime = GameConfig.GetOverfedDeathTime();
        bool anyDied = false;

        foreach (var rat in rats)
        {
            if (rat.state == RatState.Overfed && Time.time - rat.overfedTime >= deathTime)
            {
                rat.Kill();
                OnRatUpdated?.Invoke(rat);
                anyDied = true;
            }
        }

        if (anyDied)
            SaveRats();
    }

    public void RemoveRat(Rat rat)
    {
        rats.Remove(rat);
        OnRatRemoved?.Invoke(rat);
        SaveRats();
    }

    public IReadOnlyList<Rat> GetAllRats() => rats;

    public int GetRatCount() => rats.Count;
    public int GetHealthyRatCount() => rats.Count(r => r.CanFight());

    // Сохранение/загрузка
    // SaveRats только помечает данные «грязными» — фактическая JSON-сериализация
    // всего списка делается пачкой в FlushRats, а не на каждое изменение крысы.
    private void SaveRats()
    {
        ratsDirty = true;
    }

    private void FlushRats()
    {
        if (!ratsDirty) return;
        RatListWrapper wrapper = new RatListWrapper { rats = rats };
        SaveSystem.SaveObject("Rats", wrapper);
        SaveSystem.Save();
        ratsDirty = false;
    }

    private void LoadRats()
    {
        RatListWrapper wrapper = SaveSystem.LoadObject("Rats", new RatListWrapper());
        rats = wrapper.rats ?? new List<Rat>();

        if (rats.Count == 0)
        {
            CreateRat(RatType.Gray, 1);
        }
    }

    [System.Serializable]
    private class RatListWrapper
    {
        public List<Rat> rats;
    }
}
