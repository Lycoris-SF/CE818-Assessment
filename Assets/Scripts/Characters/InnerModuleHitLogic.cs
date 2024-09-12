using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InnerModuleHitLogic : MonoBehaviour, IDamageable
{
    public string ModuleType;
    public int ModuleHealth;
    public int DynamicModuleDmg;
    private bool On_Destroy; 

    public List<TurretLogic> TurretGroup;
    public List<EnemyWeaponController> EWPC_Group;
    public EnemyController enemyController;
    public ShipBotController shipmoveController;

    public GameObject DestroyParticle;
    private AudioSource audioSource;
    public AudioClip DestroySound;

    public void TakeDamage(int damage, bool passOn)
    {
        ModuleHealth -= damage;
        if (passOn) Pass_on_damage(damage);
        if (ModuleHealth <= 0 && !On_Destroy)
        {
            On_Destroy = true;
            StartCoroutine(Module_On_Destroy());
        }
    }
    void Start()
    {   //extend modules/ships
        if (ModuleType == "AmmoRack")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                ModuleHealth = StaticGameDB.SARA_data.AmmoRacklHealth;
            }
        }
        else if (ModuleType == "FuelTank")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                ModuleHealth = StaticGameDB.SARA_data.FuelTankHealth;
            }
        }

        audioSource = gameObject.GetComponentInParent<AudioSource>();
        enemyController = gameObject.GetComponentInParent<EnemyController>();
        shipmoveController = gameObject.GetComponentInParent<ShipBotController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Module_On_Destroy()
    {
        for(int i=0;i< TurretGroup.Capacity;i++)
        {
            TurretGroup[i].shipDestroyed = true;
            EWPC_Group[i].trigger = false;
            if (TurretGroup[i])
            {
                SubGOHitLogic[] subs = TurretGroup[i].gameObject.GetComponentsInChildren<SubGOHitLogic>();
                foreach (SubGOHitLogic sub in subs)
                {
                    if (sub && sub.gameObject.tag == "TurretBase")
                    {
                        IDamageable damageable = sub.GetComponent<IDamageable>();
                        damageable.DestroyImmediately();
                    }
                }
            }
        }

        yield return new WaitForSeconds(1);
        DestroyParticle.GetComponent<ParticleSystem>().Play();
        audioSource.PlayOneShot(DestroySound, 2f);
        Pass_on_damage();
        yield return new WaitForSeconds(5);
        DestroyParticle.GetComponent<ParticleSystem>().Stop();
        Destroy(gameObject);
    }

    private void Pass_on_damage()
    {
        if (ModuleType == "AmmoRack")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                enemyController.Damage_transfer(StaticGameDB.SARA_data.AmmunitionExplodeDMG);
            }
            else
            {
                enemyController.Damage_transfer(DynamicModuleDmg);
            }
        }
        else if (ModuleType == "FuelTank")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                enemyController.Damage_transfer(StaticGameDB.SARA_data.FuelExplodeDMG);
            }
            else
            {
                enemyController.Damage_transfer(DynamicModuleDmg);
            }
            //cut off engine
            shipmoveController.target = null;
        }
    }
    private void Pass_on_damage(int dmg)
    {
        if (ModuleType == "AmmoRack")
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
        else if (ModuleType == "FuelTank")
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
    }

    public void DestroyImmediately()
    {
        
    }
}
