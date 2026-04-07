using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene1Controller : MonoBehaviour
{
    [SerializeField] private Button goToScene2Button;
    [SerializeField] private Text textCountCheese;

    private void Start()
    {
        if (goToScene2Button != null)
            goToScene2Button.onClick.AddListener(GoToScene2);

        UpdateCheeseText(GameManager.Instance.GetCheeseCount());
        GameManager.Instance.OnCheeseChanged += UpdateCheeseText;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnCheeseChanged -= UpdateCheeseText;
    }

    private void UpdateCheeseText(int count)
    {
        if (textCountCheese != null)
            textCountCheese.text = count.ToString();
    }

    public void GoToScene2()
    {
        SceneManager.LoadScene("StealGame");
    }
}