using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public string ShipClass;
    private int masterFactor;
    public int Health;
    public int Armor;

    private bool On_Destroy;

    public GameObject[] On_Destory_StartParticles;
    public GameObject[] On_DestoryParticles;
    public GameObject[] On_Destory_EndParticles;
    public GameObject[] Engine_Particles;
    private SubGOHitLogic[] SubGOs;

    public ParticleSystem HitEffect_Particle;
    public ParticleSystem HitEffectL_Particle;

    private AudioSource audioSource;
    public AudioClip Explosion_L;
    public AudioClip Explosion_L_Burn;
    public AudioClip Explosion_LL;
    public AudioClip EXP_SMOKE;

    public void TakeDamage(int damage, bool passOn)
    {
        Health -= damage;
        if (Health <= 0 && !On_Destroy)
        {
            On_Destroy = true;
            StartCoroutine(Ship_On_Destroy());
        }
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("MasterMode") == 1)
        {
            masterFactor = 2;
        }
        else
        {
            masterFactor = 1;
        }

        //extend others
        if (ShipClass == "SARA")
        {
            Health = StaticGameDB.SARA_data.BaseHealth / masterFactor;
            Armor = StaticGameDB.SARA_data.BaseArmor;
        }
        else if (ShipClass == "MAGE")
        {
            Health = StaticGameDB.MAGE_data.BaseHealth / masterFactor;
            Armor = StaticGameDB.MAGE_data.BaseArmor;
        }

        audioSource = gameObject.GetComponent<AudioSource>();

        SubGOs = GetComponentsInChildren<SubGOHitLogic>();

        On_Destroy = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        bool isChildInvolved = false;
        
        foreach (var child in SubGOs)
        {
            if (child != null && collision.contacts[0].thisCollider == child.GetComponent<Collider>())
            {
                isChildInvolved = true;
                child.SubCollisionEnter(collision);
                break; 
            }
        }
        
        if (!isChildInvolved)
        {
            //extra dmg for penetration
            if (collision.gameObject.tag == "Rocket")
            {
                //display hit_Particle
                //Projectile_On_Ship(collision);
                Penetration_damage(collision);
                Destroy(collision.gameObject);

                if (Health <= 0 && !On_Destroy)
                {
                    On_Destroy = true;
                    StartCoroutine(Ship_On_Destroy());
                }
            }
            //ram dmg
            if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "ship")
            {
                Health -= (int)(CalculateRamDamage(collision) * 0.5f);
                if (Health <= 0 && !On_Destroy)
                {
                    On_Destroy = true;
                    StartCoroutine(Ship_On_Destroy());
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Damage_transfer(int dmg)
    {
        Health -= dmg;
        if (Health <= 0 && !On_Destroy)
        {
            On_Destroy = true;
            StartCoroutine(Ship_On_Destroy());
        }
    }

    private void Penetration_damage(Collision collision)
    {
        Vector3 velocityVector = collision.gameObject.GetComponent<Projectile_Behavior>().current_speed;
        ContactPoint contact = collision.GetContact(0);
        float angle = Vector3.Angle(velocityVector, -contact.normal);
        float cosine = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));

        float basePenetration = collision.gameObject.GetComponent<Projectile_Behavior>().basePenetration;
        float initialVelocity = collision.gameObject.GetComponent<Projectile_Behavior>().init_speed;

        float currentPenetration;
        if (collision.gameObject.tag == "Rocket")
        {
            currentPenetration = basePenetration;
        }
        else
        {
            currentPenetration = PenetrationAttenuation(basePenetration, initialVelocity, velocityVector.magnitude);
        } 
        float effectivePenetration = currentPenetration * cosine;

        //Debug.Log(effectivePenetration);
        if (effectivePenetration > Armor)
        {
            Health -= (int)(effectivePenetration * effectivePenetration / basePenetration);
            GameObject afterEffect = collision.gameObject.GetComponent<Projectile_Behavior>().afterEffect;
            afterEffect = Instantiate(afterEffect, collision.transform.position, collision.transform.rotation);
            if (collision.gameObject.tag == "Rocket")
            {
                Vector3 AEVelocity = velocityVector.normalized * (velocityVector.magnitude * ((effectivePenetration - Armor) / Armor) + StaticGameDB.nuclear_rocket_data.MetalJetMuzzleVelocity);
                afterEffect.GetComponent<Rigidbody>().velocity = AEVelocity;
                afterEffect.GetComponent<AfterEffect_Behavior>().Init_fromparent("HEAT", (effectivePenetration - Armor) / Armor);
            }
        }
        else  //not penetrate
        {
            Health -= (int)(effectivePenetration * effectivePenetration / basePenetration);
        }
    }
    private float PenetrationAttenuation(float basePenetration, float initialVelocity, float currentVelocity)
    {
        float penetrationRatio = Mathf.Pow(currentVelocity / initialVelocity, 2);
        float currentPenetration = basePenetration * penetrationRatio;

        return currentPenetration;
    }
    private float CalculateRamDamage(Collision collision)
    {
        Rigidbody otherRigidbody = collision.rigidbody;
        if (otherRigidbody == null) return 0;

        float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;

        // set limit
        float minVelocity = StaticGameDB.ram_dmg_data.minVelocity;
        float maxDamage = StaticGameDB.ram_dmg_data.maxDamageShip;
        float ramFactor = StaticGameDB.ram_dmg_data.ramFactor;

        float kineticEnergy = ramFactor * otherRigidbody.mass * relativeVelocityMagnitude;

        if (relativeVelocityMagnitude < minVelocity) return 0;

        float damage = Mathf.Min(kineticEnergy, maxDamage);

        return damage;
    }
    private void Projectile_On_Ship(Collision collision)
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
        if (collision.gameObject.CompareTag("Rocket"))
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
            audioSource.PlayOneShot(Explosion_L_Burn, 2f);
            Destroy(Instantiate(HitEffectL_Particle, hitPoint, hitRotation).gameObject, 10f);
        }
    }
    IEnumerator Ship_On_Destroy()
    {
        foreach (GameObject particle in On_Destory_StartParticles)
        {
            particle.GetComponent<ParticleSystem>().Play();
            
        }
        audioSource.PlayOneShot(Explosion_L, 2f);
        yield return new WaitForSeconds(5);
        StartCoroutine(Ship_Destroying());

        TurretLogic[] turrets = GetComponentsInChildren<TurretLogic>();
        foreach (TurretLogic turret in turrets)
        {
            turret.shipDestroyed = true;
        }
        EnemyWeaponController[] EWPCs = GetComponentsInChildren<EnemyWeaponController>();
        foreach (EnemyWeaponController EWPC in EWPCs)
        {
            EWPC.trigger = false;
        }
    }
    IEnumerator Ship_Destroying()
    {
        foreach (GameObject particle in Engine_Particles)
        {
            particle.GetComponent<ParticleSystem>().Stop();

        }
        foreach (GameObject particle in On_Destory_StartParticles)
        {
            particle.GetComponent<ParticleSystem>().Stop();

        }
        foreach (GameObject particle in On_DestoryParticles)
        {
            particle.GetComponent<ParticleSystem>().Play();

        }
        audioSource.PlayOneShot(Explosion_L_Burn, 2f);
        audioSource.PlayOneShot(EXP_SMOKE, 2f); 
        yield return new WaitForSeconds(10);
        StartCoroutine(Ship_Destroy_End());
    }
    IEnumerator Ship_Destroy_End()
    {
        foreach (GameObject particle in On_DestoryParticles)
        {
            particle.GetComponent<ParticleSystem>().Stop();

        }
        foreach (GameObject particle in On_Destory_EndParticles)
        {
            particle.GetComponent<ParticleSystem>().Play();

        }
        audioSource.PlayOneShot(Explosion_LL, 2.5f);
        yield return new WaitForSeconds(10);
        foreach (GameObject particle in On_Destory_EndParticles)
        {
            particle.GetComponent<ParticleSystem>().Stop();

        }
        Destroy(gameObject);
    }


    public void DestroyImmediately()
    {
        //throw new System.NotImplementedException();
    }
}
