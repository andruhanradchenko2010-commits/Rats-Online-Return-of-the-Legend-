using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyChestManager : MonoBehaviour
{
    public static DailyChestManager Instance;

    [System.Serializable]
    public class DailyQuest
    {
        public string description;
        public bool completed;
        public int cheeseReward;
        public int soulsReward;
        public int elixirsReward;
    }

    private List<DailyQuest> currentQuests = new List<DailyQuest>();
    private DateTime lastChestTime;
    private bool chestAvailable = false;

    public event Action<List<DailyQuest>> OnQuestsUpdated;
    public event Action OnChestOpened;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CheckChestAvailability();
    }

    public bool CanOpenChest()
    {
        return CurrencyManager.Instance.GetPlayerLevel() >= 6 && chestAvailable;
    }

    public void CheckChestAvailability()
    {
        TimeSpan timeSinceLastChest = DateTime.Now - lastChestTime;

        // Сундук доступен раз в 24 часа
        if (timeSinceLastChest.TotalHours >= 24)
        {
            chestAvailable = true;
            GenerateQuests();
        }
    }

    public void OpenChest()
    {
        if (!CanOpenChest())
            return;

        chestAvailable = false;
        lastChestTime = DateTime.Now;
        GenerateQuests();
        SaveData();

        OnChestOpened?.Invoke();
    }

    private void GenerateQuests()
    {
        currentQuests.Clear();

        int playerLevel = CurrencyManager.Instance.GetPlayerLevel();

        // Генерируем 3 случайных задания
        List<string> questTypes = new List<string>
        {
            "steal_cheese", "feed_rats", "breed_rats", "win_battles", "upgrade_skills"
        };

        for (int i = 0; i < 3; i++)
        {
            string questType = questTypes[UnityEngine.Random.Range(0, questTypes.Count)];
            questTypes.Remove(questType);

            DailyQuest quest = new DailyQuest
            {
                completed = false,
                cheeseReward = 50 + playerLevel * 10,
                soulsReward = 5 + playerLevel,
                elixirsReward = 1 + playerLevel / 10
            };

            switch (questType)
            {
                case "steal_cheese":
                    quest.description = $"Украсть {100 + playerLevel * 10} сыра";
                    break;
                case "feed_rats":
                    quest.description = $"Покормить {5 + playerLevel / 5} крыс";
                    break;
                case "breed_rats":
                    quest.description = "Скрестить 2 крысы";
                    break;
                case "win_battles":
                    quest.description = $"Выиграть {3 + playerLevel / 10} битв";
                    break;
                case "upgrade_skills":
                    quest.description = $"Прокачать {5 + playerLevel / 5} умений";
                    break;
            }

            currentQuests.Add(quest);
        }

        OnQuestsUpdated?.Invoke(currentQuests);
        SaveData();
    }

    public void CompleteQuest(int questIndex)
    {
        if (questIndex < 0 || questIndex >= currentQuests.Count)
            return;

        DailyQuest quest = currentQuests[questIndex];
        if (quest.completed)
            return;

        quest.completed = true;

        // Выдаем награды
        CurrencyManager.Instance.AddCheese(quest.cheeseReward);
        CurrencyManager.Instance.AddSouls(quest.soulsReward);
        CurrencyManager.Instance.AddLoveElixirs(quest.elixirsReward);

        OnQuestsUpdated?.Invoke(currentQuests);
        SaveData();

        // Проверяем, все ли задания выполнены
        CheckAllQuestsCompleted();
    }

    private void CheckAllQuestsCompleted()
    {
        bool allCompleted = true;
        foreach (var quest in currentQuests)
        {
            if (!quest.completed)
            {
                allCompleted = false;
                break;
            }
        }

        if (allCompleted)
        {
            // Бонусная награда за выполнение всех заданий
            CurrencyManager.Instance.AddCheese(200);
            CurrencyManager.Instance.AddRatBucks(10);
        }
    }

    public List<DailyQuest> GetCurrentQuests()
    {
        return new List<DailyQuest>(currentQuests);
    }

    public TimeSpan GetTimeUntilNextChest()
    {
        TimeSpan timeSinceLastChest = DateTime.Now - lastChestTime;
        TimeSpan timeUntilNext = TimeSpan.FromHours(24) - timeSinceLastChest;
        return timeUntilNext.TotalSeconds > 0 ? timeUntilNext : TimeSpan.Zero;
    }

    private void SaveData()
    {
        PlayerPrefs.SetString("LastChestTime", lastChestTime.ToString());
        PlayerPrefs.SetInt("ChestAvailable", chestAvailable ? 1 : 0);

        // Сохраняем квесты
        for (int i = 0; i < currentQuests.Count; i++)
        {
            PlayerPrefs.SetString($"Quest_{i}_Desc", currentQuests[i].description);
            PlayerPrefs.SetInt($"Quest_{i}_Completed", currentQuests[i].completed ? 1 : 0);
            PlayerPrefs.SetInt($"Quest_{i}_Cheese", currentQuests[i].cheeseReward);
            PlayerPrefs.SetInt($"Quest_{i}_Souls", currentQuests[i].soulsReward);
            PlayerPrefs.SetInt($"Quest_{i}_Elixirs", currentQuests[i].elixirsReward);
        }
        PlayerPrefs.SetInt("QuestCount", currentQuests.Count);

        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        string lastChestStr = PlayerPrefs.GetString("LastChestTime", "");
        if (string.IsNullOrEmpty(lastChestStr))
        {
            lastChestTime = DateTime.Now.AddHours(-25); // Сундук доступен сразу
        }
        else
        {
            DateTime.TryParse(lastChestStr, out lastChestTime);
        }

        chestAvailable = PlayerPrefs.GetInt("ChestAvailable", 0) == 1;

        // Загружаем квесты
        int questCount = PlayerPrefs.GetInt("QuestCount", 0);
        currentQuests.Clear();

        for (int i = 0; i < questCount; i++)
        {
            DailyQuest quest = new DailyQuest
            {
                description = PlayerPrefs.GetString($"Quest_{i}_Desc", ""),
                completed = PlayerPrefs.GetInt($"Quest_{i}_Completed", 0) == 1,
                cheeseReward = PlayerPrefs.GetInt($"Quest_{i}_Cheese", 0),
                soulsReward = PlayerPrefs.GetInt($"Quest_{i}_Souls", 0),
                elixirsReward = PlayerPrefs.GetInt($"Quest_{i}_Elixirs", 0)
            };
            currentQuests.Add(quest);
        }
    }
}
