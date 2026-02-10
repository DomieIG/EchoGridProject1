// AlarmLog.cs
using UnityEngine;

public static class AlarmLog
{
    // Toggle in Project Settings → Player → Scripting Define Symbols:
    // Add: ALARM_DEBUG

    [System.Diagnostics.Conditional("ALARM_DEBUG")]
    public static void Info(Object ctx, string msg) =>
        Debug.Log($"[Alarm] {msg}", ctx);

    [System.Diagnostics.Conditional("ALARM_DEBUG")]
    public static void Warn(Object ctx, string msg) =>
        Debug.LogWarning($"[Alarm] {msg}", ctx);

    [System.Diagnostics.Conditional("ALARM_DEBUG")]
    public static void Error(Object ctx, string msg) =>
        Debug.LogError($"[Alarm] {msg}", ctx);
}
