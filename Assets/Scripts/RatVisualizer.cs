using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RatVisualizer : RatAwareUI, IPointerClickHandler
{
    [Header("Rat Sprites")]
    public Sprite healthyRatSprite;  // Перетащи Gray rat.png
    public Sprite woundedRatSprite;  // Перетащи Wounded rat.png

    [Header("UI")]
    public Image ratImage;  // Image компонент для отображения крысы

    protected override void OnStart()
    {
        // Если не указан Image, берем с этого же GameObject
        if (ratImage == null)
        {
            ratImage = GetComponent<Image>();
        }

        // Автозагрузка спрайтов из Resources, если не назначены
        if (healthyRatSprite == null)
        {
            healthyRatSprite = Resources.Load<Sprite>("Sprites/Gray rat");
            if (healthyRatSprite == null)
            {
                healthyRatSprite = SpriteHelper.LoadRatSprite(RatType.Gray);
            }
            Debug.Log($"RatVisualizer: Загружен спрайт здоровой крысы: {healthyRatSprite != null}");
        }

        if (woundedRatSprite == null)
        {
            woundedRatSprite = Resources.Load<Sprite>("Sprites/Wounded rat");
            if (woundedRatSprite == null)
            {
                // Пытаемся найти в Assets/Sprite/Types of rats/
                woundedRatSprite = Resources.Load<Sprite>("Types of rats/Wounded rat");
            }
            Debug.Log($"RatVisualizer: Загружен спрайт раненой крысы: {woundedRatSprite != null}");
        }

        UpdateRatVisual();
    }

    protected override void OnRatChanged(Rat rat)
    {
        UpdateRatVisual();
    }

    private void UpdateRatVisual()
    {
        if (ratImage == null)
        {
            Debug.LogWarning("RatVisualizer: ratImage == null!");
            return;
        }

        if (RatManager.Instance == null)
        {
            Debug.LogWarning("RatVisualizer: RatManager.Instance == null!");
            return;
        }

        var rats = RatManager.Instance.GetAllRats();

        if (rats.Count == 0)
        {
            ratImage.enabled = false;
            Debug.Log("RatVisualizer: Нет крыс, Image отключен");
            return;
        }

        ratImage.enabled = true;
        Rat rat = rats[0];

        // Меняем спрайт в зависимости от состояния
        if (rat.state == RatState.Beaten || rat.state == RatState.Dead)
        {
            if (woundedRatSprite != null)
            {
                ratImage.sprite = woundedRatSprite;
                Debug.Log($"RatVisualizer: Установлен спрайт РАНЕНОЙ крысы (состояние: {rat.state})");
            }
            else
            {
                Debug.LogError("RatVisualizer: woundedRatSprite == null! Не могу показать раненую крысу!");
            }
        }
        else
        {
            if (healthyRatSprite != null)
            {
                ratImage.sprite = healthyRatSprite;
                Debug.Log($"RatVisualizer: Установлен спрайт здоровой крысы (состояние: {rat.state})");
            }
            else
            {
                Debug.LogError("RatVisualizer: healthyRatSprite == null! Не могу показать здоровую крысу!");
            }
        }
    }

    // Обработка клика на крысу
    public void OnPointerClick(PointerEventData eventData)
    {
        if (RatManager.Instance == null || CurrencyManager.Instance == null) return;

        var rats = RatManager.Instance.GetAllRats();
        if (rats.Count == 0) return;

        Rat rat = rats[0];

        // Если крыса голодная - кормим
        if (rat.isHungry)
        {
            int cheeseNeeded = rat.level;
            if (CurrencyManager.Instance.GetCheese() >= cheeseNeeded)
            {
                if (RatManager.Instance.FeedRat(rat))
                {
                    Debug.Log($"Крыса накормлена! Потрачено {cheeseNeeded} сыра.");
                }
            }
            else
            {
                Debug.Log($"Недостаточно сыра! Нужно: {cheeseNeeded}");
            }
        }
        // Если крыса не голодная - прокачиваем умение "Воровство"
        else if (rat.state == RatState.Healthy)
        {
            int upgradeCost = rat.type.GetUpgradeCost();
            if (CurrencyManager.Instance.GetCheese() >= upgradeCost)
            {
                if (RatManager.Instance.UpgradeSkill(rat, RatSkill.Theft, 1))
                {
                    Debug.Log($"Умение 'Воровство' прокачано! Потрачено {upgradeCost} сыра. Теперь: {rat.theftSkill}");
                }
            }
            else
            {
                Debug.Log($"Недостаточно сыра для прокачки! Нужно: {upgradeCost}");
            }
        }
    }
}
