using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public PlayerController PLC;
    public GameController GC;
    public EnemyIndicator EI;

    private List<GameObject> EnemyFlags;

    private int FlagsOriginalNum;
    private int TargetLeft;
    private string sceneName;
    private bool CanCheckFlag;

    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        CanCheckFlag = false;

        if (sceneName == "SampleScene")
        {
            StartCoroutine(lateInit_Flags(0.1f));
        }
        else if (sceneName == "Loum")
        {
            //need to be upgrade
            StartCoroutine(lateInit_Flags(0.5f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        Get_Target_Left();
        check_mission_complete();
        if (CanCheckFlag)
        {
            check_indicator();
        }
    }
    private void check_indicator()
    {
        EnemyFlags.RemoveAll(item => item == null);
        if (sceneName == "SampleScene")
        {
            if (EnemyFlags.Count != 0)
            {
                EI.enemy = EnemyFlags.First();
            }
            else
            {
                EI.enemy = null;
            }
        }
        //need to be upgrade
        else if (sceneName == "Loum")
        {
            if (EnemyFlags.Count != 0)
            {
                EI.enemy = EnemyFlags.First();
            }
            else
            {
                EI.enemy = null;
            }
        }
    }

    private void check_mission_complete()
    {
        if (PLC.checkPlayerHealth() <= 0)
        {
            // back menu
            GC.Mission_status = -1;
        }
        if (sceneName == "SampleScene")
        {
            GameObject[] EFSF_Count = GameObject.FindGameObjectsWithTag("EFSF-Team");
            if (EFSF_Count.Length == 0)
            {
                GC.Mission_status = 1;
            }
        }
        else if (sceneName == "Loum")
        {
            GameObject[] EFSF_Count = GameObject.FindGameObjectsWithTag("EFSF-FlagShip");
            TargetLeft = 5 - (FlagsOriginalNum - EFSF_Count.Length);
            if (FlagsOriginalNum - EFSF_Count.Length >=5)
            {
                //special ending for loum
                GC.Mission_status = 2;
            }
        }
    }
    IEnumerator lateInit_Flags(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        // should only do once (delay call) on start up
        if (sceneName == "SampleScene")
        {
            EnemyFlags = GameObject.FindGameObjectsWithTag("EFSF-Team").ToList();
        }
        else if (sceneName == "Loum")
        {
            EnemyFlags = GameObject.FindGameObjectsWithTag("EFSF-FlagShip").ToList();
            FlagsOriginalNum = EnemyFlags.Count();
        }

        CanCheckFlag = true;
    }

    public void Get_Target_Left()
    {
        //UI
        PLC.Debug_text[3].text = TargetLeft.ToString() + " :TargetLeft";
    }
}
