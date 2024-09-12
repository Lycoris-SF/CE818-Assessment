using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class TurretLogic : MonoBehaviour
{
    public Transform baseRotation; 
    public List<Transform> barrelRotations;
    public Transform target;
    public EnemyWeaponController EnemyWeaponController;
    //public SubGOHitLogic BaseSubGO;
    //public SubGOHitLogic BarrelSubGO;
    public float maxBaseTurnSpeed = 30f;
    public float maxBarrelTurnSpeed = 30f; 
    public float baseAcceleration = 5f; 
    public float barrelAcceleration = 5f;
    public float depressionLimit = -10f;
    public float elevationLimit = 45f;
    public float bulletSpeed;

    public List<Text> DebugText;

    private float BarrelYoffset;
    private float BarrelZoffset;
    private float BaseXoffset;
    private float BaseZoffset;
    private float currentBaseSpeed = 0f; 
    private float currentBarrelSpeed = 0f;
    private int masterFactor = 1;
    private int min_lock_angle;
    private int max_lock_range;

    private float lockOnTimer = 0f;
    private float lockOnDuration = 1f; 

    public bool shipDestroyed = false;
    private bool Firing;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        BarrelYoffset = NormalizeAngle(barrelRotations[0].localRotation.eulerAngles.y);
        BarrelZoffset = NormalizeAngle(barrelRotations[0].localRotation.eulerAngles.z);
        //BaseXoffset = NormalizeAngle(baseRotation.localRotation.eulerAngles.x);
        //BaseZoffset = NormalizeAngle(baseRotation.localRotation.eulerAngles.z);
        if (PlayerPrefs.GetInt("MasterMode") == 1)
        {
            masterFactor = 2;
        }

        baseAcceleration *= masterFactor;
        barrelAcceleration *= masterFactor;
        bulletSpeed = StaticGameDB.mega_particle_cannon.BeamMuzzleVelocity;
        min_lock_angle = StaticGameDB.mega_particle_cannon.MinLockAngle;
        max_lock_range = StaticGameDB.mega_particle_cannon.MaxLockRange;

        EnemyWeaponController.trigger = true;
    }
    private void FixedUpdate()
    {
        if (!shipDestroyed)
        {
            RotateBase_V3();
            RotateBarrel_V4();
        }
    }
    void Update()
    {
        Firing = EnemyWeaponController.beamFiring;
        if (!shipDestroyed)
        {
            NearJG_V2();
        }
    }

    private void NearJG()
    {
        if(target.tag == "Player")
        {
            max_lock_range = 1000;
        }
        Vector3 targetDir = GerTargetDir(baseRotation);
        //Vector3 gunPosition = EnemyWeaponController.gunRoots[0].position;
        Vector3 gunDirection = EnemyWeaponController.gunRoots[0].rotation * Vector3.forward;


        if (targetDir.magnitude < 150)
        {
            EnemyWeaponController.trigger = true;
        }
        else if(targetDir.magnitude >= 150 && targetDir.magnitude < max_lock_range)
        {
            float rela = Vector3.Angle(gunDirection, targetDir);
            EnemyWeaponController.trigger = rela < min_lock_angle;
        }
        else
        {
            EnemyWeaponController.trigger = false;
        }
    }
    private void NearJG_V2()
    {
        if (target.tag == "Player")
        {
            max_lock_range = 1000;
        }
        Vector3 targetDir = GerTargetDir(baseRotation);
        Vector3 gunDirection = EnemyWeaponController.gunRoots[0].rotation * Vector3.forward;

        float rela = Vector3.Angle(gunDirection, targetDir);

        if (targetDir.magnitude < 150)
        {
            EnemyWeaponController.trigger = true;
            lockOnTimer = 0; 
        }
        else if (targetDir.magnitude >= 150 && targetDir.magnitude < max_lock_range)
        {
            if (rela < min_lock_angle)
            {
                lockOnTimer += Time.deltaTime; 
                if (lockOnTimer >= lockOnDuration)
                {
                    EnemyWeaponController.trigger = true;
                }
            }
            else
            {
                lockOnTimer = 0;
                EnemyWeaponController.trigger = false;
            }
        }
        else
        {
            EnemyWeaponController.trigger = false;
            lockOnTimer = 0; 
        }
    }

    private void RotateBaseTowardsTarget()
    {
        Vector3 targetDir = GerTargetDir(baseRotation);

        float angleDiff = Vector3.SignedAngle(baseRotation.forward, targetDir, Vector3.up);

        currentBaseSpeed = Mathf.Min(currentBaseSpeed + masterFactor * baseAcceleration * Time.deltaTime, maxBaseTurnSpeed);

        float rotateAngle = Mathf.Clamp(angleDiff, -currentBaseSpeed * Time.deltaTime, currentBaseSpeed * Time.deltaTime);
        if (Mathf.Abs(angleDiff) < 0.05f) 
        { rotateAngle = 0; }
        //DebugText[0].text = Time.deltaTime.ToString();
        baseRotation.Rotate(Vector3.up, rotateAngle, Space.Self);
    }

    private void RotateBarrelTowardsTarget()
    {
        Transform barrelRotation = barrelRotations[0];
        Vector3 targetDir = target.position - barrelRotation.position;
        Vector3 predictedTargetPosition = target.gameObject.GetComponent<Rigidbody>().velocity * (targetDir.magnitude / bulletSpeed);
        targetDir += predictedTargetPosition;
        targetDir.Normalize();

        //Debug.DrawRay(barrelRotation.position, targetDir*100, Color.blue);

        float angleDiff = Vector3.SignedAngle(barrelRotation.forward, targetDir, -Vector3.right);

        currentBarrelSpeed = Mathf.Min(currentBarrelSpeed + masterFactor * barrelAcceleration * Time.deltaTime, maxBarrelTurnSpeed);

        float rotateAngle = Mathf.Clamp(angleDiff, -currentBarrelSpeed * Time.deltaTime, currentBarrelSpeed * Time.deltaTime);
        //if (Mathf.Abs(angleDiff) < 0.03f) { rotateAngle = 0; }

        float tempx = NormalizeAngle(barrelRotation.localRotation.eulerAngles.x);
        if (tempx <= elevationLimit+0.1 && tempx >= depressionLimit-0.1)
        {
            barrelRotation.Rotate(-Vector3.right, rotateAngle, Space.Self);
        }
        else
        {
            if(tempx > elevationLimit)
            {
                barrelRotation.localRotation = Quaternion.Euler(elevationLimit, 180f, 0f);
                EnemyWeaponController.trigger = false;
            }
            else if (tempx < depressionLimit)
            {
                barrelRotation.localRotation = Quaternion.Euler(depressionLimit, 180f, 0f);
                EnemyWeaponController.trigger = false;
            }
        }
    }

    private void RotateBase_V2()
    {
        Vector3 targetDirection = GerTargetDir(baseRotation);
        targetDirection.Normalize();

        Vector3 baseRelativeDirection = baseRotation.InverseTransformDirection(targetDirection);
        float baseTargetAngle = Mathf.Atan2(baseRelativeDirection.x, baseRelativeDirection.z) * Mathf.Rad2Deg;

        float angleToTarget = Mathf.DeltaAngle(baseRotation.eulerAngles.y, baseRotation.eulerAngles.y + baseTargetAngle);

        float angleThisFrame = Mathf.Sign(angleToTarget) * currentBaseSpeed * Time.deltaTime;
        if (Firing) angleThisFrame = Mathf.Sign(angleToTarget) * 10f * Time.deltaTime;
        if (Mathf.Abs(angleToTarget) < 5f) angleThisFrame = Mathf.Sign(angleToTarget) * 5f * Time.deltaTime;

        angleThisFrame = Mathf.Clamp(angleThisFrame, -Mathf.Abs(angleToTarget), Mathf.Abs(angleToTarget));

        currentBaseSpeed = Mathf.MoveTowards(currentBaseSpeed, maxBaseTurnSpeed, baseAcceleration * Time.deltaTime);

        baseRotation.Rotate(0f, angleThisFrame, 0f);
    }
    private void RotateBase_V3()
    {
        Vector3 targetDirection = GerTargetDir(baseRotation);
        targetDirection.Normalize();

        Vector3 baseRelativeDirection = baseRotation.InverseTransformDirection(targetDirection);
        float baseTargetAngle = Mathf.Atan2(baseRelativeDirection.x, baseRelativeDirection.z) * Mathf.Rad2Deg;

        float angleToTarget = Mathf.DeltaAngle(baseRotation.eulerAngles.y, baseRotation.eulerAngles.y + baseTargetAngle);

        float angleThisFrame = Mathf.Sign(angleToTarget) * currentBaseSpeed * Time.deltaTime;
        if (Firing) angleThisFrame = Mathf.Sign(angleToTarget) * 10f * Time.deltaTime;
        if (Mathf.Abs(angleToTarget) < 5f) angleThisFrame = Mathf.Sign(angleToTarget) * 5f * Time.deltaTime;

        angleThisFrame = Mathf.Clamp(angleThisFrame, -Mathf.Abs(angleToTarget), Mathf.Abs(angleToTarget));

        currentBaseSpeed = Mathf.MoveTowards(currentBaseSpeed, maxBaseTurnSpeed, baseAcceleration * Time.deltaTime);

        baseRotation.Rotate(0f, angleThisFrame, 0f);
    }
    //this one is totally running on bugs......
    private void RotateBarrel_V2()
    {
        Transform barrelRotation = barrelRotations[0];
        Vector3 targetDirection = GerTargetDir(barrelRotation);
        targetDirection.Normalize();

        Vector3 localTargetDirection = barrelRotation.InverseTransformDirection(targetDirection);

        float pitchAngle = Mathf.Atan2(localTargetDirection.y, localTargetDirection.z) * Mathf.Rad2Deg;

        pitchAngle = Mathf.Clamp(pitchAngle, depressionLimit, elevationLimit);

        Quaternion desiredRotation = Quaternion.Euler(pitchAngle, BarrelYoffset, BarrelZoffset);

        float desiredSpeed = maxBarrelTurnSpeed;

        currentBarrelSpeed = Mathf.MoveTowards(currentBarrelSpeed, desiredSpeed, barrelAcceleration * Time.deltaTime);
        if (Quaternion.Angle(barrelRotation.localRotation, desiredRotation) < 1f)
        {
            currentBarrelSpeed = 0;
        }
        barrelRotation.localRotation = Quaternion.RotateTowards(barrelRotation.localRotation, desiredRotation, currentBarrelSpeed * Time.deltaTime);
    }
    private void RotateBarrel_V3()
    {
        Transform barrelRotation = barrelRotations[0];
        Vector3 targetDirection = GerTargetDir(barrelRotation);
        targetDirection.Normalize();

        Vector3 barrelRelativeDirection = barrelRotation.InverseTransformDirection(targetDirection);
        float barrelTargetAngle = Mathf.Atan2(barrelRelativeDirection.y, barrelRelativeDirection.z) * Mathf.Rad2Deg;
        barrelTargetAngle = -RevertAngle(barrelTargetAngle);

        float angleToTarget = Mathf.DeltaAngle(barrelRotation.eulerAngles.x, barrelRotation.eulerAngles.x + barrelTargetAngle);

        float angleThisFrame = Mathf.Sign(angleToTarget) * currentBarrelSpeed * Time.deltaTime;
        if (Firing) angleThisFrame = Mathf.Sign(angleToTarget) * 5f * Time.deltaTime;
        if (Mathf.Abs(angleToTarget) < 1.5f) angleThisFrame = Mathf.Sign(angleToTarget) * 2.5f * Time.deltaTime;

        angleThisFrame = Mathf.Clamp(angleThisFrame, -Mathf.Abs(angleToTarget), Mathf.Abs(angleToTarget));
        currentBarrelSpeed = Mathf.MoveTowards(currentBarrelSpeed, maxBarrelTurnSpeed, barrelAcceleration * Time.deltaTime);
        barrelRotation.Rotate(angleThisFrame, 0f, 0f);

        //limit pitch
        float angle = barrelRotation.localEulerAngles.x;
        angle = (angle > 180) ? angle - 360 : angle;
        float newX = Mathf.Clamp(angle, depressionLimit, elevationLimit);
        Vector3 desiredRotation = new Vector3(newX, BarrelYoffset, BarrelZoffset);
        //barrelRotation.localEulerAngles = desiredRotation;

        foreach(Transform Rotation in barrelRotations)
        {
            if(Rotation) Rotation.localEulerAngles = desiredRotation;
        }
    }
    private void RotateBarrel_V4()
    {
        Transform barrelRotation = barrelRotations[0];
        Vector3 targetDirection = GerTargetDir(barrelRotation);
        targetDirection.Normalize();

        Vector3 barrelRelativeDirection = barrelRotation.InverseTransformDirection(targetDirection);
        float barrelTargetAngle = Mathf.Atan2(barrelRelativeDirection.y, barrelRelativeDirection.z) * Mathf.Rad2Deg;
        barrelTargetAngle = -RevertAngle(barrelTargetAngle);

        float angleToTarget = Mathf.DeltaAngle(barrelRotation.eulerAngles.x, barrelRotation.eulerAngles.x + barrelTargetAngle);

        float angleThisFrame = Mathf.Sign(angleToTarget) * currentBarrelSpeed * Time.deltaTime;
        if (Firing) angleThisFrame = Mathf.Sign(angleToTarget) * 5f * Time.deltaTime;
        if (Mathf.Abs(angleToTarget) < 1.5f) angleThisFrame = Mathf.Sign(angleToTarget) * 2.5f * Time.deltaTime;

        angleThisFrame = Mathf.Clamp(angleThisFrame, -Mathf.Abs(angleToTarget), Mathf.Abs(angleToTarget));
        currentBarrelSpeed = Mathf.MoveTowards(currentBarrelSpeed, maxBarrelTurnSpeed, barrelAcceleration * Time.deltaTime);
        barrelRotation.Rotate(angleThisFrame, 0f, 0f);

        //limit pitch
        float angle = barrelRotation.localEulerAngles.x;
        angle = (angle > 180) ? angle - 360 : angle;
        float newX = Mathf.Clamp(angle, depressionLimit, elevationLimit);
        Vector3 desiredRotation = new Vector3(newX, BarrelYoffset, BarrelZoffset);
        //barrelRotation.localEulerAngles = desiredRotation;

        foreach (Transform Rotation in barrelRotations)
        {
            if (Rotation) Rotation.localEulerAngles = desiredRotation;
        }
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
    float RevertAngle(float angle)
    {
        if (angle > 0 && angle <= 180) angle -= 180;
        else if (angle < 0 && angle >= -180) angle += 180;
        return angle;
    }

    private Vector3 GerTargetDir(Transform selfT)
    {
        Vector3 targetDir = target.position - selfT.position;
        Vector3 predictedTargetPosition = target.gameObject.GetComponent<Rigidbody>().velocity * (targetDir.magnitude / bulletSpeed);
        targetDir += predictedTargetPosition;
        return targetDir;
    }
}
