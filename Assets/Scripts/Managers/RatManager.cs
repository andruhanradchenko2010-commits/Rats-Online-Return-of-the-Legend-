using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class RatManager : MonoBehaviour
{
    public static RatManager Instance;

    private List<Rat> rats = new List<Rat>();

    public event Action<Rat> OnRatAdded;
    public event Action<Rat> OnRatRemoved;
    public event Action<Rat> OnRatUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRats();
        }
        else
        {
            Destroy(gameObject);
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
            float healTime = rat.level * 60f; // 1 минута на уровень (упрощенно)
            if (Time.time - rat.healStartTime >= healTime)
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
        float deathTime = 3 * 24 * 60 * 60; // 3 дня в секундах

        foreach (var rat in rats.Where(r => r.state == RatState.Overfed))
        {
            if (Time.time - rat.overfedTime >= deathTime)
            {
                rat.Kill();
                OnRatUpdated?.Invoke(rat);
            }
        }

        SaveRats();
    }

    public void RemoveRat(Rat rat)
    {
        rats.Remove(rat);
        OnRatRemoved?.Invoke(rat);
        SaveRats();
    }

    public List<Rat> GetAllRats() => new List<Rat>(rats);
    public int GetRatCount() => rats.Count;
    public int GetHealthyRatCount() => rats.Count(r => r.CanFight());

    // Сохранение/загрузка
    private void SaveRats()
    {
        string json = JsonUtility.ToJson(new RatListWrapper { rats = rats });
        PlayerPrefs.SetString("Rats", json);
        PlayerPrefs.Save();
    }

    private void LoadRats()
    {
        string json = PlayerPrefs.GetString("Rats", "");
        if (!string.IsNullOrEmpty(json))
        {
            RatListWrapper wrapper = JsonUtility.FromJson<RatListWrapper>(json);
            rats = wrapper.rats ?? new List<Rat>();
        }
        else
        {
            // Создаем стартовых крыс
            CreateRat(RatType.Gray, 1);
            CreateRat(RatType.Gray, 1);
            CreateRat(RatType.Gray, 2);
        }
    }

    [System.Serializable]
    private class RatListWrapper
    {
        public List<Rat> rats;
    }
}
