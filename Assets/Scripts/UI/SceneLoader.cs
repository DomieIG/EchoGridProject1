// SceneLoader.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    [SerializeField] private UILoadingOverlay loadingOverlay;
    [SerializeField, Min(0f)] private float minimumOverlayTime = 0.15f;

    private bool isLoading;

    private void Awake()
    {
        if (!loadingOverlay) loadingOverlay = FindAnyObjectByType<UILoadingOverlay>();
    }

    public void LoadSceneAsync(string sceneName, string loadingText = "Loading...", Action onComplete = null)
    {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(sceneName, loadingText, onComplete));
    }

    private IEnumerator LoadRoutine(string sceneName, string loadingText, Action onComplete)
    {
        isLoading = true;
        Time.timeScale = 1f;

        float shownAt = Time.unscaledTime;

        if (loadingOverlay) loadingOverlay.Show(loadingText);
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            // progress is 0..0.9 until activation, normalize it
            float p = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingOverlay) loadingOverlay.SetProgress01(p);
            yield return null;
        }

        // Ensure overlay doesn't flicker on super-fast loads
        float elapsed = Time.unscaledTime - shownAt;
        if (elapsed < minimumOverlayTime)
            yield return new WaitForSecondsRealtime(minimumOverlayTime - elapsed);

        if (loadingOverlay) loadingOverlay.Hide();

        isLoading = false;
        onComplete?.Invoke();
    }
}