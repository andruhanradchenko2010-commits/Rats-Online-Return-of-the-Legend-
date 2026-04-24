using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ArenaUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardItemPrefab;
    [SerializeField] private GameObject arenaGamePanel;
    [SerializeField] private GameObject arenaMenuPanel;

    [Header("Arena Rendering")]
    [SerializeField] private RectTransform arenaField;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject foodPrefab;

    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    private Dictionary<ArenaManager.FoodItem, GameObject> foodObjects = new Dictionary<ArenaManager.FoodItem, GameObject>();

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartArena);

        if (arenaGamePanel != null)
            arenaGamePanel.SetActive(false);

        if (arenaMenuPanel != null)
            arenaMenuPanel.SetActive(true);
    }

    private void OnEnable()
    {
        if (arenaGamePanel != null)
            arenaGamePanel.SetActive(false);

        if (arenaMenuPanel != null)
            arenaMenuPanel.SetActive(true);
    }

    private void Update()
    {
        if (ArenaManager.Instance == null || !ArenaManager.Instance.IsRoundActive())
            return;

        // Обновляем таймер
        UpdateTimer();

        // Обновляем позиции игроков
        UpdatePlayerPositions();

        // Обновляем еду
        UpdateFood();

        // Обновляем таблицу лидеров
        UpdateLeaderboard();

        // Управление локальным игроком
        HandlePlayerInput();
    }

    private void StartArena()
    {
        if (arenaMenuPanel != null)
            arenaMenuPanel.SetActive(false);

        if (arenaGamePanel != null)
            arenaGamePanel.SetActive(true);

        ArenaManager.Instance.StartRound();

        // Очищаем старые объекты
        ClearArena();
    }

    private void ClearArena()
    {
        foreach (var obj in playerObjects.Values)
        {
            if (obj != null) Destroy(obj);
        }
        playerObjects.Clear();

        foreach (var obj in foodObjects.Values)
        {
            if (obj != null) Destroy(obj);
        }
        foodObjects.Clear();
    }

    private void UpdateTimer()
    {
        if (timerText != null)
        {
            float time = ArenaManager.Instance.GetRemainingTime();
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    private void UpdatePlayerPositions()
    {
        List<ArenaManager.ArenaPlayer> players = ArenaManager.Instance.GetPlayers();

        foreach (var player in players)
        {
            if (!playerObjects.ContainsKey(player.id))
            {
                // Создаем объект игрока
                GameObject playerObj = Instantiate(playerPrefab, arenaField);
                playerObjects[player.id] = playerObj;

                // Настраиваем цвет
                Image img = playerObj.GetComponent<Image>();
                if (img != null)
                    img.color = player.color;

                // Добавляем имя
                TextMeshProUGUI nameText = playerObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = player.name;
            }

            // Обновляем позицию
            GameObject obj = playerObjects[player.id];
            if (obj != null)
            {
                RectTransform rt = obj.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = player.position;
            }
        }
    }

    private void UpdateFood()
    {
        List<ArenaManager.FoodItem> foodItems = ArenaManager.Instance.GetFoodItems();

        // Удаляем собранную еду
        List<ArenaManager.FoodItem> toRemove = new List<ArenaManager.FoodItem>();
        foreach (var kvp in foodObjects)
        {
            if (kvp.Key.collected)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var item in toRemove)
        {
            foodObjects.Remove(item);
        }

        // Создаем новую еду
        foreach (var food in foodItems)
        {
            if (food.collected) continue;

            if (!foodObjects.ContainsKey(food))
            {
                GameObject foodObj = Instantiate(foodPrefab, arenaField);
                foodObjects[food] = foodObj;

                RectTransform rt = foodObj.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = food.position;

                // Устанавливаем цвет еды (желтый)
                Image img = foodObj.GetComponent<Image>();
                if (img != null)
                    img.color = Color.yellow;
            }
        }
    }

    private void UpdateLeaderboard()
    {
        // Очищаем таблицу
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Заполняем таблицу
        List<ArenaManager.ArenaPlayer> players = ArenaManager.Instance.GetPlayers();
        players.Sort((a, b) => b.score.CompareTo(a.score));

        for (int i = 0; i < players.Count; i++)
        {
            GameObject item = Instantiate(leaderboardItemPrefab, leaderboardContainer);
            TextMeshProUGUI text = item.GetComponent<TextMeshProUGUI>();

            if (text != null)
            {
                text.text = $"{i + 1}. {players[i].name}: {players[i].score}";
                text.color = players[i].color;
            }
        }
    }

    private void HandlePlayerInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            input.y = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            input.y = -1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            input.x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            input.x = 1;

        if (input != Vector2.zero)
        {
            ArenaManager.Instance.MoveLocalPlayer(input.normalized);
        }
    }

    public void ExitArena()
    {
        if (arenaGamePanel != null)
            arenaGamePanel.SetActive(false);

        if (arenaMenuPanel != null)
            arenaMenuPanel.SetActive(true);

        ClearArena();
    }
}
