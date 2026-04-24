using UnityEngine;
using System.Collections.Generic;

public enum ItemType
{
    WonderBandage,      // Чудо-бинт (лечение прибитых)
    SimpleDefib,        // Простой ДФР (лечение закормленных)
    SuperDefib,         // СуперДФР (мгновенное лечение закормленных)
    RevivalPotion,      // Зелье оживления (воскрешение мертвых)
    BrownGlove,         // Коричневая перчатка (сила)
    Hat,                // Шляпка (воровство)
    Couch               // Кушетка (ускорение лечения)
}

[System.Serializable]
public class Item
{
    public ItemType type;
    public string name;
    public string description;
    public int ratBucksCost;
    public int cheeseCost;
    public int quantity;

    public Item(ItemType itemType)
    {
        type = itemType;
        quantity = 0;

        switch (type)
        {
            case ItemType.WonderBandage:
                name = "Чудо-бинт";
                description = "Мгновенно лечит прибитую крысу";
                ratBucksCost = 10;
                cheeseCost = 0;
                break;

            case ItemType.SimpleDefib:
                name = "Простой ДФР";
                description = "Лечит закормленную крысу (требуется несколько)";
                ratBucksCost = 5;
                cheeseCost = 0;
                break;

            case ItemType.SuperDefib:
                name = "СуперДФР";
                description = "Мгновенно лечит закормленную крысу";
                ratBucksCost = 20;
                cheeseCost = 0;
                break;

            case ItemType.RevivalPotion:
                name = "Зелье оживления";
                description = "Воскрешает мертвую крысу";
                ratBucksCost = 50;
                cheeseCost = 0;
                break;

            case ItemType.BrownGlove:
                name = "Коричневая перчатка";
                description = "+10 к атаке";
                ratBucksCost = 100;
                cheeseCost = 0;
                break;

            case ItemType.Hat:
                name = "Шляпка";
                description = "+15 к воровству";
                ratBucksCost = 150;
                cheeseCost = 0;
                break;

            case ItemType.Couch:
                name = "Кушетка";
                description = "Ускоряет лечение крыс на 50%";
                ratBucksCost = 0;
                cheeseCost = 5000;
                break;
        }
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private Dictionary<ItemType, Item> inventory = new Dictionary<ItemType, Item>();

    // Экипировка
    private Dictionary<string, ItemType?> ratEquipment = new Dictionary<string, ItemType?>(); // ratId -> ItemType

    // Постройки
    private int couchLevel = 0;

    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeInventory();
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeInventory()
    {
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            inventory[type] = new Item(type);
        }
    }

    public bool BuyItem(ItemType type, int quantity = 1)
    {
        Item item = inventory[type];

        int totalRatBucks = item.ratBucksCost * quantity;
        int totalCheese = item.cheeseCost * quantity;

        if (totalRatBucks > 0 && !CurrencyManager.Instance.SpendRatBucks(totalRatBucks))
            return false;

        if (totalCheese > 0 && !CurrencyManager.Instance.SpendCheese(totalCheese))
            return false;

        item.quantity += quantity;

        // Кушетка - это постройка
        if (type == ItemType.Couch)
        {
            couchLevel++;
        }

        OnInventoryChanged?.Invoke();
        SaveData();
        return true;
    }

    public bool UseItem(ItemType type, Rat rat = null)
    {
        Item item = inventory[type];

        if (item.quantity <= 0)
            return false;

        bool success = false;

        switch (type)
        {
            case ItemType.WonderBandage:
                if (rat != null && rat.state == RatState.Beaten)
                {
                    rat.Heal();
                    success = true;
                }
                break;

            case ItemType.SimpleDefib:
                if (rat != null && rat.state == RatState.Overfed)
                {
                    rat.defibrillatorCount = Mathf.Max(0, rat.defibrillatorCount - 1);
                    if (rat.defibrillatorCount <= 0)
                    {
                        rat.Heal();
                    }
                    success = true;
                }
                break;

            case ItemType.SuperDefib:
                if (rat != null && rat.state == RatState.Overfed)
                {
                    rat.Heal();
                    success = true;
                }
                break;

            case ItemType.RevivalPotion:
                if (rat != null && rat.state == RatState.Dead)
                {
                    rat.Heal();
                    success = true;
                }
                break;
        }

        if (success)
        {
            item.quantity--;
            OnInventoryChanged?.Invoke();
            SaveData();
        }

        return success;
    }

    public bool EquipItem(string ratId, ItemType type)
    {
        Item item = inventory[type];

        // Только экипировка
        if (type != ItemType.BrownGlove && type != ItemType.Hat)
            return false;

        if (item.quantity <= 0)
            return false;

        // Снимаем старую экипировку
        if (ratEquipment.ContainsKey(ratId) && ratEquipment[ratId].HasValue)
        {
            ItemType oldType = ratEquipment[ratId].Value;
            inventory[oldType].quantity++;
        }

        // Надеваем новую
        ratEquipment[ratId] = type;
        item.quantity--;

        OnInventoryChanged?.Invoke();
        SaveData();
        return true;
    }

    public void UnequipItem(string ratId)
    {
        if (!ratEquipment.ContainsKey(ratId) || !ratEquipment[ratId].HasValue)
            return;

        ItemType type = ratEquipment[ratId].Value;
        inventory[type].quantity++;
        ratEquipment[ratId] = null;

        OnInventoryChanged?.Invoke();
        SaveData();
    }

    public ItemType? GetEquippedItem(string ratId)
    {
        if (ratEquipment.ContainsKey(ratId))
            return ratEquipment[ratId];
        return null;
    }

    public int GetItemQuantity(ItemType type)
    {
        return inventory[type].quantity;
    }

    public Item GetItem(ItemType type)
    {
        return inventory[type];
    }

    public int GetCouchLevel()
    {
        return couchLevel;
    }

    public float GetHealSpeedMultiplier()
    {
        return 1f + (couchLevel * 0.5f); // +50% за каждый уровень кушетки
    }

    public Dictionary<ItemType, Item> GetAllItems()
    {
        return new Dictionary<ItemType, Item>(inventory);
    }

    private void SaveData()
    {
        foreach (var kvp in inventory)
        {
            PlayerPrefs.SetInt($"Item_{kvp.Key}", kvp.Value.quantity);
        }
        PlayerPrefs.SetInt("CouchLevel", couchLevel);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        foreach (var kvp in inventory)
        {
            kvp.Value.quantity = PlayerPrefs.GetInt($"Item_{kvp.Key}", 0);
        }
        couchLevel = PlayerPrefs.GetInt("CouchLevel", 0);
    }
}
