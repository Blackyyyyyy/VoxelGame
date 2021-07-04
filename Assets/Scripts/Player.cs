using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float sensitivity;
    public float dragSensitivity;

    public Transform world;
    public Transform player;

    void FixedUpdate()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * speed);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * speed);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.Translate(Vector3.down * speed);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if(Input.GetKeyUp(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector3 rot = new Vector3(transform.rotation.eulerAngles.x + -10 * Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity,
                                          transform.rotation.eulerAngles.y + 10 * Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity, 0);

            transform.rotation = Quaternion.Euler(rot);
        }

        if(Input.GetKey(KeyCode.Mouse1))
        {
            Vector3 rot1 = new Vector3(world.rotation.eulerAngles.x + 10 * Input.GetAxis("Mouse Y") * Time.deltaTime * dragSensitivity,
                                      world.rotation.eulerAngles.y + -10 * Input.GetAxis("Mouse X") * Time.deltaTime * dragSensitivity, 0);
            world.rotation = Quaternion.Euler(rot1);
        }

    }
}
