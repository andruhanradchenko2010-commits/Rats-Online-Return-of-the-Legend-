using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleRatDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI ratInfoText;
    public Button stealButton;

    private void Start()
    {
        if (stealButton != null)
        {
            stealButton.onClick.AddListener(OnStealClicked);
        }

        UpdateDisplay();
    }

    private void Update()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (ratInfoText == null) return;
        if (RatManager.Instance == null) return;

        var rats = RatManager.Instance.GetAllRats();

        if (rats.Count == 0)
        {
            ratInfoText.text = "НЕТ КРЫС!\nПроверь RatManager в сцене";
            return;
        }

        Rat rat = rats[0];
        float stealChance = CalculateStealChance(rat);
        string state = GetStateText(rat);

        ratInfoText.text = $"КРЫСА:\n" +
                          $"Уровень: {rat.level}\n" +
                          $"Состояние: {state}\n" +
                          $"Шанс воровства: {stealChance:F1}%\n" +
                          $"Голодная: {(rat.isHungry ? "Да" : "Нет")}";

        // Обновляем кнопку
        if (stealButton != null)
        {
            stealButton.interactable = rat.CanFight();
        }
    }

    private string GetStateText(Rat rat)
    {
        if (rat.state == RatState.Beaten)
        {
            float timeLeft = 30f - (Time.time - rat.healStartTime);
            if (timeLeft > 0)
                return $"Подбита ({Mathf.CeilToInt(timeLeft)}с)";
            else
                return "Восстанавливается...";
        }
        return rat.state == RatState.Healthy ? "Здорова" : rat.state.ToString();
    }

    private float CalculateStealChance(Rat rat)
    {
        float minChance = 0f;
        float maxChance = 95f;
        int maxLevel = 55;
        float chance = minChance + ((rat.level - 1) / (float)(maxLevel - 1)) * (maxChance - minChance);
        return Mathf.Clamp(chance, minChance, maxChance);
    }

    private void OnStealClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StealGame");
    }
}
