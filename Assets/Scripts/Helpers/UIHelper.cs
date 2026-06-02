using UnityEngine;

public static class UIHelper
{
    public static void ClearContainer(Transform container)
    {
        if (container == null) return;

        foreach (Transform child in container)
        {
            Object.Destroy(child.gameObject);
        }
    }

    public static void ClearContainerImmediate(Transform container)
    {
        if (container == null) return;

        while (container.childCount > 0)
        {
            Object.DestroyImmediate(container.GetChild(0).gameObject);
        }
    }

    public static void SetActive(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }

    public static void SetInteractable(UnityEngine.UI.Button button, bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    public static void SetText(TMPro.TextMeshProUGUI text, string value)
    {
        if (text != null)
            text.text = value;
    }

    public static void SetText(UnityEngine.UI.Text text, string value)
    {
        if (text != null)
            text.text = value;
    }
}
