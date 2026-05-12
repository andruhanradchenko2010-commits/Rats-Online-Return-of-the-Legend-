using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RatVisualizer : MonoBehaviour, IPointerClickHandler
{
    [Header("Rat Sprites")]
    public Sprite healthyRatSprite;  // Перетащи Gray rat.png
    public Sprite woundedRatSprite;  // Перетащи Wounded rat.png

    [Header("UI")]
    public Image ratImage;  // Image компонент для отображения крысы

    private void Start()
    {
        // Если не указан Image, берем с этого же GameObject
        if (ratImage == null)
        {
            ratImage = GetComponent<Image>();
        }

        // Подписываемся на события изменения крыс
        if (RatManager.Instance != null)
        {
            RatManager.Instance.OnRatAdded += OnRatChanged;
            RatManager.Instance.OnRatRemoved += OnRatChanged;
            RatManager.Instance.OnRatUpdated += OnRatChanged;
        }

        UpdateRatVisual();
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (RatManager.Instance != null)
        {
            RatManager.Instance.OnRatAdded -= OnRatChanged;
            RatManager.Instance.OnRatRemoved -= OnRatChanged;
            RatManager.Instance.OnRatUpdated -= OnRatChanged;
        }
    }

    private void OnRatChanged(Rat rat)
    {
        UpdateRatVisual();
    }

    private void UpdateRatVisual()
    {
        if (ratImage == null) return;
        if (RatManager.Instance == null) return;

        var rats = RatManager.Instance.GetAllRats();

        if (rats.Count == 0)
        {
            ratImage.enabled = false;
            return;
        }

        ratImage.enabled = true;
        Rat rat = rats[0];

        // Меняем спрайт в зависимости от состояния
        if (rat.state == RatState.Beaten || rat.state == RatState.Dead)
        {
            ratImage.sprite = woundedRatSprite;
        }
        else
        {
            ratImage.sprite = healthyRatSprite;
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
