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

        // TODO: Загрузить спрайт крысы из ресурсов
        LoadRatSprite();
    }

    private void LoadRatSprite()
    {
        if (ratIcon == null) return;

        // Пытаемся загрузить спрайт из папки Sprite
        string spriteName = rat.type switch
        {
            RatType.Gray => "Серая крыса",
            RatType.Royal => "Царская крыса",
            RatType.Angel => "Ангельская крыса",
            RatType.Devil => "Дьявольская крыса",
            RatType.Vampire => "Вампир",
            RatType.Joker => "Джокер",
            RatType.BatRat => "Bat Rat",
            _ => "Серая крыса"
        };

        Sprite sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");

        if (sprite != null)
        {
            ratIcon.sprite = sprite;
        }
        else
        {
            // Если спрайт не найден, создаем красный квадрат
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.red;
            tex.SetPixels(pixels);
            tex.Apply();
            ratIcon.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }
    }
}
