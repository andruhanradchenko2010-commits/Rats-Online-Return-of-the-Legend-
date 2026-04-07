using UnityEngine;

public class OpenWindow : MonoBehaviour
{
    [SerializeField] private GameObject panelInfoCheese;

    public void OpenWindowCheese()
    {
        panelInfoCheese.SetActive(!panelInfoCheese.activeSelf);
    }
}