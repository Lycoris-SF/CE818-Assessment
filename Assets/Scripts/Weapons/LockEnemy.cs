// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using static StaticGameDB;
using static Unity.VisualScripting.Member;

public class LockEnemy : MonoBehaviour
{
    public LeadPointer Main_leadPointer;
    public LeadPointer MachineGun_leadPointer;
    private GameObject Enemy;
    public AimConstraint MainAim;
    public List<AimConstraint> MachineGunAim;
    public Transform MachineGunRF;
    public Camera MainCamera;
    public float maxAngle = 45f;
    public float LockminDistance = 2500;
    public float LockminAngle = 15;
    private Vector3 MachineGunLocalorigin;
    public bool lock_mode = false;
    private bool calibrating = true;
    private LineRenderer lineRenderer;

    void Start()
    {
        maxAngle = Player_data.LockAngle;
        LockminDistance = Player_data.LockRange;

        StartCoroutine(calibrate_machine_gun(2));
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        SetupLineRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        Update_LineRender();
    }

    private void FixedUpdate()
    {
        if (!calibrating)
        {
            if (Enemy == null) 
            {
                if (lock_mode) 
                {
                    unlock_target();  
                }
                return; 
            }

            if (lock_mode) 
            {
                Vector3 directionToEnemy = Enemy.transform.position - MainCamera.transform.position;
                directionToEnemy.Normalize();
                Vector3 cameraForward = MainCamera.transform.forward;
                float angle = Vector3.Angle(cameraForward, directionToEnemy);

                ApplyConstraintsBasedOnAngle(angle);
            }
        }
    }

    private void ApplyConstraintsBasedOnAngle(float angle)
    {
        bool shouldActivate = angle <= maxAngle;
        if(shouldActivate)
        {
            MainAim.constraintActive = true;
            foreach (AimConstraint aim in MachineGunAim)
            {
                if (aim != null)
                {
                    aim.constraintActive = true;
                }
            }
        }
        else{
            unlock_target();
        }
    }

    public void Lock_On_Closest()
    {
        GameObject[] AllEnemys = GameObject.FindGameObjectsWithTag("Ship");
        GameObject bestTarget = null;
        float bestScore = float.MinValue;

        foreach (GameObject enemy in AllEnemys)
        {
            GameObject EnemyRoot = enemy.transform.root.gameObject;
            float distance = Vector3.Distance(MainCamera.transform.position, EnemyRoot.transform.position);
            if (distance < LockminDistance)
            {
                Vector3 directionToEnemy = (EnemyRoot.transform.position - MainCamera.transform.position).normalized;
                float angle = Vector3.Angle(MainCamera.transform.forward, directionToEnemy);

                if (angle < LockminAngle)
                {
                    float score = CalculateScore(distance, angle);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = EnemyRoot;
                    }
                }
            }
        }

        Enemy = bestTarget;
        if (Enemy)
        {
            Set_Target(); lock_mode = true;
        }
    }

    private float CalculateScore(float distance, float angle)
    {
        float distanceScore = 1f / distance; 
        float angleScore = 2f / angle;       

        return distanceScore + angleScore;
    }
    IEnumerator calibrate_machine_gun(float transitionDuration)
    {
        Vector3[] startRotations = new Vector3[MachineGunAim.Count];
        for (int i = 0; i < MachineGunAim.Count; i++)
        {
            startRotations[i] = Vector3.zero;
        }

        Vector3[] targetRotations = new Vector3[MachineGunAim.Count];

        float timeElapsed = 0;
        while (timeElapsed < transitionDuration)
        {
            for (int i = 0; i < MachineGunAim.Count; i++)
            {
                if (MachineGunAim[i] != null)
                {
                    Quaternion difference = Quaternion.Inverse(MainCamera.transform.rotation) * MachineGunRF.rotation;
                    float angleX = NormalizeAngle(difference.eulerAngles.x);
                    float angleY = NormalizeAngle(difference.eulerAngles.y);
                    targetRotations[i] = new Vector3(-angleX, -angleY, 0);
                    Vector3 currentRotation = Vector3.Lerp(startRotations[i], targetRotations[i], timeElapsed / transitionDuration);
                    MachineGunAim[i].rotationAtRest = currentRotation;
                }
            }
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < MachineGunAim.Count; i++)
        {
            if (MachineGunAim[i] != null)
            {
                MachineGunAim[i].rotationAtRest = targetRotations[i];
                MachineGunLocalorigin = MachineGunAim[i].transform.localEulerAngles;
            }
        }
        calibrating = false;
    }
    IEnumerator return_machine_gun(float transitionDuration)
    {
        float targetXRotation = MachineGunLocalorigin.x;

        for (int i = 0; i < MachineGunAim.Count; i++)
        {
            if (MachineGunAim[i] != null)
            {
                Quaternion originalRotation = Quaternion.Euler(MachineGunAim[i].transform.localEulerAngles);
                Quaternion targetRotation = Quaternion.Euler(MachineGunLocalorigin);

                float timeElapsed = 0;
                while (timeElapsed < transitionDuration)
                {
                    Quaternion currentRotationQ = Quaternion.Lerp(originalRotation, targetRotation, timeElapsed / transitionDuration);
                    MachineGunAim[i].transform.localEulerAngles = currentRotationQ.eulerAngles;
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }

                MachineGunAim[i].transform.localEulerAngles = new Vector3(targetXRotation, 0, 0);
            }
        }
    }

    public void LockActivate()
    {
        Lock_On_Closest();
    }
    public void LockDeactivate()
    {
        unlock_target();
    }
    private void unlock_target()
    {
        lock_mode = false;
        Enemy = null;
        MainAim.constraintActive = false;
        MainAim.transform.localEulerAngles = Vector3.zero;
        foreach (AimConstraint aim in MachineGunAim)
        {
            if (aim != null)
            {
                aim.constraintActive = false;
            }
        }
        StartCoroutine(return_machine_gun(0.5f));
    }

    private void Set_Target()
    {
        Main_leadPointer.target = Enemy.transform;
        Main_leadPointer.targetRigidbody = Enemy.GetComponent<Rigidbody>();
        MachineGun_leadPointer.target = Enemy.transform;
        MachineGun_leadPointer.targetRigidbody = Enemy.GetComponent<Rigidbody>();
        UpdateMainAimSource(Main_leadPointer.aimPoint);
        UpdateMachineGunAimSource(MachineGun_leadPointer.aimPoint);
    }
    void UpdateMainAimSource(Transform target)
    {
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = target;
        source.weight = 1; 

        MainAim.RemoveSource(0);
        MainAim.AddSource(source);
        MainAim.constraintActive = true;
    }
    void UpdateMachineGunAimSource(Transform target)
    {
        ConstraintSource source = new ConstraintSource();
        source.sourceTransform = target;
        source.weight = 1;

        foreach(AimConstraint aim in MachineGunAim)
        {
            aim.RemoveSource(0);
            aim.AddSource(source);
            aim.constraintActive = true;
            aim.rotationAtRest = Vector3.zero;
            aim.rotationOffset = Vector3.zero;
        }
    }
    float NormalizeAngle(float angle)
    {
        while (angle > 180)
            angle -= 360;
        while (angle < -180)
            angle += 360;
        return angle;
    }

    private void SetupLineRenderer()
    {
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;

        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        lineRenderer.positionCount = 2;

        lineRenderer.numCornerVertices = 4;
        lineRenderer.numCapVertices = 4;

        lineRenderer.SetPosition(0, MainAim.transform.position);
        lineRenderer.SetPosition(1, MainAim.transform.position + MainAim.transform.forward * 100);

        Material beamMaterial = Resources.Load<Material>("Beam");
        if (beamMaterial != null)
        {
            lineRenderer.material = beamMaterial;
        }
        else
        {
            Debug.LogError("Material 'Beam' not found. Please ensure it's in the Resources folder.");
        }
    }
    private void Update_LineRender()
    {
        lineRenderer.SetPosition(0, MainAim.transform.position);
        lineRenderer.SetPosition(1, MainAim.transform.position + MainAim.transform.forward * 2000);
    }

    public float Get_LockDis()
    {
        return Vector3.Distance(MainCamera.transform.position, Enemy.transform.position);
    }
}
