using UnityEngine;
using System.Collections.Generic;

public class ArenaManager : MonoBehaviour
{
    public static ArenaManager Instance;

    [System.Serializable]
    public class ArenaPlayer
    {
        public string id;
        public string name;
        public Vector2 position;
        public int score;
        public Color color;
        public bool isLocalPlayer;
    }

    [System.Serializable]
    public class FoodItem
    {
        public Vector2 position;
        public int value;
        public bool collected;
    }

    private List<ArenaPlayer> players = new List<ArenaPlayer>();
    private List<FoodItem> foodItems = new List<FoodItem>();
    private float roundTime = 180f; // 3 минуты
    private float currentTime;
    private bool roundActive = false;

    [Header("Arena Settings")]
    public Vector2 arenaSize = new Vector2(800f, 600f);
    public int maxPlayers = 8;
    public float foodSpawnInterval = 2f;
    public int maxFoodItems = 20;

    private float lastFoodSpawnTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartRound()
    {
        roundActive = true;
        currentTime = roundTime;
        players.Clear();
        foodItems.Clear();

        // Создаем локального игрока
        ArenaPlayer localPlayer = new ArenaPlayer
        {
            id = "local",
            name = "Вы",
            position = Vector2.zero,
            score = 0,
            color = Color.green,
            isLocalPlayer = true
        };
        players.Add(localPlayer);

        // Генерируем AI игроков
        GenerateAIPlayers();

        // Спавним начальную еду
        for (int i = 0; i < 10; i++)
        {
            SpawnFood();
        }

        lastFoodSpawnTime = Time.time;
    }

    private void GenerateAIPlayers()
    {
        int aiCount = Random.Range(3, maxPlayers);
        Color[] colors = { Color.red, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.white, new Color(1f, 0.5f, 0f) };

        for (int i = 0; i < aiCount; i++)
        {
            ArenaPlayer ai = new ArenaPlayer
            {
                id = $"ai_{i}",
                name = $"Игрок {i + 1}",
                position = new Vector2(Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                                      Random.Range(-arenaSize.y / 2, arenaSize.y / 2)),
                score = 0,
                color = colors[i % colors.Length],
                isLocalPlayer = false
            };
            players.Add(ai);
        }
    }

    private void Update()
    {
        if (!roundActive) return;

        // Обновляем таймер
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            EndRound();
            return;
        }

        // Спавним еду
        if (Time.time - lastFoodSpawnTime >= foodSpawnInterval && foodItems.Count < maxFoodItems)
        {
            SpawnFood();
            lastFoodSpawnTime = Time.time;
        }

        // Обновляем AI игроков
        UpdateAIPlayers();
    }

    private void SpawnFood()
    {
        FoodItem food = new FoodItem
        {
            position = new Vector2(Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
                                  Random.Range(-arenaSize.y / 2, arenaSize.y / 2)),
            value = Random.Range(1, 5),
            collected = false
        };
        foodItems.Add(food);
    }

    private void UpdateAIPlayers()
    {
        foreach (var player in players)
        {
            if (player.isLocalPlayer) continue;

            // AI ищет ближайшую еду
            FoodItem nearestFood = FindNearestFood(player.position);
            if (nearestFood != null)
            {
                Vector2 direction = (nearestFood.position - player.position).normalized;
                player.position += direction * 100f * Time.deltaTime;

                // Проверяем сбор еды
                if (Vector2.Distance(player.position, nearestFood.position) < 20f)
                {
                    CollectFood(player, nearestFood);
                }
            }
        }
    }

    private FoodItem FindNearestFood(Vector2 position)
    {
        FoodItem nearest = null;
        float minDistance = float.MaxValue;

        foreach (var food in foodItems)
        {
            if (food.collected) continue;

            float distance = Vector2.Distance(position, food.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = food;
            }
        }

        return nearest;
    }

    public void MoveLocalPlayer(Vector2 direction)
    {
        if (!roundActive) return;

        ArenaPlayer localPlayer = players.Find(p => p.isLocalPlayer);
        if (localPlayer != null)
        {
            localPlayer.position += direction * 150f * Time.deltaTime;

            // Ограничиваем движение границами арены
            localPlayer.position.x = Mathf.Clamp(localPlayer.position.x, -arenaSize.x / 2, arenaSize.x / 2);
            localPlayer.position.y = Mathf.Clamp(localPlayer.position.y, -arenaSize.y / 2, arenaSize.y / 2);

            // Проверяем сбор еды
            CheckFoodCollection(localPlayer);
        }
    }

    private void CheckFoodCollection(ArenaPlayer player)
    {
        foreach (var food in foodItems)
        {
            if (food.collected) continue;

            if (Vector2.Distance(player.position, food.position) < 20f)
            {
                CollectFood(player, food);
            }
        }
    }

    private void CollectFood(ArenaPlayer player, FoodItem food)
    {
        food.collected = true;
        player.score += food.value;
    }

    private void EndRound()
    {
        roundActive = false;

        // Сортируем игроков по очкам
        players.Sort((a, b) => b.score.CompareTo(a.score));

        // Выдаем награды локальному игроку
        ArenaPlayer localPlayer = players.Find(p => p.isLocalPlayer);
        if (localPlayer != null)
        {
            int place = players.IndexOf(localPlayer) + 1;
            int cheeseReward = Mathf.Max(50, 200 - (place * 20));
            CurrencyManager.Instance.AddCheese(cheeseReward);
        }
    }

    public List<ArenaPlayer> GetPlayers() => new List<ArenaPlayer>(players);
    public List<FoodItem> GetFoodItems() => new List<FoodItem>(foodItems);
    public float GetRemainingTime() => currentTime;
    public bool IsRoundActive() => roundActive;
    public Vector2 GetArenaSize() => arenaSize;
}
