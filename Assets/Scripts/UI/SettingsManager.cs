using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject settingsPanel;
    public AudioMixer MainMixer;
    public Slider SoundBar1;
    public Slider SoundBar2;
    public WeaponController WPController;
    public SaveLoadManager Leveltracker;
    public Button level2;

    private void Start()
    {
        initSound();

        if (Leveltracker != null)
        {
            bool data_exist = Leveltracker.LoadGame();
            int level_unlocked = Leveltracker.gameData.level;
            if (!data_exist || level_unlocked == 0) level2.interactable = false;
        }
    }

    public void ToggleMainPanel()
    {
        //stop game
        bool isActive = !MainPanel.activeSelf;
        MainPanel.SetActive(isActive);
    }
    public void ToggleSettingsPanel_Simple()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
    public void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        WPController.gamePause = !WPController.gamePause;

        if (settingsPanel.activeSelf)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.visible = false;
        }
    }

    private void initSound()
    {
        float SB1 = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        float SB2 = PlayerPrefs.GetFloat("SEVolume", 0.5f);

        SoundBar1.value = SB1;
        SoundBar2.value = SB2;

        SetBGMVolume(SB1);
        SetSEVolume(SB2);
    }
    public void SetBGMVolume(float volume)
    {
        PlayerPrefs.SetFloat("BGMVolume", volume);

        float dB;
        if (volume <= 0)
            dB = -80f;
        else
            dB = Mathf.Lerp(-80f, 10f, volume);

        MainMixer.SetFloat("BGMVolume", dB);
    }
    public void SetSEVolume(float volume)
    {
        PlayerPrefs.SetFloat("SEVolume", volume);

        float dB;
        if (volume <= 0)
            dB = -80f;
        else
            dB = Mathf.Lerp(-80f, 10f, volume);

        MainMixer.SetFloat("SEVolume", dB);
    }
    public void ToggleMaster()
    {
        int currentMode = PlayerPrefs.GetInt("MasterMode");
        PlayerPrefs.SetInt("MasterMode", currentMode ^ 1);
        Debug.Log(currentMode ^ 1);
    }
}
