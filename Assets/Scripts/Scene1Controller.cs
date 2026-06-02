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

        if (CurrencyManager.Instance != null)
        {
            UpdateCheeseText(CurrencyManager.Instance.GetCheese());
            CurrencyManager.Instance.OnCheeseChanged += UpdateCheeseText;
        }

        // Подписываемся на события изменения крыс для обновления кнопки
        if (RatManager.Instance != null)
        {
            RatManager.Instance.OnRatAdded += OnRatChanged;
            RatManager.Instance.OnRatRemoved += OnRatChanged;
            RatManager.Instance.OnRatUpdated += OnRatChanged;
        }

        // Проверяем состояние кнопки при старте
        UpdateAttackButtonState();

        // Вызываем бочку с задержкой, чтобы BarrelRewardController успел подписаться
        StartCoroutine(TriggerBarrelAfterDelay());
    }

    private System.Collections.IEnumerator TriggerBarrelAfterDelay()
    {
        yield return null; // Ждем один кадр, чтобы все Start() методы выполнились

        if (GameManager.Instance != null && GameManager.Instance.ShouldShowBarrel())
        {
            GameManager.Instance.TriggerBarrelReward();
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCheeseChanged -= UpdateCheeseText;

        if (RatManager.Instance != null)
        {
            RatManager.Instance.OnRatAdded -= OnRatChanged;
            RatManager.Instance.OnRatRemoved -= OnRatChanged;
            RatManager.Instance.OnRatUpdated -= OnRatChanged;
        }
    }

    private void OnRatChanged(Rat rat)
    {
        UpdateAttackButtonState();
    }

    private void UpdateAttackButtonState()
    {
        if (goToScene2Button == null) return;
        if (RatManager.Instance == null) return;

        var rats = RatManager.Instance.GetAllRats();

        if (rats.Count == 0)
        {
            // Нет крыс - кнопка неактивна
            goToScene2Button.interactable = false;
            return;
        }

        Rat firstRat = rats[0];

        // Кнопка активна только если крыса может атаковать
        bool canAttack = firstRat.CanFight();
        goToScene2Button.interactable = canAttack;

        // Debug информация
        if (!canAttack)
        {
            string reason = firstRat.state switch
            {
                RatState.Beaten => "восстанавливается",
                RatState.Overfed => "закормлена",
                RatState.Dead => "мертва",
                _ => "не может атаковать"
            };
            // Можно добавить текст на кнопке или рядом с ней
        }
    }

    private void UpdateCheeseText(int count)
    {
        if (textCountCheese != null)
        {
            textCountCheese.text = count.ToString();
            Debug.Log($"Scene1Controller.UpdateCheeseText: Обновлен текст сыра на {count}");
        }
        else
        {
            Debug.LogError("Scene1Controller.UpdateCheeseText: textCountCheese = NULL!");
        }
    }

    public void GoToScene2()
    {
        SceneManager.LoadScene("StealGame");
    }
}