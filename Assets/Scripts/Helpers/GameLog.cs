using System.Diagnostics;

/// <summary>
/// Информационное логирование. Вызовы полностью вырезаются из релизных билдов
/// (компилируются только в редакторе и в development-сборках), поэтому не создают
/// нагрузки в проде — аргументы тоже не вычисляются.
/// Для ошибок и предупреждений используйте Debug.LogError / Debug.LogWarning напрямую.
/// </summary>
public static class GameLog
{
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }
}
