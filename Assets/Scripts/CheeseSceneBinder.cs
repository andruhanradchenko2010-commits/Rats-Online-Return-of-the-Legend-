using UnityEngine;
using UnityEngine.UI;

public class CheeseSceneBinder : MonoBehaviour
{
    [SerializeField] private Text cheeseText;

    private void Start()
    {
        Cheese.Instance.SetText(cheeseText);
    }
}