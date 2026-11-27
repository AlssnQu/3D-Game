using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Example";
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private float minLoadTime = 2f;

    public void Play()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        menuPanel.SetActive(false);
        loadingPanel.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);
        op.allowSceneActivation = false;

        float timer = 0f;
        while (op.progress < 0.9f || timer < minLoadTime)
        {
            timer += Time.deltaTime;
            progressBar.value = Mathf.Clamp01(Mathf.Min(op.progress / 0.9f, timer / minLoadTime));
            yield return null;
        }

        progressBar.value = 1f;
        op.allowSceneActivation = true;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}