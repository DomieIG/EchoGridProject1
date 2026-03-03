// UIEase.cs
public static class UIEase
{
    // Smooth & snappy without overshoot
    public static float EaseOutCubic(float t)
    {
        t = Clamp01(t);
        float inv = 1f - t;
        return 1f - (inv * inv * inv);
    }

    public static float Clamp01(float t) => t < 0f ? 0f : (t > 1f ? 1f : t);
}