using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int cheeseCount = 0;
    public event Action<int> OnCheeseChanged;

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
}