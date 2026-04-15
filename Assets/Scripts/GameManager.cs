using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int cheeseCount = 0;
    private int pendingCheeseReward = 0;
    private bool shouldShowBarrel = false;

    public event Action<int> OnCheeseChanged;
    public event Action<int> OnBarrelRewardReady;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddCheese(int amount)
    {
        cheeseCount += amount;
        OnCheeseChanged?.Invoke(cheeseCount);
    }

    public int GetCheeseCount()
    {
        return cheeseCount;
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