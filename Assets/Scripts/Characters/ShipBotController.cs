// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Author: Lycoris

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ShipBotController : MonoBehaviour
{
    public Transform target; //hack method is tobe abandoned later
    private Transform nearestTeammate;
    private List<GameObject> teams;
    public List<EngineHitLogic> Engines;

    public float moveSpeed = 5f;
    public float moveForce = 50f; 
    public float rotationForce = 50f;
    public float evadeDis = 350f;
    private float mass;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        mass = GetComponent<Rigidbody>().mass;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        if (target != null)
        {
            Seek_V1_0();
        }
        checkTeams();
        checkEngines();
        EvadeTeam_V1_0();
    }

    private void checkTeams()
    {
        teams = GameObject.FindGameObjectsWithTag("EFSF-Team").ToList();
        teams.AddRange(GameObject.FindGameObjectsWithTag("EFSF-FlagShip").ToList());
        teams.Remove(gameObject);

        List<Transform> TeamT = new List<Transform>();
        foreach (GameObject gameObject in teams)
        {
            TeamT.Add(gameObject.transform);
        }
        Transform nearest = GetClosest(TeamT);

        if (teams.Count != 0)
        {
            nearestTeammate = nearest;
        }
    }
    private void checkEngines()
    {
        if (Engines.Count != 0)
        {
            int count = 0;
            for(int i=0;i<Engines.Count;i++)
            {
                if (Engines[i]) count++;
            }
            if(count == 0)
            {
                target = null;
            }
        }
    }
    Transform GetClosest(List<Transform> TeamT)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Transform potentialTarget in TeamT)
        {
            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        return bestTarget;
    }

    private void Seek_V1_0()
    {
        if (target != null)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Vector3 targetDirection = (target.position - transform.position);
            float distance = targetDirection.magnitude;
            targetDirection = targetDirection.normalized;

            // 当接近目标时减速
            float speed = (distance < 350) ? (moveForce * (distance / 350)) : moveForce;

            rb.AddForce(targetDirection * speed * mass, ForceMode.Force);

            Vector3 rotationTorque = Vector3.Cross(transform.forward, targetDirection).normalized;
            rb.AddTorque(rotationTorque * rotationForce * mass, ForceMode.Force);

        }
    }
    private void EvadeTeam_V1_0()
    {
        if (nearestTeammate != null)
        {
            // evade
            Vector3 targetDirection = nearestTeammate.position - transform.position;
            float distance = targetDirection.magnitude;

            if (distance < evadeDis)
            {
                float evadeSpeed = moveForce * (1 - (distance / evadeDis));

                gameObject.GetComponent<Rigidbody>().AddForce(-targetDirection.normalized * evadeSpeed * mass /2, ForceMode.Force);
            }

            // away target
            /*Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            GetComponent<Rigidbody>().MoveRotation(Quaternion.RotateTowards(GetComponent<Rigidbody>().rotation, targetRotation, rotationForce * Time.deltaTime));*/
        }
    }
}
