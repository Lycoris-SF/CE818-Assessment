// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterEffect_Behavior : MonoBehaviour
{
    private string AfterEffect_Type;
    private float init_speed;
    private int baseDamage;

    // Start is called before the first frame update
    void Start()
    {
        init_speed = StaticGameDB.nuclear_rocket_data.MetalJetMuzzleVelocity;
        GetComponent<Rigidbody>().velocity += transform.forward * init_speed;
        Destroy(gameObject, 1f);
    }
    public void Init_fromparent(string AFE, float Rest_penetration_ratio)
    {
        AfterEffect_Type = AFE;
        float AFESpeedFactor = StaticGameDB.nuclear_rocket_data.AFESpeedLowerFactor;
        GetComponent<Rigidbody>().velocity *= Rest_penetration_ratio * AFESpeedFactor;
        baseDamage = (int)(StaticGameDB.nuclear_rocket_data.RocketDamage * Rest_penetration_ratio);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (AfterEffect_Type == "HEAT")
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            damageable.TakeDamage(baseDamage,true);
            Destroy(gameObject);
        }
    }
}
