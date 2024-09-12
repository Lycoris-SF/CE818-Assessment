using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    public List<Transform> gunRoots;
    public GameObject Projectile;
    public Rigidbody Hull;

    private List<ParticleSystem> WeaponParticles;

    private AudioSource WeaponAudioSource;
    public AudioClip Weapon_AudioClip;

    //test only
    public float fireRate;
    private float megaFiringTime;


    public bool trigger = false;
    public bool beamFiring = false;
    private bool canShoot = true;

    // Start is called before the first frame update
    void Start()
    {
        //extend weapon for every class
        if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "SARA")
        {
            if (PlayerPrefs.GetInt("MasterMode") == 1)
            {
                fireRate = StaticGameDB.SARA_data.Mega_ReloadTime * 1.5f;
            }
            else
            {
                fireRate = StaticGameDB.SARA_data.Mega_ReloadTime;
            }
            megaFiringTime = StaticGameDB.SARA_data.Mega_FiringTime;
        }
        else if (transform.root.gameObject.GetComponent<EnemyController>().ShipClass == "MAGE")
        {
            if (PlayerPrefs.GetInt("MasterMode") == 1)
            {
                fireRate = StaticGameDB.MAGE_data.Mega_ReloadTime * 1.5f;
            }
            else
            {
                fireRate = StaticGameDB.MAGE_data.Mega_ReloadTime;
            }
            megaFiringTime = StaticGameDB.MAGE_data.Mega_FiringTime;
        }

            WeaponAudioSource = gameObject.GetComponentInParent<AudioSource>();
        Hull = GetComponentInParent<Rigidbody>();

        for (int i=0; i< gunRoots.Count; i++ )
        {
            if(gunRoots[i].GetComponentInChildren<ParticleSystem>()!=null)
                WeaponParticles.Add(gunRoots[i].GetComponentInChildren<ParticleSystem>());
        }
        Guns_ParticleOff();
    }

    // Update is called once per frame
    void Update()
    {
        if (canShoot && trigger)
        {
            StartCoroutine(Shoot());
            Guns_ParticleOn();
        }
        if (!canShoot||!trigger)
        {
            Guns_ParticleOff();
        }
    }

    private void Guns_ParticleOn()
    {
        if (WeaponParticles != null)
        {
            for (int i = 0; i < WeaponParticles.Count; i++)
            {
                if (WeaponParticles[i] != null)
                    WeaponParticles[i].Play();
            }
        }
    }
    private void Guns_ParticleOff()
    {
        if (WeaponParticles != null)
        {
            for (int i = 0; i < WeaponParticles.Count; i++)
            {
                if (WeaponParticles[i] != null)
                    WeaponParticles[i].Stop();
            }
        }
    }
    IEnumerator Shoot()
    {
        canShoot = false;
        StartCoroutine(BeamFiring());
        for (int i = 0; i < gunRoots.Count; i++)
        {
            GameObject newBullet = Instantiate(Projectile, gunRoots[i].transform.position, gunRoots[i].transform.rotation);
            newBullet.GetComponent<Beam_Behavior>().Init_Speed_fromparent(Hull, gunRoots[i], megaFiringTime);
        }
        WeaponAudioSource.PlayOneShot(Weapon_AudioClip, 1f);
        yield return new WaitForSeconds(fireRate+ Random.Range(0.0f, 0.5f));
        canShoot = true;
    }
    IEnumerator BeamFiring()
    {
        beamFiring = true;
        yield return new WaitForSeconds(megaFiringTime);
        beamFiring = false;
    }
}
