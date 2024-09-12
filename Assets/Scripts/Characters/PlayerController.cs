using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable
{
    private int screenWidth;
    private int screenHeight;

    private float mass;
    public float maxspeed_setting;//how fastest you can move physically
    public float maxAngularVelocity_setting;//how fastest you can rotate physically
    public float max_allow_AngularVelocity;//how fast you are allowed to rotate in MS
    public float turningTime;//setting MS designed 180 degree turning time
    private float bursttorque;// calculate how much torque you need for turningTime
    private float bursttorque_cache;
    private float inverttorque;//cache for invert torque
    public float maxsacceletation;//how fast you are allowed to acceletate in MS /G value
    public float threeXspeed;
    public float maxsacceletation_cache;
    public float breakacceletation;//setting disacceletate G value
    public float Joystick_deadzone;
    public float smoothMouse; //multi mouthsense faster you are in speed 
    private float horAxis;
    private float verAxis;

    //player_states
    private int playerHealth;
    private int playerMaxHealth;
    private int dmgFactor = 1;
    private int dmgFactor_cache;

    private Vector3 previousVelocity;

    private bool acceletating;
    private bool isRotating;
    private bool ABS;
    private bool On_Destroy;
    private bool slowmotion;

    public GameObject HeadLight;
    public GameObject On_Destory_StartParticles;
    public ParticleSystem HitEffect_Particle;
    public AudioClip Explosion_LL;
    public LockEnemy lockEnemy;
    public GameObject crosshairmouse;
    public MouseSensitivityManager MouseSensitivity;
    public EngineSoundController engineSoundController;
    public Scrollbar HealthBar;
    public List<Text> Debug_text;

    public void TakeDamage(int damage,bool passOn)
    {
        playerHealth -= dmgFactor * damage;
        deathCheck();
    }
    void Start()
    {
        Debug.Log(PlayerPrefs.GetInt("MasterMode"));
        if (PlayerPrefs.GetInt("MasterMode") == 1)
        {
            playerHealth = 1000;
            playerMaxHealth = playerHealth;
            threeXspeed = maxsacceletation * 3;
            maxspeed_setting = 950;
            dmgFactor = 2;
            dmgFactor_cache = dmgFactor;
        }
        else
        {
            playerHealth = 2000;
            playerMaxHealth = playerHealth;
            maxsacceletation = 5;
            threeXspeed = maxsacceletation * 5;
            maxspeed_setting = 400;
            dmgFactor_cache = dmgFactor;
        }

        mass = GetComponent<Rigidbody>().mass;
        GetComponent<Rigidbody>().maxAngularVelocity = maxAngularVelocity_setting;
        GetComponent<Rigidbody>().maxLinearVelocity = maxspeed_setting;

        float momentOfInertia = GetComponent<Rigidbody>().inertiaTensor.magnitude;
        bursttorque = 180f / turningTime * Mathf.Deg2Rad * momentOfInertia * MouseSensitivity.GetSensitivity();

        screenWidth = Screen.width;
        screenHeight = Screen.height;

        ABS = false;
        On_Destroy = false;
        slowmotion = false;

        bursttorque_cache = bursttorque;
        maxsacceletation_cache = maxsacceletation;

        previousVelocity = GetComponent<Rigidbody>().velocity;
    }

    // Update is called once per frame
    void Update()
    {
        Cheat();
        Switch_HeadLight();
        Toggle_LockMode();
        HealthBar.size = Mathf.Clamp((float)playerHealth / playerMaxHealth, 0,1);
    }
    void FixedUpdate()
    {
        isRotating = false;
        acceletating = false;
        if (!On_Destroy)
        {
            apply_movement();
            apply_rotate();
        }

    }

    /*private void OnTriggerEnter(Collider trigger)
    {
        int dmg = trigger.gameObject.GetComponent<Beam_Behavior>().baseDamage;
        if (trigger.gameObject.tag == "Projectile")
        {
            playerHealth -= dmgFactor * dmg;
            deathCheck();
        }
    }*/
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ship")
        {
            playerHealth -= (int)(CalculateRamDamage(collision) * dmgFactor);
            deathCheck();
        }
    }
    private float CalculateRamDamage(Collision collision)
    {
        Rigidbody otherRigidbody = collision.rigidbody;
        if (otherRigidbody == null) return 0;

        float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;

        // set limit
        float minVelocity = StaticGameDB.ram_dmg_data.minVelocityMS;
        float maxDamage = StaticGameDB.ram_dmg_data.maxDamageMS;
        float ramFactor = StaticGameDB.ram_dmg_data.ramFactor;

        float kineticEnergy = ramFactor * otherRigidbody.mass * relativeVelocityMagnitude;

        if (relativeVelocityMagnitude < minVelocity) return 0;

        float damage = Mathf.Min(kineticEnergy, maxDamage);

        return damage;
    }
    private void deathCheck()
    {
        if (playerHealth <= 0 && !On_Destroy)
        {
            Debug.Log("Killed!");
            On_Destroy = true;
            StartCoroutine(MS_On_Destroy());
        }
    }
    private void Projectile_On_MS(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            ContactPoint contact = collision.GetContact(0);
            Vector3 hitPoint = contact.point;
            Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

            RaycastHit hit;
            if (Physics.Raycast(hitPoint + contact.normal * 0.1f, -contact.normal, out hit, 0.2f))
            {
                hitPoint = hit.point;
                hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            Destroy(Instantiate(HitEffect_Particle, hitPoint, hitRotation).gameObject, 10f);
        }
    }
    IEnumerator MS_On_Destroy()
    {
        engineSoundController.shutEngine();
        On_Destory_StartParticles.GetComponent<ParticleSystem>().Play();
        GetComponent<AudioSource>().PlayOneShot(Explosion_LL, 1f);
        yield return new WaitForSeconds(3.5f);
    }

    private void apply_movement()
    {
        Vector3 currentVelocity = GetComponent<Rigidbody>().velocity;
        Vector3 acceleration = (currentVelocity - previousVelocity) / Time.fixedDeltaTime;

        //UI
        Debug_text[0].text = currentVelocity.magnitude.ToString()+ " m/s";
        Debug_text[1].text = (acceleration.magnitude/9.8f).ToString() + " g";
        if (lockEnemy.lock_mode)
        {
            Debug_text[2].text = "LockTarget: " + lockEnemy.Get_LockDis().ToString() + " m";
        }
        else
        {
            Debug_text[2].text = "UnLock";
        }

        previousVelocity = currentVelocity;
        float power = acceleration.magnitude;
        engineSoundController.SetPower(power);
        engineSoundController.SetBurst(false);

        //3*speed
        maxsacceletation = maxsacceletation_cache;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            maxsacceletation = threeXspeed;
            if (currentVelocity.magnitude > 800) { engineSoundController.SetBurst(false); }
            else
            {
                engineSoundController.SetBurst(true);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            maxsacceletation = threeXspeed;
            if (currentVelocity.magnitude > 800) { engineSoundController.SetBurst(false); }
            else
            {
                engineSoundController.SetBurst(true);
            }
        }
        //movement
        if (Input.GetKey(KeyCode.W))
        {
            acceletating = true;
            GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().transform.forward * mass * maxsacceletation * 9.81f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            acceletating = true;
            GetComponent<Rigidbody>().AddForce(-GetComponent<Rigidbody>().transform.forward * mass * maxsacceletation * 9.81f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            acceletating = true;
            GetComponent<Rigidbody>().AddForce(-GetComponent<Rigidbody>().transform.right * mass * maxsacceletation * 9.81f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            acceletating = true;
            GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().transform.right * mass * maxsacceletation * 9.81f);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            acceletating = true;
            GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().transform.up * mass * maxsacceletation * 9.81f);
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            acceletating = true;
            GetComponent<Rigidbody>().AddForce(-GetComponent<Rigidbody>().transform.up * mass * maxsacceletation * 9.81f);
        }
        if (!acceletating)//slow down
        {
            Vector3 antiforce = -GetComponent<Rigidbody>().velocity.normalized * mass * breakacceletation * 9.81f;
            GetComponent<Rigidbody>().AddForce(antiforce);
        }
    }
    private void apply_rotate()
    {
        bursttorque = bursttorque_cache * MapValue_Mouse(GetComponent<Rigidbody>().velocity.magnitude);
        bursttorque *= MouseSensitivity.GetSensitivity();
        if (lockEnemy.lock_mode) { bursttorque *= StaticGameDB.Player_data.ESP_factor; }

        //rotate
        if (Input.GetKey(KeyCode.Q) && !ABS)
        {
            isRotating = true;
            GetComponent<Rigidbody>().AddTorque(transform.forward * bursttorque);
            ABS = !ABS;
        }
        if (Input.GetKey(KeyCode.E) && !ABS)
        {
            isRotating = true;
            GetComponent<Rigidbody>().AddTorque(-transform.forward * bursttorque);
            ABS = !ABS;
        }
        if (ABS) ABS = !ABS;
        if (!isRotating)
        {
            Vector3 localAngularVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().angularVelocity);
            Vector3 localAntiTorque = new Vector3(-localAngularVelocity.x, 0, -localAngularVelocity.z) * bursttorque;
            GetComponent<Rigidbody>().AddTorque(transform.TransformDirection(localAntiTorque));
        }
        apply_joystick();
    }
    private Vector3 get_transfer_mouse()
    {
        Vector2 mousePos = crosshairmouse.GetComponent<CustomCursor>().get_fixed_mouse_pos();
        Vector2 screenCenter = new Vector3(screenWidth / 2, screenHeight / 2, 0);
        Vector2 joystickPos = (mousePos - screenCenter) / crosshairmouse.GetComponent<CustomCursor>().joystickradius;

        //narrow in x
        joystickPos.x *= 0.5f;
        return joystickPos;
    }
    private void apply_joystick()
    {
        Vector3 joystickPos = get_transfer_mouse();
        if (joystickPos.magnitude > Joystick_deadzone && !ABS)
        {
            if (GetComponent<Rigidbody>().angularVelocity.magnitude <= max_allow_AngularVelocity)
            {
                GetComponent<Rigidbody>().AddTorque(-transform.right * bursttorque * joystickPos.y);
                GetComponent<Rigidbody>().AddTorque(transform.up * bursttorque * joystickPos.x);
            }
            else
            {
                slowdown_Mouse();
            }
        }
        else//slow down
        {
            slowdown_Mouse();
        }
    }
    float MapValue_Mouse(float originalValue)
    {
        float scale_factor = smoothMouse;
        float max_original_value = maxspeed_setting;
        return 1.0f - scale_factor * (originalValue / max_original_value);
    }

    private void slowdown_Mouse()
    {
        if (!isRotating)
        {
            if (GetComponent<Rigidbody>().angularVelocity.magnitude < 0.35)
            {
                inverttorque = Mathf.Lerp(inverttorque, bursttorque / mass, 0.05f);
                Vector3 localAngularVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().angularVelocity);
                Vector3 localAntiTorque = new Vector3(-localAngularVelocity.x, -localAngularVelocity.y, 0).normalized * inverttorque;
                GetComponent<Rigidbody>().AddTorque(transform.TransformDirection(localAntiTorque));
                //Vector3 antitorque = -GetComponent<Rigidbody>().angularVelocity.normalized * invertangular;
                //GetComponent<Rigidbody>().AddTorque(antitorque);
            }
            else
            {
                inverttorque = bursttorque;
                Vector3 localAngularVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().angularVelocity);
                Vector3 localAntiTorque = new Vector3(-localAngularVelocity.x, -localAngularVelocity.y, 0).normalized * inverttorque;
                GetComponent<Rigidbody>().AddTorque(transform.TransformDirection(localAntiTorque));
                //Vector3 antitorque = -GetComponent<Rigidbody>().angularVelocity.normalized * invertangular;
                //GetComponent<Rigidbody>().AddTorque(antitorque);
            }
        }
    }
    private bool IsRolling()
    {
        Vector3 localAngularVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().angularVelocity);
        if (Mathf.Approximately(localAngularVelocity.x, 0) && Mathf.Approximately(localAngularVelocity.z, 0))
        {
            return false; 
        }
        else
        {
            return true;
        }
    }
    private void Switch_HeadLight()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            bool lightEnabled = HeadLight.GetComponent<Light>().enabled;
            HeadLight.GetComponent<Light>().enabled = !lightEnabled;
        }
    }
    private void Toggle_LockMode()
    {
        if ((Input.GetKeyDown(KeyCode.T) || Input.GetMouseButtonDown(3)) && PlayerPrefs.GetInt("MasterMode") != 1)
        {
            lockEnemy.LockActivate();
        }
        if (Input.GetKey(KeyCode.LeftAlt) && (Input.GetKeyDown(KeyCode.T) || Input.GetMouseButtonDown(3)) && PlayerPrefs.GetInt("MasterMode") != 1)
        {
            lockEnemy.LockDeactivate();
        }
    }

    private void Toggle_SlowMotion()
    {
        if (slowmotion)
        {
            Time.timeScale = 0.25f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    //CHEAT!
    private void Cheat()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)){
            if (dmgFactor == 0) dmgFactor = dmgFactor_cache;
            else dmgFactor = 0;
        }
        if (Input.GetMouseButtonDown(4)){
            slowmotion = !slowmotion;
            Toggle_SlowMotion();
        }
    }
    public int checkPlayerHealth()
    {
        return playerHealth;
    }

    public void DestroyImmediately()
    {
        //throw new System.NotImplementedException();
    }
}
