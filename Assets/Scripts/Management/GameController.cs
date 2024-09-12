using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public FaderScript fader;
    public SettingsManager settingManager;
    //public PlayerController PLC;
    public PhotoMode photoMode;


    // 0: mission on, -1: fail, 1: success
    public int Mission_status;

    void Start()
    {
        Mission_status = 0;
    }
    private void FixedUpdate()
    {
        if (Mission_status == -1)
        {
            // back menu
            StartCoroutine(FadeAndLoadLevel("MainTitle"));
        }
        else if(Mission_status == 1)
        {
            GetComponent<SaveLoadManager>().gameData.level = 1;
            GetComponent<SaveLoadManager>().SaveGame();

            //went to menu, now level2 should unlocked
            StartCoroutine(FadeAndLoadLevel("MainTitle"));
        }
        else if(Mission_status == 2)
        {
            //special ending for loum
            // TODO
            StartCoroutine(FadeAndLoadLevel("MainTitle"));
        }
    }
    // Update is called once per frame
    void Update()
    {
        game_window();
    }
    private void game_window()
    {
        if (!Application.isEditor) // Not in editor
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) && !photoMode.flycameraOn)
            {
                settingManager.ToggleSettingsPanel();
            }
            if (Input.GetKeyDown(KeyCode.P) && !settingManager.settingsPanel.activeSelf)
            {
                if (photoMode.flycameraOn) photoMode.ExitPhotoMode();
                else photoMode.EnterPhotoMode();
            }
        }
#if UNITY_EDITOR
        else // In editor
        {
            if (Input.GetMouseButtonDown(0) && Time.timeScale != 0)
            {
                Vector2 mousePosition = Input.mousePosition;
                Rect gameViewRect = new Rect(0, 0, Screen.width + 1, Screen.height + 1);

                if (gameViewRect.Contains(mousePosition))
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Confined;
                }
            }
            if (Input.GetKeyDown(KeyCode.P)&& !settingManager.settingsPanel.activeSelf)
            {
                if (photoMode.flycameraOn) photoMode.ExitPhotoMode();
                else photoMode.EnterPhotoMode();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && !photoMode.flycameraOn)
            {
                settingManager.ToggleSettingsPanel();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }
#endif
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    public void LoadLevel(string levelName)
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        StartCoroutine(FadeAndLoadLevel(levelName));
    }
    IEnumerator FadeAndLoadLevel(string levelName)
    {
        float fadeTime = fader.BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        yield return new WaitForSeconds(0.5f);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            Cursor.lockState = CursorLockMode.None;
            asyncLoad.allowSceneActivation = true;
            yield return null;
        }

    }

}
public static class StaticGameDB
{
    public class Player_data
    {
        public static int BaseHealth = 500;
        public static int LockRange = 4500;
        public static int LockAngle = 50;
        public static float ESP_factor = 0.5f;
    }
    public class bullet_machinegun_data
    {
        public static int BulletDamage = 5;
        public static int BulletMuzzleVelocity = 750;
        public static int BulletBasePenetration = 50;
        public static int RicochetAngle = 30;
        public static int Bulletlifetime = 4;
    }

    public class nuclear_rocket_data
    {
        public static int RocketDamage = 450;
        public static int RocketMuzzleVelocity = 550;
        public static int MetalJetMuzzleVelocity = 100;
        public static int RocketBasePenetration = 1000;
        public static int RocketExplosionRadius = 35;
        public static int RocketfireRate = 5; // 5s reload
        public static int Rocketlifetime = 10;
        //after effect must move really slower than designed
        public static float AFESpeedLowerFactor = 0.00001f;
    }
    public class mega_particle_cannon
    {
        public static int BeamLenthNormal = 550;
        public static int BeamDamage = 450;
        public static int BeamMuzzleVelocity = 550;
        public static int BeamBasePenetration = 3500;
        public static float Beamlifetime = 10;
        public static int MaxLockRange = 3000;
        public static int MinLockAngle = 5;
    }
    public class SARA_data
    {
        public static int BaseHealth = 3500;
        public static int BaseArmor = 450;
        public static int TurretBaseHealth = 350;
        public static int TurretBaseMass = 50;
        public static int TurretBarrelHealth = 100;
        public static int TurretBarrelMass = 8;
        public static int AmmoRacklHealth = 230;
        public static int FuelTankHealth = 300;

        public static int EngineHealth = 450;
        public static int EngineBurnDamageTick = 50;
        public static float EngineBurnDuration = 10f;
        public static float EngineBurnInterval = 0.5f;


        public static int AmmunitionExplodeDMG = 950;
        public static int FuelExplodeDMG = 750;

        public static float Mega_FiringTime = 0.85f;
        public static float Mega_ReloadTime = 15f;
    }

    public class MAGE_data
    {
        public static int BaseHealth = 9500;
        public static int BaseArmor = 600;
        public static int TurretBaseHealth = 650;
        public static int TurretBaseMass = 100;
        public static int TurretBarrelHealth = 100;
        public static int TurretBarrelMass = 12;

        //Module Health for MAGE is dynamic
        //public static int AmmoRacklHealth = 350;
        //public static int FuelTankHealth = 500;

        public static int EngineHealth = 1050;
        public static int EngineBurnDamageTick = 75;
        public static float EngineBurnDuration = 12f;
        public static float EngineBurnInterval = 0.5f;


        //Module DMG for MAGE is dynamic
        //public static int AmmunitionExplodeDMG = 1150;
        //public static int FuelExplodeDMG = 950;

        public static float Mega_FiringTime = 1.5f;
        public static float Mega_ReloadTime = 20f;
    }

    public class ram_dmg_data
    {
        public static float ramFactor = 0.1f; // K energy is too large for a dmg
        public static int minVelocity = 15; // min V to start cal dmg
        public static int minVelocityMS = 75; // min V to start cal dmg for MS
        public static int maxDamageShip = 1550; // max ram dmg for ships
        public static int maxDamageModule = 450;
        public static int maxDamageMS = 550;
    }
}
