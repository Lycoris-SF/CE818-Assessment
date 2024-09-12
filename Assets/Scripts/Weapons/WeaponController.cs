// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static System.TimeZoneInfo;

public class WeaponController : MonoBehaviour
{
    public GameObject machine_gun1;
    public GameObject machine_gun2;
    public GameObject Bullet_MachineGun;
    public Animator SKM_Animator;

    private ParticleSystem machine_gun_particle1;
    private ParticleSystem machine_gun_particle2;

    private AudioSource audioSource;
    public AudioClip MachineGun_AudioClip;

    //test only
    private float fireRate = 0.1f;

    public Transform Camera;

    public bool canShoot = true;
    public bool gamePause = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();

        machine_gun_particle1 = machine_gun1.GetComponentInChildren<ParticleSystem>();
        machine_gun_particle2 = machine_gun2.GetComponentInChildren<ParticleSystem>();
        MachineGun_ParticleOff();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && canShoot && !gamePause)
        {
            StartCoroutine(Shoot_MachineGun());
            MachineGun_ParticleOn();
        }
        else if (Input.GetMouseButton(0) && canShoot && !gamePause)
        {

            StartCoroutine(Shoot_MachineGun());
            MachineGun_ParticleOn();
        }
        if (Input.GetMouseButtonUp(0))
        {
            MachineGun_ParticleOff();
        }
    }
    IEnumerator LerpParameter(string parameterName, float target, float duration)
    {
        float time = 0;
        float startValue = SKM_Animator.GetFloat(parameterName);

        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, target, time / duration);

            SKM_Animator.SetFloat(parameterName, newValue);
            yield return null;
        }

        SKM_Animator.SetFloat(parameterName, target);
    }

    private void MachineGun_ParticleOn()
    {
        if (machine_gun_particle1 != null)
        {
            machine_gun_particle1.Play();
        }
        if (machine_gun_particle2 != null)
        {
            machine_gun_particle2.Play();
        }
    }
    private void MachineGun_ParticleOff()
    {
        if (machine_gun_particle1 != null)
        {
            machine_gun_particle1.Stop();
        }
        if (machine_gun_particle2 != null)
        {
            machine_gun_particle2.Stop();
        }
    }
    IEnumerator Shoot_MachineGun()
    {
        canShoot = false;
        GameObject bulletObj1=Instantiate(Bullet_MachineGun, machine_gun1.transform.position, machine_gun1.transform.rotation);
        GameObject bulletObj2=Instantiate(Bullet_MachineGun, machine_gun2.transform.position, machine_gun2.transform.rotation);
        bulletObj1.GetComponent<Projectile_Behavior>().Init_Speed_fromparent(GetComponent<Rigidbody>().velocity);
        bulletObj2.GetComponent<Projectile_Behavior>().Init_Speed_fromparent(GetComponent<Rigidbody>().velocity);
        audioSource.PlayOneShot(MachineGun_AudioClip,1f);
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
}
