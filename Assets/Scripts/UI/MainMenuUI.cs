using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("Currency Display")]
    [SerializeField] private TextMeshProUGUI cheeseText;
    [SerializeField] private TextMeshProUGUI elixirsText;
    [SerializeField] private TextMeshProUGUI soulsText;
    [SerializeField] private TextMeshProUGUI ratBucksText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private Slider expSlider;

    [Header("Panels")]
    [SerializeField] private GameObject ratsPanel;
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private GameObject breedingPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject tournamentPanel;
    [SerializeField] private GameObject arenaPanel;
    [SerializeField] private GameObject dailyChestPanel;

    [Header("Buttons")]
    [SerializeField] private Button ratsButton;
    [SerializeField] private Button battleButton;
    [SerializeField] private Button breedingButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button tournamentButton;
    [SerializeField] private Button arenaButton;
    [SerializeField] private Button dailyChestButton;

    private void Start()
    {
        // Подписываемся на события валют
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCheeseChanged += UpdateCheese;
            CurrencyManager.Instance.OnLoveElixirsChanged += UpdateElixirs;
            CurrencyManager.Instance.OnSoulsChanged += UpdateSouls;
            CurrencyManager.Instance.OnRatBucksChanged += UpdateRatBucks;
            CurrencyManager.Instance.OnPlayerExpChanged += UpdatePlayerExp;

            // Инициализация
            UpdateCheese(CurrencyManager.Instance.GetCheese());
            UpdateElixirs(CurrencyManager.Instance.GetLoveElixirs());
            UpdateSouls(CurrencyManager.Instance.GetSouls());
            UpdateRatBucks(CurrencyManager.Instance.GetRatBucks());
            UpdatePlayerExp(CurrencyManager.Instance.GetPlayerExp(), CurrencyManager.Instance.GetPlayerLevel());
        }

        // Настройка кнопок
        if (ratsButton != null) ratsButton.onClick.AddListener(() => OpenPanel(ratsPanel));
        if (battleButton != null) battleButton.onClick.AddListener(() => OpenPanel(battlePanel));
        if (breedingButton != null) breedingButton.onClick.AddListener(() => OpenPanel(breedingPanel));
        if (shopButton != null) shopButton.onClick.AddListener(() => OpenPanel(shopPanel));
        if (tournamentButton != null) tournamentButton.onClick.AddListener(() => OpenPanel(tournamentPanel));
        if (arenaButton != null) arenaButton.onClick.AddListener(() => OpenPanel(arenaPanel));
        if (dailyChestButton != null) dailyChestButton.onClick.AddListener(() => OpenPanel(dailyChestPanel));

        // Закрываем все панели
        CloseAllPanels();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCheeseChanged -= UpdateCheese;
            CurrencyManager.Instance.OnLoveElixirsChanged -= UpdateElixirs;
            CurrencyManager.Instance.OnSoulsChanged -= UpdateSouls;
            CurrencyManager.Instance.OnRatBucksChanged -= UpdateRatBucks;
            CurrencyManager.Instance.OnPlayerExpChanged -= UpdatePlayerExp;
        }
    }

    private void UpdateCheese(int amount)
    {
        if (cheeseText != null)
            cheeseText.text = amount.ToString();
    }

    private void UpdateElixirs(int amount)
    {
        if (elixirsText != null)
            elixirsText.text = amount.ToString();
    }

    private void UpdateSouls(int amount)
    {
        if (soulsText != null)
            soulsText.text = amount.ToString();
    }

    private void UpdateRatBucks(int amount)
    {
        if (ratBucksText != null)
            ratBucksText.text = amount.ToString();
    }

    private void UpdatePlayerExp(int exp, int level)
    {
        if (playerLevelText != null)
            playerLevelText.text = $"Уровень {level}";

        if (expSlider != null && CurrencyManager.Instance != null)
        {
            int expForNext = CurrencyManager.Instance.GetExpForNext();
            expSlider.maxValue = expForNext;
            expSlider.value = exp;
        }
    }

    private void OpenPanel(GameObject panel)
    {
        CloseAllPanels();
        if (panel != null)
            panel.SetActive(true);
    }

    private void CloseAllPanels()
    {
        if (ratsPanel != null) ratsPanel.SetActive(false);
        if (battlePanel != null) battlePanel.SetActive(false);
        if (breedingPanel != null) breedingPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (tournamentPanel != null) tournamentPanel.SetActive(false);
        if (arenaPanel != null) arenaPanel.SetActive(false);
        if (dailyChestPanel != null) dailyChestPanel.SetActive(false);
    }

    public void CloseCurrentPanel()
    {
        CloseAllPanels();
    }
}
