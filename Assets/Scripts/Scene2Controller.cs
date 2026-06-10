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
            addCheeseButton.onClick.AddListener(OnAttackButtonClicked);

        if (panel != null)
            panel.SetActive(false);
    }

    public void OpenPanel()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public void OnAttackButtonClicked()
    {
        // Получаем первую крысу игрока
        var rats = RatManager.Instance.GetAllRats();
        if (rats.Count == 0)
        {
            Debug.LogError("Нет крыс!");
            SceneManager.LoadScene("MainWindow");
            return;
        }

        Rat playerRat = rats[0];

        // Проверяем может ли крыса атаковать
        if (!playerRat.CanFight())
        {
            GameLog.Log("Крыса не может атаковать (подбита или мертва)");
            GameManager.Instance.SetPendingReward(0);
            SceneManager.LoadScene("MainWindow");
            return;
        }

        // Единый бросок воровства через BattleManager (без начисления — сыр выдаст бочка на MainWindow)
        float stealChance = BattleManager.Instance.CalculateStealChance(playerRat);
        BattleManager.StealResult steal = BattleManager.Instance.RollSteal(playerRat);

        if (steal.success)
        {
            // Успех - награду заберет бочка
            GameManager.Instance.SetPendingReward(steal.cheese);
            playerRat.SetHungry();
            GameLog.Log($"✅ Успех! Украдено {steal.cheese} сыра. Шанс был {stealChance:F1}%");
            GameLog.Log($"📦 Установлена награда: {steal.cheese} сыра. Бочка должна появиться на MainWindow!");
        }
        else
        {
            // Провал - крыса подбита
            playerRat.Beat();
            GameManager.Instance.SetPendingReward(0);
            GameLog.Log($"❌ Провал! Крыса подбита на {RatManager.Instance.GetCurrentHealTime():F0} секунд. Шанс был {stealChance:F1}%");
            GameLog.Log($"📦 Награда = 0. Бочка НЕ должна появиться.");
        }

        SceneManager.LoadScene("MainWindow");
    }
}