using UnityEngine;

public class StealingCheese : MonoBehaviour
{
    public void ProcessStealingCheese()
    {
        Cheese.Instance.AddCheese(1);
    }
}