using UnityEngine;
using UnityEngine.UI;

public class Cheese : MonoBehaviour
{
    public static Cheese Instance;

    private int cheeseCount;
    private Text cheeseText;

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
        }
    }

    public void AddCheese(int amount)
    {
        cheeseCount += amount;
        UpdateUI();
    }

    public void SetText(Text newText)
    {
        cheeseText = newText;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (cheeseText != null)
        {
            cheeseText.text = cheeseCount.ToString();
        }
    }
}