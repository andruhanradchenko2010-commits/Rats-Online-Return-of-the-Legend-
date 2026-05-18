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
            Debug.Log("Крыса не может атаковать (подбита или мертва)");
            GameManager.Instance.SetPendingReward(0);
            SceneManager.LoadScene("MainWindow");
            return;
        }

        // Рассчитываем шанс воровства через BattleManager
        float stealChance = BattleManager.Instance.CalculateStealChance(playerRat);
        float roll = Random.Range(0f, 100f);

        if (roll < stealChance)
        {
            // Успех - крадем сыр
            int stolenCheese = Random.Range(BattleManager.Instance.minStolenCheese, BattleManager.Instance.maxStolenCheese);
            GameManager.Instance.SetPendingReward(stolenCheese);
            playerRat.SetHungry();
            Debug.Log($"✅ Успех! Украдено {stolenCheese} сыра. Шанс был {stealChance:F1}%");
            Debug.Log($"📦 Установлена награда: {stolenCheese} сыра. Бочка должна появиться на MainWindow!");
        }
        else
        {
            // Провал - крыса подбита
            playerRat.Beat();
            GameManager.Instance.SetPendingReward(0);
            Debug.Log($"❌ Провал! Крыса подбита на {(RatManager.Instance.fastHealMode ? RatManager.Instance.fastHealTimeSeconds : RatManager.Instance.healTimeSeconds)} секунд. Шанс был {stealChance:F1}%");
            Debug.Log($"📦 Награда = 0. Бочка НЕ должна появиться.");
        }

        SceneManager.LoadScene("MainWindow");
    }
}