using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform dungeonGrid;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Button resetButton;

    [Header("Cell Colors")]
    [SerializeField] private Color wallColor = Color.gray;
    [SerializeField] private Color pathColor = Color.green;
    [SerializeField] private Color emptyColor = Color.white;

    private GameObject[,] cellObjects;

    private void Start()
    {
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetDungeon);

        GenerateDungeonUI();
    }

    private void OnEnable()
    {
        GenerateDungeonUI();
    }

    private void GenerateDungeonUI()
    {
        // Очищаем старую сетку
        if (cellObjects != null)
        {
            for (int x = 0; x < DungeonManager.Instance.GetWidth(); x++)
            {
                for (int y = 0; y < DungeonManager.Instance.GetHeight(); y++)
                {
                    if (cellObjects[x, y] != null)
                        Destroy(cellObjects[x, y]);
                }
            }
        }

        int width = DungeonManager.Instance.GetWidth();
        int height = DungeonManager.Instance.GetHeight();

        cellObjects = new GameObject[width, height];

        // Настраиваем GridLayoutGroup
        GridLayoutGroup grid = dungeonGrid.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = width;
        }

        // Создаем клетки
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, dungeonGrid);
                cellObjects[x, y] = cell;

                int cellX = x;
                int cellY = y;

                Button btn = cell.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnCellClicked(cellX, cellY));
                }

                UpdateCell(x, y);
            }
        }
    }

    private void UpdateCell(int x, int y)
    {
        DungeonManager.DungeonCell cell = DungeonManager.Instance.GetCell(x, y);
        if (cell == null) return;

        GameObject cellObj = cellObjects[x, y];
        if (cellObj == null) return;

        Image img = cellObj.GetComponent<Image>();
        if (img != null)
        {
            if (cell.isPath)
            {
                img.color = pathColor;
            }
            else if (cell.hasWall)
            {
                img.color = wallColor;
            }
            else
            {
                img.color = emptyColor;
            }
        }

        // Добавляем текст с координатами
        TextMeshProUGUI text = cellObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            if (cell.hasWall)
            {
                text.text = "█";
            }
            else if (cell.isPath)
            {
                text.text = "✓";
            }
            else
            {
                text.text = "";
            }
        }
    }

    private void OnCellClicked(int x, int y)
    {
        DungeonManager.DungeonCell cell = DungeonManager.Instance.GetCell(x, y);
        if (cell == null) return;

        if (cell.isPath)
        {
            ShowCellInfo(cell);
            return;
        }

        if (cell.hasWall)
        {
            int cost = DungeonManager.Instance.GetBreakCost(x, y);

            if (infoText != null)
                infoText.text = $"Сломать стену?\nСтоимость: {cost} сыра\nНаграда: {cell.cheeseReward} сыра, {cell.soulsReward} душ";

            if (DungeonManager.Instance.BreakWall(x, y))
            {
                UpdateCell(x, y);
                if (infoText != null)
                    infoText.text = $"Стена сломана!\nПолучено: {cell.cheeseReward} сыра, {cell.soulsReward} душ";
            }
        }
        else
        {
            ShowCellInfo(cell);
        }
    }

    private void ShowCellInfo(DungeonManager.DungeonCell cell)
    {
        if (infoText != null)
        {
            string status = cell.isPath ? "Проложен путь" : cell.hasWall ? "Стена" : "Пусто";
            infoText.text = $"Клетка ({cell.x}, {cell.y})\nСтатус: {status}";
        }
    }

    private void ResetDungeon()
    {
        DungeonManager.Instance.ResetDungeon();
        GenerateDungeonUI();

        if (infoText != null)
            infoText.text = "Подземелье сброшено!";
    }
}
