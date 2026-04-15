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
            addCheeseButton.onClick.AddListener(OnForwardButtonClicked);

        if (panel != null)
            panel.SetActive(false);
    }

    public void OpenPanel()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public void OnForwardButtonClicked()
    {
        int randomCheeseAmount = Random.Range(1, 6);
        GameManager.Instance.SetPendingReward(randomCheeseAmount);
        SceneManager.LoadScene("MainWindow");
    }
}