using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BreedingUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform ratSelectionContainer;
    [SerializeField] private GameObject ratSelectionItemPrefab;
    [SerializeField] private Image rat1Image;
    [SerializeField] private Image rat2Image;
    [SerializeField] private TextMeshProUGUI rat1InfoText;
    [SerializeField] private TextMeshProUGUI rat2InfoText;
    [SerializeField] private Button breedButton;
    [SerializeField] private TextMeshProUGUI breedCostText;
    [SerializeField] private TextMeshProUGUI resultPreviewText;

    private Rat selectedRat1;
    private Rat selectedRat2;

    private void Start()
    {
        if (breedButton != null)
        {
            breedButton.onClick.AddListener(BreedRats);
            breedButton.interactable = false;
        }

        RefreshRatSelection();
    }

    private void OnEnable()
    {
        RefreshRatSelection();
    }

    private void RefreshRatSelection()
    {
        // Очищаем список
        foreach (Transform child in ratSelectionContainer)
        {
            Destroy(child.gameObject);
        }

        // Показываем только крыс максимального уровня для своего вида
        List<Rat> rats = RatManager.Instance.GetAllRats();
        foreach (Rat rat in rats)
        {
            if (rat.CanEvolve() && rat.CanFight())
            {
                GameObject item = Instantiate(ratSelectionItemPrefab, ratSelectionContainer);
                Button btn = item.GetComponent<Button>();
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();

                if (text != null)
                    text.text = $"{rat.type.GetDisplayName()} Ур.{rat.level}";

                if (btn != null)
                    btn.onClick.AddListener(() => SelectRat(rat));
            }
        }
    }

    private void SelectRat(Rat rat)
    {
        if (selectedRat1 == null)
        {
            selectedRat1 = rat;
            UpdateRatDisplay(1);
        }
        else if (selectedRat2 == null && rat.id != selectedRat1.id)
        {
            selectedRat2 = rat;
            UpdateRatDisplay(2);
        }
        else
        {
            // Сброс выбора
            selectedRat1 = rat;
            selectedRat2 = null;
            UpdateRatDisplay(1);
            UpdateRatDisplay(2);
        }

        UpdateBreedButton();
    }

    private void UpdateRatDisplay(int slot)
    {
        Rat rat = slot == 1 ? selectedRat1 : selectedRat2;
        Image img = slot == 1 ? rat1Image : rat2Image;
        TextMeshProUGUI info = slot == 1 ? rat1InfoText : rat2InfoText;

        if (rat != null)
        {
            if (info != null)
            {
                info.text = $"{rat.type.GetDisplayName()}\n" +
                           $"Уровень: {rat.level}\n" +
                           $"Воровство: {rat.theftSkill}\n" +
                           $"Добыча: {rat.miningSkill}\n" +
                           $"Защита: {rat.defenseSkill}\n" +
                           $"Атака: {rat.attackSkill}";
            }

            if (img != null)
            {
                // TODO: Загрузить спрайт
                img.color = Color.white;
            }
        }
        else
        {
            if (info != null)
                info.text = "Не выбрана";

            if (img != null)
                img.color = new Color(1, 1, 1, 0.3f);
        }
    }

    private void UpdateBreedButton()
    {
        bool canBreed = selectedRat1 != null && selectedRat2 != null &&
                       selectedRat1.type == selectedRat2.type &&
                       selectedRat1.CanEvolve() && selectedRat2.CanEvolve();

        if (breedButton != null)
            breedButton.interactable = canBreed;

        if (breedCostText != null)
            breedCostText.text = "Стоимость: 1 эликсир";

        if (resultPreviewText != null && canBreed)
        {
            RatType nextType = (RatType)((int)selectedRat1.type + 1);
            resultPreviewText.text = $"Результат: {nextType.GetDisplayName()}";
        }
        else if (resultPreviewText != null)
        {
            resultPreviewText.text = "Выберите 2 крысы одного вида";
        }
    }

    private void BreedRats()
    {
        if (selectedRat1 == null || selectedRat2 == null)
            return;

        Rat newRat = RatManager.Instance.BreedRats(selectedRat1, selectedRat2);

        if (newRat != null)
        {
            Debug.Log($"Скрещивание успешно! Получена {newRat.type.GetDisplayName()} уровня {newRat.level}");
            selectedRat1 = null;
            selectedRat2 = null;
            RefreshRatSelection();
            UpdateRatDisplay(1);
            UpdateRatDisplay(2);
            UpdateBreedButton();
        }
        else
        {
            Debug.Log("Скрещивание не удалось!");
        }
    }
}
