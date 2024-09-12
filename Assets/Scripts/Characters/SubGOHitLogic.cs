// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SubGOHitLogic : MonoBehaviour, IDamageable
{
    public int SubGOHealth;
    private bool On_Destroy;
    public ParticleSystem HitEffect_Particle;
    public ParticleSystem HitEffectL_Particle;
    public GameObject DestroyParticle;

    private AudioSource audioSource;
    public List<AudioClip> DestroySounds;

    public TurretLogic TurretFather;
    public EnemyWeaponController EWPC;
    public EnemyController enemyController;

    public void TakeDamage(int damage, bool passOn)
    {
        SubGOHealth -= damage;
        if (gameObject.tag == "TurretBase")
            if (passOn) Pass_on_damage(damage);
        if (SubGOHealth <= 0 && !On_Destroy)
        {
            On_Destroy = true;
            if (gameObject.tag == "TurretBase")
            {
                StartCoroutine(Sub_On_Destroy());
            }
            else if (gameObject.tag == "TurretBarrel")
            {
                StartCoroutine(Destroy_Barrel());
            }
        }
    }
    public void DestroyImmediately()
    {
        SubGOHealth = 0;
        if (!On_Destroy)
        {
            On_Destroy = true;
            if (gameObject.tag == "TurretBase")
            {
                StartCoroutine(Sub_On_Destroy());
            }
            else if (gameObject.tag == "TurretBarrel")
            {
                StartCoroutine(Destroy_Barrel());
            }
        }
    }

    void Start()
    {
        //extend modules/ships
        if (gameObject.tag == "TurretBarrel")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                SubGOHealth = StaticGameDB.SARA_data.TurretBaseHealth;
            }
            else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
            {
                SubGOHealth = StaticGameDB.MAGE_data.TurretBaseHealth;
            }
        }
        else if (gameObject.tag == "TurretBarrel")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                SubGOHealth = StaticGameDB.SARA_data.TurretBarrelHealth;
            }
            else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
            {
                SubGOHealth = StaticGameDB.MAGE_data.TurretBarrelHealth;
            }
        }

        audioSource = gameObject.GetComponentInParent<AudioSource>();
        On_Destroy = false;
        enemyController = gameObject.GetComponentInParent<EnemyController>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //abandon
    public void SubCollisionEnter(Collision collision)
    {
        if (gameObject.tag == "TurretBase")
            Damage_Type_Base(collision);
        else if (gameObject.tag == "TurretBarrel")
            Damage_Type_Barrel(collision);
    }

    private void Damage_Type_Base(Collider collision)
    {
        if (collision.gameObject.tag == "Projectile")
        {
            SubGOHealth--;

            //display hit_Particle
            //Projectile_On_Sub(collision);
            Destroy(collision.gameObject);

            if (SubGOHealth <= 0 && !On_Destroy)
            {
                On_Destroy = true;
                StartCoroutine(Sub_On_Destroy());
            }
        }
    }
    private void Damage_Type_Barrel(Collider collision)
    {
        if (collision.gameObject.tag == "Projectile")
        {
            SubGOHealth--;

            //display hit_Particle
            //Projectile_On_Sub(collision);

            if (SubGOHealth <= 0 && !On_Destroy)
            {
                On_Destroy = true;
                StartCoroutine(Destroy_Barrel());
            }
        }
    }
    private void Damage_Type_Base(Collision collision)
    {
        //ram dmg
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "ship")
        {
            SubGOHealth -= (int)(CalculateRamDamage(collision)*0.5f);
            if (SubGOHealth <= 0 && !On_Destroy)
            {
                On_Destroy = true;
                StartCoroutine(Sub_On_Destroy());
            }
        }
    }
    private void Damage_Type_Barrel(Collision collision)
    {
        //ram dmg
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "ship")
        {
            SubGOHealth -= (int)(CalculateRamDamage(collision)*1.5f);
            if (SubGOHealth <= 0 && !On_Destroy)
            {
                On_Destroy = true;
                StartCoroutine(Destroy_Barrel());
            }
        }
    }
    private float CalculateRamDamage(Collision collision)
    {
        Rigidbody otherRigidbody = collision.rigidbody;
        if (otherRigidbody == null) return 0;

        float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;

        // set limit
        float minVelocity = StaticGameDB.ram_dmg_data.minVelocity; 
        float maxDamage = StaticGameDB.ram_dmg_data.maxDamageModule;
        float ramFactor = StaticGameDB.ram_dmg_data.ramFactor;

        float kineticEnergy = ramFactor * otherRigidbody.mass * relativeVelocityMagnitude;

        if (relativeVelocityMagnitude < minVelocity) return 0;

        float damage = Mathf.Min(kineticEnergy, maxDamage);

        return damage;
    }

    private void Projectile_On_Sub(Collision collision)
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
            Debug.Log("exp!");
            ContactPoint contact = collision.GetContact(0);
            Vector3 hitPoint = contact.point;
            Quaternion hitRotation = Quaternion.LookRotation(contact.normal);

            RaycastHit hit;
            if (Physics.Raycast(hitPoint + contact.normal * 0.1f, -contact.normal, out hit, 0.2f))
            {
                hitPoint = hit.point;
                hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            audioSource.PlayOneShot(DestroySounds[1], 2f);
            Destroy(Instantiate(HitEffectL_Particle, hitPoint, hitRotation).gameObject, 10f);
        }
    }
    private void Projectile_On_Sub(Collider other)//useless
    {
        if (other.gameObject.CompareTag("Projectile"))
        {
            Projectile_Behavior projectileBehavior = other.GetComponent<Projectile_Behavior>();
            if (projectileBehavior != null)
            {
                Vector3 hitPoint = projectileBehavior.last_position;
                Quaternion hitRotation = Quaternion.LookRotation(projectileBehavior.current_speed);

                RaycastHit hit;
                if (Physics.Raycast(hitPoint + projectileBehavior.current_speed.normalized * 0.1f, -projectileBehavior.current_speed.normalized, out hit, 0.2f))
                {
                    hitPoint = hit.point;
                    hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }

                Destroy(Instantiate(HitEffect_Particle, hitPoint, hitRotation).gameObject, 10f);
            }
        }
    }

    IEnumerator Sub_On_Destroy()
    {
        TurretFather.shipDestroyed = true;
        EWPC.trigger = false;

        DestroyParticle.GetComponent<ParticleSystem>().Play();
        audioSource.PlayOneShot(DestroySounds[0], 2f);

        yield return new WaitForSeconds(5);
        Pass_on_damage();
        DestroyParticle.GetComponent<ParticleSystem>().Stop();

        Vector3 pos = TurretFather.gameObject.transform.position;
        Quaternion rot = TurretFather.gameObject.transform.rotation;
        Destroy(Instantiate(HitEffectL_Particle, pos, rot).gameObject, 10f);
        audioSource.PlayOneShot(DestroySounds[1], 2f);

        Temp_Add_RigidBody();
        Vector3 explosionPosition = transform.position + transform.up * -1.5f;
        gameObject.GetComponent<Rigidbody>().AddExplosionForce(350, explosionPosition, 10);
        StartCoroutine(Destroy_Sub());
    }
    IEnumerator Destroy_Sub()
    {
        yield return new WaitForSeconds(10);
        Destroy(TurretFather.gameObject);
    }
    IEnumerator Destroy_Barrel()
    {
        TurretFather.shipDestroyed = true;
        EWPC.trigger = false;
        Temp_Add_RigidBody();
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
    private void Pass_on_damage()
    {
        if (gameObject.tag == "TurretBase")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                enemyController.Damage_transfer(StaticGameDB.SARA_data.AmmunitionExplodeDMG/3);
            }
            else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
            {
                enemyController.Damage_transfer(StaticGameDB.SARA_data.AmmunitionExplodeDMG/2);
            }
        }
    }
    private void Pass_on_damage(int dmg)
    {
        if (gameObject.tag == "TurretBase")
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

    private void Temp_Add_RigidBody()
    {
        gameObject.AddComponent<Rigidbody>();
        //extend turret mass
        if (gameObject.tag == "TurretBarrel")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                gameObject.GetComponent<Rigidbody>().mass = StaticGameDB.SARA_data.TurretBaseMass;
            }
            else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
            {
                gameObject.GetComponent<Rigidbody>().mass = StaticGameDB.MAGE_data.TurretBaseMass;
            }
        }
        else if (gameObject.tag == "TurretBarrel")
        {
            if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
            {
                gameObject.GetComponent<Rigidbody>().mass = StaticGameDB.SARA_data.TurretBarrelMass;
            }
            else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
            {
                gameObject.GetComponent<Rigidbody>().mass = StaticGameDB.MAGE_data.TurretBarrelMass;
            }
        }
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().velocity = GetComponentInParent<Rigidbody>().velocity;
    }
}
