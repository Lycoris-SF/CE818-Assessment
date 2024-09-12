using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Projectile_Behavior : MonoBehaviour
{
    public float init_speed;
    public float basePenetration;
    public float explosionRadius;
    private float lifetime;
    public int baseDamage;
    private int ricochetAngle;
    public Vector3 current_speed;
    public Vector3 last_position;

    public GameObject afterEffect;

    public ParticleSystem HitEffect_Particle;
    public AudioMixer mainMixer;
    public AudioClip Explosion;

    void Start()
    {
        //use static data
        if (gameObject.tag == "Rocket")
        {
            baseDamage = StaticGameDB.nuclear_rocket_data.RocketDamage;
            basePenetration = StaticGameDB.nuclear_rocket_data.RocketBasePenetration;
            explosionRadius = StaticGameDB.nuclear_rocket_data.RocketExplosionRadius;
            init_speed = StaticGameDB.nuclear_rocket_data.RocketMuzzleVelocity;
            lifetime = StaticGameDB.nuclear_rocket_data.Rocketlifetime;
        }
        else if (gameObject.tag == "Projectile")
        {
            baseDamage = StaticGameDB.bullet_machinegun_data.BulletDamage;
            basePenetration = StaticGameDB.bullet_machinegun_data.BulletBasePenetration;
            explosionRadius = 0;
            init_speed = StaticGameDB.bullet_machinegun_data.BulletMuzzleVelocity;
            ricochetAngle = StaticGameDB.bullet_machinegun_data.RicochetAngle;
            lifetime = StaticGameDB.bullet_machinegun_data.Bulletlifetime;
        }
        GetComponent<Rigidbody>().velocity += transform.forward * init_speed;
        Destroy(gameObject, lifetime);
    }
    public void Init_Speed_fromparent(Vector3 parent_velocity)
    {
        GetComponent<Rigidbody>().velocity += parent_velocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        current_speed = GetComponent<Rigidbody>().velocity;
        last_position = GetComponent<Rigidbody>().position;
        if (current_speed.magnitude <= 100)
        {
            gameObject.GetComponent<TrailRenderer>().enabled = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (gameObject.tag == "Rocket")
        {
            Explode(gameObject.transform.position);
        }
        Projectile_On_Target(collision);
    }
    void Explode(Vector3 explosionCenter)
    {
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, explosionRadius);
        HashSet<GameObject> processedObjects = new HashSet<GameObject>();

        foreach (var hitCollider in hitColliders)
        {
            if (processedObjects.Contains(hitCollider.gameObject))
            {
                continue; // jump through same object
            }
            processedObjects.Add(hitCollider.gameObject); 

            //explosion force
            Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(1000f, explosionCenter, explosionRadius);
            }

            //damage transfer
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hitCollider.GetComponentInParent<IDamageable>();
            }
            if (damageable != null)
            {
                float distance = Vector3.Distance(hitCollider.transform.position, explosionCenter);
                float damage = CalculateExplodeDamage(distance) / 2; //half standard dmg

                if (hitCollider.gameObject.tag == "TurretBarrel")
                {
                    damageable.TakeDamage((int)(damage/2),false); //Barrel only take half explosion dmg
                }
                else if(hitCollider.gameObject.tag != "Rocket")
                {
                    damageable.TakeDamage((int)damage,false);
                }
            }
        }
        Destroy(gameObject);
    }
    private float CalculateExplodeDamage(float distance)
    {
        float relativeDistance = (explosionRadius - distance) / explosionRadius;
        float damage = relativeDistance * relativeDistance * baseDamage;

        damage = Mathf.Max(0f, damage);
        damage = Mathf.Min(damage, baseDamage);

        return damage;
    }
    private void Projectile_On_Target(Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;
        Vector3 hitPoint = collision.contacts[0].point;
        Quaternion hitRotation = Quaternion.LookRotation(collision.contacts[0].normal);
        float incidentAngle = Vector3.Angle(-current_speed.normalized, normal);

        if (CompareTag("Projectile"))
        {
            if (incidentAngle <= ricochetAngle)
            {
                PlayPenetrationEffect(collision.contacts[0].point, -current_speed.normalized, collision);
                //bullet has dmg only with Penetration
                IDamageable damageable = collision.collider.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = collision.collider.GetComponentInParent<IDamageable>();
                }
                if (damageable != null)
                {
                    damageable.TakeDamage(baseDamage,true);
                }
                Destroy(gameObject);
            }
            else
            {
                Vector3 ricochetDirection = Vector3.Reflect(current_speed.normalized, normal);
                PlayRicochetEffect(collision.contacts[0].point, ricochetDirection);
            }
        }
        else if (CompareTag("Rocket"))
        {
            Destroy(Instantiate(HitEffect_Particle, hitPoint, hitRotation).gameObject, 5f);

            Temp_Hit_Audio(hitPoint);

            Destroy(gameObject);
        }
    }
    void PlayRicochetEffect(Vector3 position, Vector3 direction)
    {
        Quaternion effectRotation = Quaternion.LookRotation(direction);
        Destroy(Instantiate(HitEffect_Particle, position, effectRotation).gameObject, 2f);
    }

    void PlayPenetrationEffect(Vector3 position, Vector3 direction, Collision collision)
    {
        Quaternion effectRotation = Quaternion.LookRotation(direction);
        Destroy(Instantiate(HitEffect_Particle, position, effectRotation).gameObject, 2f);
    }
    void Temp_Hit_Audio(Vector3 hitPoint)
    {
        GameObject tempAudioSource = new GameObject("TempAudio");
        tempAudioSource.transform.position = hitPoint; 

        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();
        audioSource.clip = Explosion; 
        audioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("Env")[0]; 
        audioSource.spatialBlend = 1.0f; 
        audioSource.volume = 2f; 
        audioSource.rolloffMode = AudioRolloffMode.Custom; 
        audioSource.minDistance = 1f; 
        audioSource.maxDistance = 1000f; 

        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(500f, 0.75f);
        curve.AddKey(1000f, 0f);
        audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);

        audioSource.Play();

        Destroy(tempAudioSource, Explosion.length);
    }
}
