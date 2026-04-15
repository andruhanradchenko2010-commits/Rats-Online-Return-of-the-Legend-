using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class BarrelRewardSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Barrel Reward System")]
    public static void SetupBarrelReward()
    {
        // Создаем prefab для сыра
        GameObject cheesePrefab = CreateCheesePrefab();

        // Находим или создаем Canvas на сцене
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found on scene!");
            return;
        }

        // Создаем BarrelRewardController GameObject
        GameObject controllerObj = new GameObject("BarrelRewardController");
        controllerObj.transform.SetParent(canvas.transform, false);
        BarrelRewardController controller = controllerObj.AddComponent<BarrelRewardController>();

        // Создаем бочку
        GameObject barrel = CreateBarrel(canvas.transform);

        // Создаем точку спавна
        GameObject spawnPoint = new GameObject("CheeseSpawnPoint");
        RectTransform spawnRect = spawnPoint.AddComponent<RectTransform>();
        spawnRect.SetParent(canvas.transform, false);
        spawnRect.anchoredPosition = Vector2.zero;

        // Настраиваем контроллер через SerializedObject
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("barrel").objectReferenceValue = barrel;
        so.FindProperty("cheesePrefab").objectReferenceValue = cheesePrefab;
        so.FindProperty("cheeseSpawnPoint").objectReferenceValue = spawnPoint.transform;
        so.FindProperty("barrelAutoDisappearTime").floatValue = 2f;
        so.FindProperty("cheeseSpawnRadius").vector2Value = new Vector2(200f, 150f);
        so.ApplyModifiedProperties();

        Debug.Log("Barrel Reward System setup complete!");
        EditorUtility.SetDirty(controller);
    }

    private static GameObject CreateBarrel(Transform parent)
    {
        GameObject barrel = new GameObject("Barrel");
        RectTransform rectTransform = barrel.AddComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.sizeDelta = new Vector2(200, 200);
        rectTransform.anchoredPosition = Vector2.zero;

        Image image = barrel.AddComponent<Image>();
        Sprite barrelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Бочка сыра.png");
        if (barrelSprite != null)
            image.sprite = barrelSprite;

        Button button = barrel.AddComponent<Button>();
        barrel.SetActive(false);

        return barrel;
    }

    private static GameObject CreateCheesePrefab()
    {
        GameObject cheese = new GameObject("CheesePrefab");
        RectTransform rectTransform = cheese.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        Image image = cheese.AddComponent<Image>();
        Sprite cheeseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/Сыр.png");
        if (cheeseSprite != null)
            image.sprite = cheeseSprite;

        Button button = cheese.AddComponent<Button>();

        // Сохраняем как prefab
        string prefabPath = "Assets/Prefabs/CheesePrefab.prefab";

        // Создаем папку Prefabs если её нет
        if (!Directory.Exists("Assets/Prefabs"))
            Directory.CreateDirectory("Assets/Prefabs");

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cheese, prefabPath);
        DestroyImmediate(cheese);

        return prefab;
    }
}
