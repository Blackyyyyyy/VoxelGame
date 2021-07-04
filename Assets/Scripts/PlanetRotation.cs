using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    public bool setTimeScale;
    public float newTimeScale;

    float rotationSpeedX;
    float rotationSpeedY;
    float rotationSpeedZ;

    Rigidbody planetRigid;

    private void Start()
    {
        planetRigid = GetComponent<Rigidbody>();

        rotationSpeedX = Random.Range(0, 10);
        rotationSpeedY = Random.Range(0, 10);
        rotationSpeedZ = Random.Range(0, 10);
    }

    private void FixedUpdate()
    {
       planetRigid.AddTorque(rotationSpeedX * planetRigid.mass, rotationSpeedY * planetRigid.mass, rotationSpeedZ * planetRigid.mass);
    }

    private void Update()
    {
        if(setTimeScale)
        {
            Time.timeScale = newTimeScale;
            setTimeScale = false;
        }
    }
}
