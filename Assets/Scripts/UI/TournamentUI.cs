using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startTournamentButton;
    [SerializeField] private TextMeshProUGUI tournamentInfoText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button closeResultButton;

    private void Start()
    {
        if (startTournamentButton != null)
            startTournamentButton.onClick.AddListener(StartTournament);

        if (closeResultButton != null)
            closeResultButton.onClick.AddListener(CloseResult);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateTournamentInfo();
    }

    private void OnEnable()
    {
        UpdateTournamentInfo();
    }

    private void UpdateTournamentInfo()
    {
        int playerLevel = CurrencyManager.Instance.GetPlayerLevel();

        if (playerLevelText != null)
            playerLevelText.text = $"Ваш уровень: {playerLevel}";

        if (tournamentInfoText != null)
        {
            string league = GetLeagueName(playerLevel);
            tournamentInfoText.text = $"Текущая лига: {league}\n\n" +
                                     "В турнире участвуют 10 игроков.\n" +
                                     "Ваши 5 случайных крыс будут сражаться.\n\n" +
                                     "Награды зависят от занятого места:\n" +
                                     "1 место: 500 сыра, 50 душ, 10 эликсиров\n" +
                                     "2-3 место: 300 сыра, 30 душ, 5 эликсиров\n" +
                                     "4-5 место: 150 сыра, 15 душ, 2 эликсира\n" +
                                     "6-10 место: 50 сыра, 5 душ, 1 эликсир";
        }
    }

    private string GetLeagueName(int level)
    {
        if (level >= 151) return "Алмазная лига";
        if (level >= 51) return "Золотая лига";
        if (level >= 15) return "Серебряная лига";
        return "Бронзовая лига";
    }

    private void StartTournament()
    {
        if (RatManager.Instance.GetHealthyRatCount() < 1)
        {
            Debug.Log("У вас нет здоровых крыс для турнира!");
            return;
        }

        // Запускаем турнир
        BattleManager.TournamentResult result = BattleManager.Instance.PlayTournament();

        ShowResult(result);
    }

    private void ShowResult(BattleManager.TournamentResult result)
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            string placeText = result.place == 1 ? "1-е место!" :
                              result.place == 2 ? "2-е место!" :
                              result.place == 3 ? "3-е место!" :
                              $"{result.place}-е место";

            resultText.text = $"Турнир завершен!\n\n" +
                             $"Ваше место: {placeText}\n\n" +
                             $"Награды:\n" +
                             $"Сыр: {result.cheeseReward}\n" +
                             $"Души: {result.soulsReward}\n" +
                             $"Эликсиры: {result.elixirsReward}";
        }
    }

    private void CloseResult()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
}
