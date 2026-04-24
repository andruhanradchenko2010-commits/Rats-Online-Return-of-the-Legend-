using UnityEngine;
using System.Collections.Generic;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;

    [System.Serializable]
    public class DungeonCell
    {
        public int x;
        public int y;
        public bool hasWall;
        public bool isPath;
        public int cheeseReward;
        public int soulsReward;
    }

    private DungeonCell[,] dungeon;
    private int dungeonWidth = 10;
    private int dungeonHeight = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GenerateDungeon();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void GenerateDungeon()
    {
        dungeon = new DungeonCell[dungeonWidth, dungeonHeight];

        for (int x = 0; x < dungeonWidth; x++)
        {
            for (int y = 0; y < dungeonHeight; y++)
            {
                dungeon[x, y] = new DungeonCell
                {
                    x = x,
                    y = y,
                    hasWall = Random.value > 0.3f, // 70% шанс стены
                    isPath = false,
                    cheeseReward = Random.Range(10, 50),
                    soulsReward = Random.Range(1, 5)
                };
            }
        }

        // Стартовая клетка без стены
        dungeon[0, 0].hasWall = false;
        dungeon[0, 0].isPath = true;
    }

    public bool BreakWall(int x, int y)
    {
        if (x < 0 || x >= dungeonWidth || y < 0 || y >= dungeonHeight)
            return false;

        DungeonCell cell = dungeon[x, y];

        if (!cell.hasWall)
            return false;

        // Проверяем, есть ли рядом проложенный путь
        if (!HasAdjacentPath(x, y))
        {
            Debug.Log("Нет пути рядом с этой стеной!");
            return false;
        }

        // Стоимость ломания стены
        int cost = 50 + (x + y) * 10;

        if (!CurrencyManager.Instance.SpendCheese(cost))
        {
            Debug.Log("Недостаточно сыра для ломания стены!");
            return false;
        }

        // Ломаем стену
        cell.hasWall = false;
        cell.isPath = true;

        // Выдаем награды
        CurrencyManager.Instance.AddCheese(cell.cheeseReward);
        CurrencyManager.Instance.AddSouls(cell.soulsReward);

        return true;
    }

    private bool HasAdjacentPath(int x, int y)
    {
        // Проверяем соседние клетки
        if (x > 0 && dungeon[x - 1, y].isPath) return true;
        if (x < dungeonWidth - 1 && dungeon[x + 1, y].isPath) return true;
        if (y > 0 && dungeon[x, y - 1].isPath) return true;
        if (y < dungeonHeight - 1 && dungeon[x, y + 1].isPath) return true;

        return false;
    }

    public DungeonCell GetCell(int x, int y)
    {
        if (x < 0 || x >= dungeonWidth || y < 0 || y >= dungeonHeight)
            return null;

        return dungeon[x, y];
    }

    public int GetWidth() => dungeonWidth;
    public int GetHeight() => dungeonHeight;

    public void ResetDungeon()
    {
        GenerateDungeon();
    }

    public int GetBreakCost(int x, int y)
    {
        return 50 + (x + y) * 10;
    }
}
