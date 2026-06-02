using UnityEngine;
using System;

public class CurrencyManager : SingletonManager<CurrencyManager>
{
    // Валюты
    private int cheese = 0;
    private int loveElixirs = 0;
    private int souls = 0;
    private int ratBucks = 0;
    private int playerExp = 0;
    private int playerLevel = 1;

    // События для обновления UI
    public event Action<int> OnCheeseChanged;
    public event Action<int> OnLoveElixirsChanged;
    public event Action<int> OnSoulsChanged;
    public event Action<int> OnRatBucksChanged;
    public event Action<int, int> OnPlayerExpChanged; // exp, level
    public event Action<int> OnPlayerLevelUp;

    protected override void OnInitialize()
    {
        LoadData();
    }

    // Сыр
    public void AddCheese(int amount)
    {
        cheese += amount;
        Debug.Log($"CurrencyManager.AddCheese: +{amount} сыра. Всего: {cheese}. Подписчиков на событие: {OnCheeseChanged?.GetInvocationList().Length ?? 0}");
        OnCheeseChanged?.Invoke(cheese);
        SaveData();
    }

    public bool SpendCheese(int amount)
    {
        if (cheese >= amount)
        {
            cheese -= amount;
            OnCheeseChanged?.Invoke(cheese);
            SaveData();
            return true;
        }
        return false;
    }

    public int GetCheese() => cheese;

    // Любовные эликсиры
    public void AddLoveElixirs(int amount)
    {
        loveElixirs += amount;
        OnLoveElixirsChanged?.Invoke(loveElixirs);
        SaveData();
    }

    public bool SpendLoveElixirs(int amount)
    {
        if (loveElixirs >= amount)
        {
            loveElixirs -= amount;
            OnLoveElixirsChanged?.Invoke(loveElixirs);
            SaveData();
            return true;
        }
        return false;
    }

    public int GetLoveElixirs() => loveElixirs;

    // Души
    public void AddSouls(int amount)
    {
        souls += amount;
        OnSoulsChanged?.Invoke(souls);
        SaveData();
    }

    public bool SpendSouls(int amount)
    {
        if (souls >= amount)
        {
            souls -= amount;
            OnSoulsChanged?.Invoke(souls);
            SaveData();
            return true;
        }
        return false;
    }

    public int GetSouls() => souls;

    // Крысобаксы
    public void AddRatBucks(int amount)
    {
        ratBucks += amount;
        OnRatBucksChanged?.Invoke(ratBucks);
        SaveData();
    }

    public bool SpendRatBucks(int amount)
    {
        if (ratBucks >= amount)
        {
            ratBucks -= amount;
            OnRatBucksChanged?.Invoke(ratBucks);
            SaveData();
            return true;
        }
        return false;
    }

    public int GetRatBucks() => ratBucks;

    // Опыт и уровень игрока
    public void AddPlayerExp(int amount)
    {
        playerExp += amount;
        CheckLevelUp();
        OnPlayerExpChanged?.Invoke(playerExp, playerLevel);
        SaveData();
    }

    private void CheckLevelUp()
    {
        int expNeeded = GetExpForNextLevel();
        while (playerExp >= expNeeded && playerLevel < 999)
        {
            playerExp -= expNeeded;
            playerLevel++;
            ratBucks += GameConfig.RAT_BUCKS_PER_LEVEL;
            OnPlayerLevelUp?.Invoke(playerLevel);
            OnRatBucksChanged?.Invoke(ratBucks);
            expNeeded = GetExpForNextLevel();
        }
    }

    private int GetExpForNextLevel()
    {
        // Прогрессивная формула опыта
        return 100 + (playerLevel * 50);
    }

    public int GetPlayerLevel() => playerLevel;
    public int GetPlayerExp() => playerExp;
    public int GetExpForNext() => GetExpForNextLevel();

    // Базовый уровень новорожденных крыс (растет с уровнем игрока)
    public int GetBaseRatLevel()
    {
        if (playerLevel < 10) return 1;
        if (playerLevel < 30) return 2;
        if (playerLevel < 60) return 3;
        if (playerLevel < 100) return 4;
        return 5;
    }

    // Сохранение/загрузка
    private void SaveData()
    {
        SaveSystem.SaveInt("Cheese", cheese);
        SaveSystem.SaveInt("LoveElixirs", loveElixirs);
        SaveSystem.SaveInt("Souls", souls);
        SaveSystem.SaveInt("RatBucks", ratBucks);
        SaveSystem.SaveInt("PlayerExp", playerExp);
        SaveSystem.SaveInt("PlayerLevel", playerLevel);
        SaveSystem.Save();
    }

    private void LoadData()
    {
        cheese = SaveSystem.LoadInt("Cheese", 100);
        loveElixirs = SaveSystem.LoadInt("LoveElixirs", 5);
        souls = SaveSystem.LoadInt("Souls", 10);
        ratBucks = SaveSystem.LoadInt("RatBucks", 50);
        playerExp = SaveSystem.LoadInt("PlayerExp", 0);
        playerLevel = SaveSystem.LoadInt("PlayerLevel", 1);

        // Уведомляем UI
        OnCheeseChanged?.Invoke(cheese);
        OnLoveElixirsChanged?.Invoke(loveElixirs);
        OnSoulsChanged?.Invoke(souls);
        OnRatBucksChanged?.Invoke(ratBucks);
        OnPlayerExpChanged?.Invoke(playerExp, playerLevel);
    }

    public void ResetData()
    {
        cheese = 100;
        loveElixirs = 5;
        souls = 10;
        ratBucks = 50;
        playerExp = 0;
        playerLevel = 1;
        SaveData();
    }
}
