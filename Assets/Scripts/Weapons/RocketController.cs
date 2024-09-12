// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static System.TimeZoneInfo;

public class RocketController : MonoBehaviour
{
    public GameObject bazuka;
    public GameObject Rocket;
    public Animator SKM_Animator;

    private Quaternion bazukaRotation_offset;

    private AudioSource audioSource;
    public AudioClip Rocket_AudioClip;

    private float fireRate;

    public bool canShoot = true;
    public bool gamePause = false;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        bazukaRotation_offset = bazuka.transform.rotation;
        fireRate = StaticGameDB.nuclear_rocket_data.RocketfireRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && canShoot && !gamePause)
        {
            StartCoroutine(Shoot_Rocket());
        }
        else if (Input.GetMouseButton(1) && canShoot && !gamePause)
        {
            StartCoroutine(Shoot_Rocket());
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

    IEnumerator Shoot_Rocket()
    {
        StartCoroutine(LerpParameter("BlendAim", 1, 0.3f));
        StartCoroutine(Lauch());
        canShoot = false;
        
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }
    IEnumerator Lauch()
    {
        yield return new WaitForSeconds(0.5f);
        GameObject bulletObj1 = Instantiate(Rocket, bazuka.transform.position, transform.rotation);
        bulletObj1.GetComponent<Projectile_Behavior>().Init_Speed_fromparent(GetComponent<Rigidbody>().velocity);
        audioSource.PlayOneShot(Rocket_AudioClip, 2f);
        StartCoroutine(LerpParameter("BlendAim", 0, 0.3f));
    }

}
