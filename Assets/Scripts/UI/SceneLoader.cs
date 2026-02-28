using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneLoader : MonoBehaviour
{
    [SerializeField] private UILoadingOverlay loadingOverlay;
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

        if (loadingOverlay) loadingOverlay.Show(loadingText);
        yield return null;

        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            float p = Mathf.Clamp01(op.progress / 0.9f);
            if (loadingOverlay) loadingOverlay.SetProgress01(p);
            yield return null;
        }

        if (loadingOverlay) loadingOverlay.Hide();

        isLoading = false;
        onComplete?.Invoke();
    }
}