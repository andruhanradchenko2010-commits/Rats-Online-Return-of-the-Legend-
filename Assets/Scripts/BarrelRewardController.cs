using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BarrelRewardController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject barrel;
    [SerializeField] private GameObject cheesePrefab;
    [SerializeField] private Transform cheeseSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float barrelAutoDisappearTime = 2f;
    [SerializeField] private float cheeseAutoCollectTime = 5f;
    [SerializeField] private Vector2 cheeseSpawnRadius = new Vector2(100f, 100f);

    private Button barrelButton;
    private bool barrelClicked = false;
    private int cheeseAmount = 0;
    private List<GameObject> spawnedCheeses = new List<GameObject>();

    private void Start()
    {
        if (barrel != null)
        {
            barrel.SetActive(false);
            barrelButton = barrel.GetComponent<Button>();
            if (barrelButton != null)
                barrelButton.onClick.AddListener(OnBarrelClicked);
        }

        GameManager.Instance.OnBarrelRewardReady += ShowBarrel;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnBarrelRewardReady -= ShowBarrel;
    }

    private void ShowBarrel(int amount)
    {
        cheeseAmount = amount;
        barrelClicked = false;

        if (barrel != null)
        {
            barrel.SetActive(true);
            StartCoroutine(AutoDisappearBarrel());
        }
    }

    private IEnumerator AutoDisappearBarrel()
    {
        yield return new WaitForSeconds(barrelAutoDisappearTime);

        if (!barrelClicked && barrel != null && barrel.activeSelf)
        {
            OnBarrelClicked();
        }
    }

    private void OnBarrelClicked()
    {
        barrelClicked = true;

        if (barrel != null)
            barrel.SetActive(false);

        SpawnCheeses();
        StartCoroutine(AutoCollectAllCheese());
    }

    private void SpawnCheeses()
    {
        if (cheesePrefab == null)
        {
            Debug.LogError("Cheese prefab is null!");
            return;
        }

        Transform spawnParent = cheeseSpawnPoint != null ? cheeseSpawnPoint : transform;
        Vector3 basePosition = spawnParent.position;

        for (int i = 0; i < cheeseAmount; i++)
        {
            Vector3 spawnPos = basePosition;
            spawnPos.x += Random.Range(-cheeseSpawnRadius.x, cheeseSpawnRadius.x);
            spawnPos.y += Random.Range(-cheeseSpawnRadius.y, cheeseSpawnRadius.y);

            GameObject cheese = Instantiate(cheesePrefab, spawnParent.parent);
            RectTransform cheeseRect = cheese.GetComponent<RectTransform>();
            if (cheeseRect != null)
            {
                cheeseRect.anchoredPosition = new Vector2(spawnPos.x, spawnPos.y);
            }
            else
            {
                cheese.transform.position = spawnPos;
            }

            spawnedCheeses.Add(cheese);

            Button cheeseButton = cheese.GetComponent<Button>();
            if (cheeseButton != null)
            {
                cheeseButton.onClick.AddListener(() => OnCheeseClicked(cheese));
            }
        }

        GameManager.Instance.ClearPendingReward();
    }

    private IEnumerator AutoCollectAllCheese()
    {
        yield return new WaitForSeconds(cheeseAutoCollectTime);

        int remainingCheese = spawnedCheeses.Count;
        if (remainingCheese > 0)
        {
            GameManager.Instance.AddCheese(remainingCheese);

            foreach (GameObject cheese in spawnedCheeses)
            {
                if (cheese != null)
                    Destroy(cheese);
            }

            spawnedCheeses.Clear();
        }
    }

    private void OnCheeseClicked(GameObject cheese)
    {
        if (cheese != null && spawnedCheeses.Contains(cheese))
        {
            spawnedCheeses.Remove(cheese);
            Destroy(cheese);
            GameManager.Instance.AddCheese(1);
        }
    }
}
