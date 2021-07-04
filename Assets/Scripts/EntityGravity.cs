using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGravity : MonoBehaviour
{
    Transform world;
    Rigidbody entityRigid;
    Vector3 directionOfGravity;
    float gravity = 9.8f;
    float rotationSpeed = 2;

    void Start()
    {
        world = GameObject.Find("World").transform;
        entityRigid = GetComponent<Rigidbody>();
        directionOfGravity = new Vector3(0, -gravity * entityRigid.mass, 0);
    }

    
    void FixedUpdate()
    {
        setDirectionOfGravity();
        entityRigid.AddForce(directionOfGravity);
        

        Debug.DrawRay(transform.position,directionOfGravity);
    }

    void setDirectionOfGravity()
    {
        switch(GetSextant())
        {
            case 0: //back face
                directionOfGravity = (world.rotation * Vector3.back).normalized * gravity * entityRigid.mass;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, world.rotation * Quaternion.Euler(90, 0, 0), rotationSpeed);
                break;
            case 1: //front face
                directionOfGravity = (world.rotation * Vector3.forward).normalized * gravity * entityRigid.mass;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, world.rotation * Quaternion.Euler(-90, 0, 0), rotationSpeed);
                break;
            case 2: //top face
                directionOfGravity = (world.rotation * Vector3.down).normalized * gravity * entityRigid.mass;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, world.rotation, rotationSpeed);
                break;
            case 3: //bottom face
                directionOfGravity = (world.rotation * Vector3.up).normalized * gravity * entityRigid.mass;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, world.rotation * Quaternion.Euler(180, 0, 0), rotationSpeed);
                break;
            case 4: //left face
                directionOfGravity = (world.rotation * Vector3.right).normalized * gravity * entityRigid.mass;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, world.rotation * Quaternion.Euler(0, 0, -90), rotationSpeed);
                break;
            case 5: //right face
                directionOfGravity = (world.rotation * Vector3.left).normalized * gravity * entityRigid.mass;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, world.rotation * Quaternion.Euler(0, 0, 90), rotationSpeed);
                break;
            default:
                break;
        }
    }

    int GetSextant()
    {
        Vector3 pos = world.InverseTransformPoint(transform.position);

        float absX = Math.Abs(pos.x);
        float absY = Math.Abs(pos.y);
        float absZ = Math.Abs(pos.z);

        if (absZ > absX && absZ > absY)
        {
            if (pos.z > 0) return 0; //back face
            else return 1; //front face
        }
        else if (absY > absX && absY > absZ)
        {
            if (pos.y > 0) return 2; //top face
            else return 3; //bottom face
        }
        else if (absX > absY && absX > absZ)
        {
            if (pos.x < 0) return 4; //left face
            else return 5; //right face
        }
        return -1;
    }
}
