using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RatListItem : MonoBehaviour
{
    [SerializeField] private Image ratIcon;
    [SerializeField] private TextMeshProUGUI ratNameText;
    [SerializeField] private TextMeshProUGUI ratLevelText;
    [SerializeField] private TextMeshProUGUI ratStateText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image stateIndicator;

    private Rat rat;
    private System.Action onSelect;

    public void Setup(Rat ratData, System.Action onSelectCallback)
    {
        rat = ratData;
        onSelect = onSelectCallback;

        if (ratNameText != null)
            ratNameText.text = rat.type.GetDisplayName();

        if (ratLevelText != null)
            ratLevelText.text = $"Ур. {rat.level}";

        if (ratStateText != null)
        {
            string stateText = rat.state switch
            {
                RatState.Healthy => rat.isHungry ? "Голодна" : "Здорова",
                RatState.Beaten => "Прибита",
                RatState.Overfed => "Закормлена",
                RatState.Dead => "Мертва",
                _ => ""
            };
            ratStateText.text = stateText;
        }

        // Цвет индикатора состояния
        if (stateIndicator != null)
        {
            stateIndicator.color = rat.state switch
            {
                RatState.Healthy => rat.isHungry ? Color.yellow : Color.green,
                RatState.Beaten => new Color(1f, 0.5f, 0f), // Оранжевый
                RatState.Overfed => Color.red,
                RatState.Dead => Color.black,
                _ => Color.white
            };
        }

        if (selectButton != null)
            selectButton.onClick.AddListener(() => onSelect?.Invoke());

        // Загружаем спрайт крысы через SpriteHelper
        LoadRatSprite();
    }

    private void LoadRatSprite()
    {
        if (ratIcon == null) return;

        // Используем SpriteHelper для загрузки спрайта
        ratIcon.sprite = SpriteHelper.LoadRatSprite(rat.type);
    }
}
