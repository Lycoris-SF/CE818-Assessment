using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeadPointer : MonoBehaviour
{
    public Transform target;
    public Rigidbody targetRigidbody;
    private Rigidbody selfRigidbody;
    public float bulletSpeed = 100f;
    public Transform aimPoint;

    private void Start()
    {
        selfRigidbody = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (target != null && targetRigidbody != null && aimPoint != null)
        {
            Vector3 currentDirection = selfRigidbody.transform.forward;

            Vector3 bulletVelocity = currentDirection * bulletSpeed + selfRigidbody.velocity;

            float distance = Vector3.Distance(selfRigidbody.position, target.position);

            float flightTime = distance / bulletVelocity.magnitude;

            Vector3 relativeTargetVelocity = targetRigidbody.velocity - selfRigidbody.velocity;

            Vector3 predictedPosition = target.position + relativeTargetVelocity * flightTime;

            aimPoint.position = predictedPosition;
        }
    }
}
