using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using VolumetricLines;

public class Beam_Behavior : MonoBehaviour
{
    public float init_speed;
    public float basePenetration;
    public int baseDamage;
    public float beamLenth;

    public float VL_offset;
    public float init_timelength;
    public float lifetime;

    private bool SoundPlaying;

    private Transform gunroot;
    private Rigidbody parent_rig;
    public VolumetricLineBehavior volumetricLine;
    public ParticleSystem HitEffect_Particle;
    public AudioMixer mainMixer;
    public AudioClip Explosion;

    void Start()
    {
        beamLenth = StaticGameDB.mega_particle_cannon.BeamLenthNormal;
        baseDamage = StaticGameDB.mega_particle_cannon.BeamDamage;
        basePenetration = StaticGameDB.mega_particle_cannon.BeamBasePenetration;
        init_speed = StaticGameDB.mega_particle_cannon.BeamMuzzleVelocity;

        volumetricLine = gameObject.GetComponent<VolumetricLineBehavior>();
        VL_offset = Mathf.Abs(volumetricLine.StartPos.z);
        lifetime = StaticGameDB.mega_particle_cannon.Beamlifetime;

        SoundPlaying = false;
    }
    public void Init_Speed_fromparent(Rigidbody parent, Transform root, float megaFiringTime)
    {
        gunroot = root;
        parent_rig = parent;
        init_timelength = megaFiringTime;
        StartCoroutine(StretchBeamOverTime(init_timelength));
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = other.GetComponentInParent<IDamageable>();
        }
        if (damageable != null)
        {
            damageable.TakeDamage(baseDamage,true);
            gameObject.GetComponent<Collider>().enabled = false;
        }
        Projectile_On_Target(other);
    }

    IEnumerator StretchBeamOverTime(float duration)
    {
        float elapsedTime = 0;
        CapsuleCollider capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
        if (capsuleCollider == null)
        {
            yield break; 
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scaleFactor = Mathf.Lerp(1.0f, beamLenth, elapsedTime / duration);
            float EndPointZ = scaleFactor - VL_offset;
            Vector3 newEndPoint = new Vector3(0, 0, -EndPointZ);    //revert beam here

            transform.rotation = gunroot.rotation;
            transform.position = gunroot.position + gunroot.forward * scaleFactor;

            volumetricLine.SetStartAndEndPoints(volumetricLine.StartPos, newEndPoint);
            capsuleCollider.center = new Vector3(0, 0, -scaleFactor / 2);
            capsuleCollider.height = scaleFactor;

            yield return null;
        }

        GetComponent<Rigidbody>().velocity += parent_rig.velocity + gunroot.forward * init_speed;
        Destroy(gameObject, lifetime);
    }
    private void Projectile_On_Target(Collider other)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (other.transform.position - transform.position).normalized, out hit, Mathf.Infinity))
        {
            if (!hit.collider.gameObject.CompareTag("Projectile"))
            {
                Vector3 hitPoint = hit.point;
                Quaternion hitRotation = Quaternion.LookRotation(hit.normal);

                if (CompareTag("Projectile"))
                {
                    Destroy(Instantiate(HitEffect_Particle, hitPoint, hitRotation).gameObject, 2f);
                    if(!SoundPlaying) Temp_Hit_Audio(hitPoint);
                }
            }
        }
    }
    void Temp_Hit_Audio(Vector3 hitPoint)
    {
        SoundPlaying = true;
        GameObject tempAudioSource = new GameObject("TempAudio");
        tempAudioSource.transform.position = hitPoint;

        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();
        audioSource.clip = Explosion;
        audioSource.outputAudioMixerGroup = mainMixer.FindMatchingGroups("Env")[0];
        audioSource.spatialBlend = 1.0f;
        audioSource.volume = 1f;
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
