using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomCursor : MonoBehaviour
{
    public float joystickradius = 300f; // limit mouse radius

    public Text Debug;
    void Start()
    {
        Cursor.visible = false;

        joystickradius = Screen.height * joystickradius / 640;
    }

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 direction = mousePosition - screenCenter;
        float distance = direction.magnitude;

        if (!Application.isEditor) // no in editor
        {
            if (distance > joystickradius)
            {
                Vector3 restrictedPosition = screenCenter + direction.normalized * joystickradius;
                //Cursor.lockState = CursorLockMode.Confined;
                transform.position = restrictedPosition;
            }
            else
            {
                //Cursor.lockState = CursorLockMode.None;
                transform.position = Input.mousePosition;
            }
        }
        else
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                transform.position = screenCenter;
            }
            else
            {
                if (distance > joystickradius)
                {
                    Vector3 restrictedPosition = screenCenter + direction.normalized * joystickradius;
                    //Cursor.lockState = CursorLockMode.Confined;
                    transform.position = restrictedPosition;
                }
                else
                {
                    //Cursor.lockState = CursorLockMode.None;
                    transform.position = Input.mousePosition;
                }
            }
        }
    }
    public Vector2 get_fixed_mouse_pos()
    {
        return transform.position;
    }
}
