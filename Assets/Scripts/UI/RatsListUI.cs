using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RatsListUI : RatAwareUI
{
    [Header("UI Elements")]
    [SerializeField] private Transform ratListContainer;
    [SerializeField] private GameObject ratItemPrefab;
    [SerializeField] private Button createRatButton;
    [SerializeField] private TextMeshProUGUI soulCostText;
    [SerializeField] private TextMeshProUGUI ratCountText;

    [Header("Rat Detail Panel")]
    [SerializeField] private GameObject ratDetailPanel;
    [SerializeField] private Image ratDetailImage;
    [SerializeField] private TextMeshProUGUI ratDetailName;
    [SerializeField] private TextMeshProUGUI ratDetailLevel;
    [SerializeField] private TextMeshProUGUI ratDetailState;
    [SerializeField] private TextMeshProUGUI ratDetailStats;
    [SerializeField] private Button feedButton;
    [SerializeField] private Button healButton;
    [SerializeField] private Button upgradeTheftButton;
    [SerializeField] private Button upgradeMiningButton;
    [SerializeField] private Button upgradeDefenseButton;
    [SerializeField] private Button upgradeAttackButton;

    private Rat selectedRat;

    protected override void OnStart()
    {
        if (createRatButton != null)
            createRatButton.onClick.AddListener(CreateNewRat);

        if (feedButton != null) feedButton.onClick.AddListener(FeedSelectedRat);
        if (healButton != null) healButton.onClick.AddListener(HealSelectedRat);
        if (upgradeTheftButton != null) upgradeTheftButton.onClick.AddListener(() => UpgradeSkill(RatSkill.Theft));
        if (upgradeMiningButton != null) upgradeMiningButton.onClick.AddListener(() => UpgradeSkill(RatSkill.Mining));
        if (upgradeDefenseButton != null) upgradeDefenseButton.onClick.AddListener(() => UpgradeSkill(RatSkill.Defense));
        if (upgradeAttackButton != null) upgradeAttackButton.onClick.AddListener(() => UpgradeSkill(RatSkill.Attack));

        if (ratDetailPanel != null)
            ratDetailPanel.SetActive(false);

        RefreshRatList();
        UpdateSoulCost();
    }

    protected override void OnRatChanged(Rat rat)
    {
        RefreshRatList();
        if (selectedRat != null && selectedRat.id == rat.id)
        {
            ShowRatDetail(rat);
        }
    }

    private void RefreshRatList()
    {
        UIHelper.ClearContainer(ratListContainer);

        // Заполняем список крыс
        IReadOnlyList<Rat> rats = RatManager.Instance.GetAllRats();
        foreach (Rat rat in rats)
        {
            GameObject item = Instantiate(ratItemPrefab, ratListContainer);
            RatListItem ratItem = item.GetComponent<RatListItem>();
            if (ratItem != null)
            {
                ratItem.Setup(rat, () => ShowRatDetail(rat));
            }
        }

        if (ratCountText != null)
            ratCountText.text = $"Крыс: {rats.Count}";
    }

    private void ShowRatDetail(Rat rat)
    {
        selectedRat = rat;

        if (ratDetailPanel != null)
            ratDetailPanel.SetActive(true);

        if (ratDetailName != null)
            ratDetailName.text = rat.type.GetDisplayName();

        if (ratDetailLevel != null)
            ratDetailLevel.text = $"Уровень {rat.level}";

        if (ratDetailState != null)
        {
            string stateText = rat.state switch
            {
                RatState.Healthy => "Здорова",
                RatState.Beaten => "Прибита",
                RatState.Overfed => "Закормлена",
                RatState.Dead => "Мертва",
                _ => "Неизвестно"
            };
            ratDetailState.text = $"Состояние: {stateText}";
        }

        if (ratDetailStats != null)
        {
            ratDetailStats.text = $"Воровство: {rat.theftSkill}\n" +
                                 $"Добыча: {rat.miningSkill}\n" +
                                 $"Защита: {rat.defenseSkill}\n" +
                                 $"Атака: {rat.attackSkill}\n" +
                                 $"Кормежек до уровня: {rat.feedsRemaining}";
        }

        // Обновляем кнопки
        if (feedButton != null)
            feedButton.interactable = rat.isHungry && rat.CanFight();

        if (healButton != null)
            healButton.interactable = rat.state != RatState.Healthy;
    }

    private void CreateNewRat()
    {
        Rat newRat = RatManager.Instance.CreateRatFromSouls();
        if (newRat != null)
        {
            GameLog.Log("Создана новая крыса!");
            UpdateSoulCost();
        }
        else
        {
            GameLog.Log("Недостаточно душ!");
        }
    }

    private void UpdateSoulCost()
    {
        if (soulCostText != null)
        {
            int cost = RatManager.Instance.GetCurrentSoulCost();
            soulCostText.text = $"Стоимость: {cost} душ";
        }
    }

    private void FeedSelectedRat()
    {
        if (selectedRat != null)
        {
            if (RatManager.Instance.FeedRat(selectedRat))
            {
                GameLog.Log("Крыса покормлена!");
                ShowRatDetail(selectedRat);
            }
            else
            {
                GameLog.Log("Недостаточно сыра или крыса не голодна!");
            }
        }
    }

    private void HealSelectedRat()
    {
        if (selectedRat == null) return;

        bool healed = false;

        switch (selectedRat.state)
        {
            case RatState.Beaten:
                healed = RatManager.Instance.HealBeatenRat(selectedRat, true);
                break;
            case RatState.Overfed:
                healed = RatManager.Instance.HealOverfedRat(selectedRat, true);
                break;
            case RatState.Dead:
                healed = RatManager.Instance.ReviveRat(selectedRat);
                break;
        }

        if (healed)
        {
            GameLog.Log("Крыса вылечена!");
            ShowRatDetail(selectedRat);
        }
        else
        {
            GameLog.Log("Недостаточно крысобаксов!");
        }
    }

    private void UpgradeSkill(RatSkill skill)
    {
        if (selectedRat != null)
        {
            if (RatManager.Instance.UpgradeSkill(selectedRat, skill, 1))
            {
                GameLog.Log($"Умение {skill} прокачано!");
                ShowRatDetail(selectedRat);
            }
            else
            {
                GameLog.Log("Недостаточно сыра!");
            }
        }
    }

    public void CloseDetailPanel()
    {
        if (ratDetailPanel != null)
            ratDetailPanel.SetActive(false);
        selectedRat = null;
    }
}
