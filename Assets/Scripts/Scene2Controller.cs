using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene2Controller : MonoBehaviour
{
    [SerializeField] private Button openPanelButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button addCheeseButton;

    private void Start()
    {
        if (openPanelButton != null)
            openPanelButton.onClick.AddListener(OpenPanel);

        if (addCheeseButton != null)
            addCheeseButton.onClick.AddListener(AddCheeseAndGoBack);

        if (panel != null)
            panel.SetActive(false);
    }

    public void OpenPanel()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public void AddCheeseAndGoBack()
    {
        GameManager.Instance.AddCheese(1);
        SceneManager.LoadScene("MainWindow");
    }
}