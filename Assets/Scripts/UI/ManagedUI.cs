using UnityEngine;

public abstract class ManagedUI : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        SubscribeToEvents();
        RefreshUI();
    }

    protected virtual void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    protected abstract void SubscribeToEvents();
    protected abstract void UnsubscribeFromEvents();
    protected abstract void RefreshUI();
}
