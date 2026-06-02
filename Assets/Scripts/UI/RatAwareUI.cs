using UnityEngine;

/// <summary>
/// Базовый класс для UI компонентов, которым нужно реагировать на изменения крыс.
/// Автоматически подписывается и отписывается от событий RatManager.
/// </summary>
public abstract class RatAwareUI : MonoBehaviour
{
    protected virtual void Start()
    {
        SubscribeToRatEvents();
        OnStart();
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeFromRatEvents();
        OnCleanup();
    }

    private void SubscribeToRatEvents()
    {
        if (RatManager.Instance != null)
        {
            RatManager.Instance.OnRatAdded += OnRatChanged;
            RatManager.Instance.OnRatRemoved += OnRatChanged;
            RatManager.Instance.OnRatUpdated += OnRatChanged;
        }
    }

    private void UnsubscribeFromRatEvents()
    {
        if (RatManager.Instance != null)
        {
            RatManager.Instance.OnRatAdded -= OnRatChanged;
            RatManager.Instance.OnRatRemoved -= OnRatChanged;
            RatManager.Instance.OnRatUpdated -= OnRatChanged;
        }
    }

    /// <summary>
    /// Вызывается когда крыса добавлена, удалена или обновлена.
    /// </summary>
    protected abstract void OnRatChanged(Rat rat);

    /// <summary>
    /// Переопределите этот метод вместо Start() для инициализации.
    /// </summary>
    protected virtual void OnStart() { }

    /// <summary>
    /// Переопределите этот метод вместо OnDestroy() для очистки.
    /// </summary>
    protected virtual void OnCleanup() { }
}
