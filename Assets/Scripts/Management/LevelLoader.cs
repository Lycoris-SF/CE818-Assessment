using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public AudioSource BGM_Source;
    public FaderScript fader;

    public void LoadLevel(string levelName)
    {
        StartCoroutine(FadeAndLoadLevel(levelName));
    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator FadeAndLoadLevel(string levelName)
    {
        float fadeTime = fader.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            asyncLoad.allowSceneActivation = true;
            yield return null;
        }
    }

    private void Start()
    {
        PlayerPrefs.SetInt("MasterMode", 0);
        Cursor.visible = true;
        BGM_Source.enabled = true;
    }
}
