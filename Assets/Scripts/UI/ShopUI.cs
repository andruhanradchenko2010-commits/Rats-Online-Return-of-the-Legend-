using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform shopItemsContainer;
    [SerializeField] private GameObject shopItemPrefab;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshShop;
        }

        RefreshShop();
    }

    private void OnEnable()
    {
        RefreshShop();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshShop;
        }
    }

    private void RefreshShop()
    {
        // Очищаем список
        foreach (Transform child in shopItemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Отображаем все предметы
        Dictionary<ItemType, Item> items = InventoryManager.Instance.GetAllItems();

        foreach (var kvp in items)
        {
            Item item = kvp.Value;
            GameObject itemObj = Instantiate(shopItemPrefab, shopItemsContainer);

            // Название
            TextMeshProUGUI nameText = itemObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = item.name;

            // Описание
            TextMeshProUGUI descText = itemObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
                descText.text = item.description;

            // Цена
            TextMeshProUGUI priceText = itemObj.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
            if (priceText != null)
            {
                if (item.ratBucksCost > 0)
                    priceText.text = $"{item.ratBucksCost} крысобаксов";
                else if (item.cheeseCost > 0)
                    priceText.text = $"{item.cheeseCost} сыра";
                else
                    priceText.text = "Бесплатно";
            }

            // Количество в инвентаре
            TextMeshProUGUI quantityText = itemObj.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
            if (quantityText != null)
                quantityText.text = $"В наличии: {item.quantity}";

            // Кнопка покупки
            Button buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
            if (buyButton != null)
            {
                ItemType itemType = kvp.Key;
                buyButton.onClick.AddListener(() => BuyItem(itemType));

                TextMeshProUGUI btnText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = "Купить";
            }
        }
    }

    private void BuyItem(ItemType type)
    {
        if (InventoryManager.Instance.BuyItem(type))
        {
            Debug.Log($"Куплен предмет: {type}");
        }
        else
        {
            Debug.Log("Недостаточно средств!");
        }
    }
}
