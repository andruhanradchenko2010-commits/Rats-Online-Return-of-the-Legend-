using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DailyChestUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button openChestButton;
    [SerializeField] private TextMeshProUGUI chestStatusText;
    [SerializeField] private Transform questsContainer;
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        if (openChestButton != null)
            openChestButton.onClick.AddListener(OpenChest);

        if (DailyChestManager.Instance != null)
        {
            DailyChestManager.Instance.OnQuestsUpdated += UpdateQuests;
            DailyChestManager.Instance.OnChestOpened += OnChestOpened;
        }

        UpdateUI();
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (DailyChestManager.Instance != null)
        {
            DailyChestManager.Instance.OnQuestsUpdated -= UpdateQuests;
            DailyChestManager.Instance.OnChestOpened -= OnChestOpened;
        }
    }

    private void Update()
    {
        UpdateTimer();
    }

    private void UpdateUI()
    {
        bool canOpen = DailyChestManager.Instance.CanOpenChest();

        if (openChestButton != null)
            openChestButton.interactable = canOpen;

        if (chestStatusText != null)
        {
            int playerLevel = CurrencyManager.Instance.GetPlayerLevel();
            if (playerLevel < 6)
            {
                chestStatusText.text = $"Сундук откроется на 6 уровне\nВаш уровень: {playerLevel}";
            }
            else if (canOpen)
            {
                chestStatusText.text = "Сундук доступен!";
            }
            else
            {
                chestStatusText.text = "Сундук открыт. Выполните задания!";
            }
        }

        UpdateQuests(DailyChestManager.Instance.GetCurrentQuests());
    }

    private void UpdateTimer()
    {
        if (timerText == null) return;

        System.TimeSpan timeUntilNext = DailyChestManager.Instance.GetTimeUntilNextChest();

        if (timeUntilNext.TotalSeconds > 0)
        {
            timerText.text = $"Следующий сундук через: {timeUntilNext.Hours:D2}:{timeUntilNext.Minutes:D2}:{timeUntilNext.Seconds:D2}";
        }
        else
        {
            timerText.text = "Сундук доступен!";
        }
    }

    private void OpenChest()
    {
        DailyChestManager.Instance.OpenChest();
    }

    private void OnChestOpened()
    {
        UpdateUI();
    }

    private void UpdateQuests(List<DailyChestManager.DailyQuest> quests)
    {
        // Очищаем список
        foreach (Transform child in questsContainer)
        {
            Destroy(child.gameObject);
        }

        // Отображаем квесты
        for (int i = 0; i < quests.Count; i++)
        {
            DailyChestManager.DailyQuest quest = quests[i];
            GameObject item = Instantiate(questItemPrefab, questsContainer);

            TextMeshProUGUI descText = item.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI rewardText = item.transform.Find("Reward")?.GetComponent<TextMeshProUGUI>();
            Button completeBtn = item.transform.Find("CompleteButton")?.GetComponent<Button>();
            Image checkmark = item.transform.Find("Checkmark")?.GetComponent<Image>();

            if (descText != null)
                descText.text = quest.description;

            if (rewardText != null)
                rewardText.text = $"Награда: {quest.cheeseReward} сыра, {quest.soulsReward} душ, {quest.elixirsReward} эликсиров";

            if (completeBtn != null)
            {
                int questIndex = i;
                completeBtn.onClick.AddListener(() => CompleteQuest(questIndex));
                completeBtn.gameObject.SetActive(!quest.completed);
            }

            if (checkmark != null)
                checkmark.gameObject.SetActive(quest.completed);
        }
    }

    private void CompleteQuest(int questIndex)
    {
        DailyChestManager.Instance.CompleteQuest(questIndex);
    }
}
