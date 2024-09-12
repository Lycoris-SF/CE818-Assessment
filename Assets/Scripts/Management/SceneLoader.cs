using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//failed
public class SceneLoader : MonoBehaviour
{
    private Dictionary<string, AsyncOperation> asyncLoads = new Dictionary<string, AsyncOperation>();
    public List<string> sceneNames;
    public AudioSource BGM_Source;
    public FaderScript fader;
    public Image progressBarFill1;
    public Image progressBarFill2;

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void PreloadScenes(List<string> sceneNames)
    {
        foreach (var sceneName in sceneNames)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
    }
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(FadeIntoLevel(sceneName));
    }
    IEnumerator FadeIntoLevel(string sceneName)
    {
        float fadeTime = fader.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        yield return new WaitForSeconds(0.5f);

        ActivateScene(sceneName);
    }
    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        asyncLoads[sceneName] = asyncLoad;

        while (!asyncLoad.isDone)
        {
            if (sceneName == sceneNames[0]) SetProgress1(asyncLoad.progress);
            else SetProgress2(asyncLoad.progress);
            if (asyncLoad.progress >= 0.9f)
            {
                if (sceneName == sceneNames[0]) SetProgress1(1);
                else SetProgress2(1);
                break;
            }

            yield return null;
        }
    }
    public void SetProgress1(float progress)
    {
        progressBarFill1.fillAmount = progress;
    }
    public void SetProgress2(float progress)
    {
        progressBarFill2.fillAmount = progress;
    }
    public void ActivateScene(string sceneName)
    {
        if (asyncLoads.ContainsKey(sceneName))
        {
            asyncLoads[sceneName].allowSceneActivation = true;
        }
    }
    private void Start()
    {
        Cursor.visible = true;
        BGM_Source.enabled = true;
        //PreloadScenes(sceneNames);
    }
    private void Awake()
    {
        PreloadScenes(sceneNames);
    }
}
