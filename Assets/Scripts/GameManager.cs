using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int pendingCheeseReward = 0;
    private bool shouldShowBarrel = false;

    public event Action<int> OnBarrelRewardReady;

    [Header("Manager References")]
    [SerializeField] private GameObject currencyManagerPrefab;
    [SerializeField] private GameObject ratManagerPrefab;
    [SerializeField] private GameObject battleManagerPrefab;
    [SerializeField] private GameObject dailyChestManagerPrefab;
    [SerializeField] private GameObject arenaManagerPrefab;
    [SerializeField] private GameObject dungeonManagerPrefab;
    [SerializeField] private GameObject inventoryManagerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeManagers()
    {
        CreateManager<CurrencyManager>("CurrencyManager", currencyManagerPrefab);
        CreateManager<RatManager>("RatManager", ratManagerPrefab);
        CreateManager<BattleManager>("BattleManager", battleManagerPrefab);
        CreateManager<DailyChestManager>("DailyChestManager", dailyChestManagerPrefab);
        CreateManager<ArenaManager>("ArenaManager", arenaManagerPrefab);
        CreateManager<DungeonManager>("DungeonManager", dungeonManagerPrefab);
        CreateManager<InventoryManager>("InventoryManager", inventoryManagerPrefab);
    }

    private void CreateManager<T>(string name, GameObject prefab) where T : MonoBehaviour
    {
        var instance = FindObjectOfType<T>();
        if (instance != null) return;

        if (prefab != null)
        {
            Instantiate(prefab);
        }
        else
        {
            GameObject go = new GameObject(name);
            go.AddComponent<T>();
            DontDestroyOnLoad(go);
        }
    }

    // Старые методы для совместимости
    public void AddCheese(int amount)
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCheese(amount);
    }

    public int GetCheeseCount()
    {
        if (CurrencyManager.Instance != null)
            return CurrencyManager.Instance.GetCheese();
        return 0;
    }

    public void SetPendingReward(int amount)
    {
        pendingCheeseReward = amount;
        shouldShowBarrel = true;
    }

    public bool ShouldShowBarrel()
    {
        return shouldShowBarrel;
    }

    public int GetPendingReward()
    {
        return pendingCheeseReward;
    }

    public void TriggerBarrelReward()
    {
        if (shouldShowBarrel)
        {
            OnBarrelRewardReady?.Invoke(pendingCheeseReward);
            shouldShowBarrel = false;
        }
    }

    public void ClearPendingReward()
    {
        pendingCheeseReward = 0;
        shouldShowBarrel = false;
    }
}