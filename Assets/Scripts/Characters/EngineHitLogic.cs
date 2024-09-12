using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EngineHitLogic : MonoBehaviour, IDamageable
{
    //public int EngineNumID;
    public int EngineHealth;
    private bool On_Destroy;
    private bool Engine_Burning;
    public int BurnDamageTick; 
    public float BurnDuration; 
    public float BurnInterval; 

    public GameObject[] Engine_Particles;
    public GameObject DestroyParticle;
    public GameObject BurnParticle;
    private AudioSource audioSource;
    public AudioClip DestroySound;
    public EnemyController enemyController;
    public ShipBotController shipmoveController;

    public void TakeDamage(int damage, bool passOn)
    {
        EngineHealth -= damage;
        if (passOn)
        {
            Pass_on_damage(damage);
            if (!Engine_Burning) StartCoroutine(Engine_Burn());
        }

        if (EngineHealth <= 0 && !On_Destroy)
        {
            On_Destroy = true;
            StartCoroutine(Engine_On_Destroy());
        }
    }
    void Start()
    {
        if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
        {
            EngineHealth = StaticGameDB.SARA_data.EngineHealth;
            BurnDamageTick = StaticGameDB.SARA_data.EngineBurnDamageTick;
            BurnDuration = StaticGameDB.SARA_data.EngineBurnDuration;
            BurnInterval = StaticGameDB.SARA_data.EngineBurnInterval;
        }
        else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
        {
            EngineHealth = StaticGameDB.MAGE_data.EngineHealth;
            BurnDamageTick = StaticGameDB.MAGE_data.EngineBurnDamageTick;
            BurnDuration = StaticGameDB.MAGE_data.EngineBurnDuration;
            BurnInterval = StaticGameDB.MAGE_data.EngineBurnInterval;
        }
        Engine_Burning = false;

        audioSource = gameObject.GetComponentInParent<AudioSource>();
        enemyController = gameObject.GetComponentInParent<EnemyController>();
        shipmoveController = gameObject.GetComponentInParent<ShipBotController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Engine_On_Destroy()
    {
        foreach (GameObject particle in Engine_Particles)
        {
            particle.GetComponent<ParticleSystem>().Stop();
        }
        yield return new WaitForSeconds(1);
        DestroyParticle.GetComponent<ParticleSystem>().Play();
        audioSource.PlayOneShot(DestroySound, 2f);
        yield return new WaitForSeconds(1);
        //StartCoroutine(Engine_Burn());
        yield return new WaitForSeconds(BurnDuration);
        DestroyParticle.GetComponent<ParticleSystem>().Stop();
        BurnParticle.GetComponent<ParticleSystem>().Stop();
        Destroy(gameObject);
    }
    IEnumerator Engine_Burn()
    {
        Engine_Burning = true;
        BurnParticle.GetComponent<ParticleSystem>().Play();

        float elapsed = 0f;

        while (elapsed < BurnDuration)
        {
            elapsed += BurnInterval;
            yield return new WaitForSeconds(BurnInterval);
            EngineHealth -= BurnDamageTick;
            Pass_on_damage(BurnDamageTick);
            if (EngineHealth <= 0 && !On_Destroy)
            {
                On_Destroy = true;
                StartCoroutine(Engine_On_Destroy());
            }
        }
        BurnParticle.GetComponent<ParticleSystem>().Stop();
    }
    private void Pass_on_damage()
    {
        if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
        {
            enemyController.Damage_transfer(StaticGameDB.SARA_data.FuelExplodeDMG/2);
        }
        //cut off engine
        shipmoveController.target = null;
    }
    private void Pass_on_damage(int dmg)
    {
        if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
        {
            enemyController.Damage_transfer(dmg);
        }
        else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
        {
            enemyController.Damage_transfer(dmg);
        }
    }

    public void DestroyImmediately()
    {

    }
}
