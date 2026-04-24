using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform enemyListContainer;
    [SerializeField] private GameObject enemyItemPrefab;
    [SerializeField] private Button attackButton;
    [SerializeField] private TextMeshProUGUI battleResultText;
    [SerializeField] private GameObject battleResultPanel;
    [SerializeField] private TextMeshProUGUI playerRatsInfoText;

    [Header("Battle Animation")]
    [SerializeField] private GameObject battleAnimationPanel;
    [SerializeField] private Transform playerRatsContainer;
    [SerializeField] private Transform enemyRatsContainer;

    private List<Rat> currentEnemyRats;
    private int currentEnemyCheese;

    private void Start()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(StartBattle);

        if (battleResultPanel != null)
            battleResultPanel.SetActive(false);

        if (battleAnimationPanel != null)
            battleAnimationPanel.SetActive(false);

        GenerateNewEnemy();
    }

    private void OnEnable()
    {
        GenerateNewEnemy();
        UpdatePlayerRatsInfo();
    }

    private void GenerateNewEnemy()
    {
        int playerLevel = CurrencyManager.Instance.GetPlayerLevel();
        currentEnemyRats = BattleManager.Instance.GenerateEnemyRats(playerLevel);
        currentEnemyCheese = Random.Range(100, 500);

        DisplayEnemyRats();
    }

    private void DisplayEnemyRats()
    {
        // Очищаем список
        foreach (Transform child in enemyListContainer)
        {
            Destroy(child.gameObject);
        }

        // Отображаем вражеских крыс
        foreach (Rat rat in currentEnemyRats)
        {
            GameObject item = Instantiate(enemyItemPrefab, enemyListContainer);
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
            {
                text.text = $"{rat.type.GetDisplayName()} Ур.{rat.level}\n" +
                           $"Защита: {rat.defenseSkill} | Атака: {rat.attackSkill}";
            }
        }
    }

    private void UpdatePlayerRatsInfo()
    {
        if (playerRatsInfoText == null) return;

        List<Rat> playerRats = RatManager.Instance.GetRandomBattleRats(5);
        int totalPower = 0;

        string info = "Ваши крысы в бою:\n";
        foreach (Rat rat in playerRats)
        {
            info += $"• {rat.type.GetDisplayName()} Ур.{rat.level}\n";
            totalPower += rat.GetTotalPower();
        }

        info += $"\nОбщая сила: {totalPower}";
        playerRatsInfoText.text = info;
    }

    private void StartBattle()
    {
        if (battleAnimationPanel != null)
            battleAnimationPanel.SetActive(true);

        // Симулируем битву
        BattleManager.BattleResult result = BattleManager.Instance.AttackPlayer(currentEnemyRats, currentEnemyCheese);

        // Показываем результат через 2 секунды
        Invoke(nameof(ShowBattleResult), 2f);
        lastBattleResult = result;
    }

    private BattleManager.BattleResult lastBattleResult;

    private void ShowBattleResult()
    {
        if (battleAnimationPanel != null)
            battleAnimationPanel.SetActive(false);

        if (battleResultPanel != null)
            battleResultPanel.SetActive(true);

        if (battleResultText != null)
        {
            string resultText = "";

            if (lastBattleResult.victory)
            {
                resultText = "ПОБЕДА!\n\n";
                resultText += $"Украдено сыра: {lastBattleResult.cheeseStolen}\n";
                resultText += $"Получено душ: {lastBattleResult.soulsGained}\n";
                resultText += $"Получено эликсиров: {lastBattleResult.elixirsGained}\n";

                if (lastBattleResult.overfedEnemies.Count > 0)
                {
                    resultText += $"\nЗакормлено вражеских крыс: {lastBattleResult.overfedEnemies.Count}";
                }
            }
            else
            {
                resultText = "ПОРАЖЕНИЕ!\n\n";
                resultText += $"Прибито ваших крыс: {lastBattleResult.beatenRats.Count}\n";
                resultText += "Вылечите их в списке крыс!";
            }

            battleResultText.text = resultText;
        }

        UpdatePlayerRatsInfo();
    }

    public void CloseResultPanel()
    {
        if (battleResultPanel != null)
            battleResultPanel.SetActive(false);

        GenerateNewEnemy();
    }

    public void FindNewEnemy()
    {
        GenerateNewEnemy();
    }
}
