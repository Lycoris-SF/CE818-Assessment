using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhotoMode : MonoBehaviour
{
    public float baseSpeed = 5.0f;
    public float maxSpeed = 20.0f;
    public float acceleration = 5.0f;
    public float rotationSpeed = 2.0f;

    public List<GameObject> uiElements;
    public Camera cameramain;

    private float speedMultiplier;

    public bool flycameraOn;
    private bool accelerating;

    private void Start()
    {
        flycameraOn = false;
        accelerating = false;
        speedMultiplier = baseSpeed;
    }
    void Update()
    {
        if (flycameraOn)
            FlyCamera();
    }

    private void FlyCamera()
    {
        accelerating = false;

        speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? Mathf.Lerp(speedMultiplier, maxSpeed, acceleration * Time.unscaledDeltaTime) : baseSpeed;
        MoveCamera(speedMultiplier);
        RotateCamera();

        if (!accelerating)
        {
            SlowDown();
        }

        if (Input.GetKeyDown(KeyCode.F11))
        {
            CaptureScreenshot();
        }
    }

    void MoveCamera(float speedMultiplier)
    {
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += transform.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection -= transform.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += transform.right;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection += transform.up;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDirection -= transform.up;
        }

        if (moveDirection.magnitude>0) accelerating = true;
        Vector3 moveAmount = moveDirection.normalized * speedMultiplier * Time.unscaledDeltaTime;
        transform.Translate(moveAmount, Space.World);
    }


    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * mouseX * rotationSpeed, Space.World);
        transform.Rotate(Vector3.left * mouseY * rotationSpeed, Space.Self);
    }

    void SlowDown()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 antiforce = -rb.velocity.normalized * 9.81f * Time.unscaledDeltaTime;
            rb.AddForce(antiforce);
            if (rb.velocity.magnitude < 0.1)
            {
                rb.velocity = Vector3.zero;
            }
        }
    }

    public void EnterPhotoMode()
    {
        Time.timeScale = 0;
        Cursor.visible = false;
        flycameraOn = true;
        TogglePhotoMode(true);
    }
    public void TogglePhotoMode(bool enteringPhotoMode)
    {
        foreach(GameObject uiElement in uiElements)
        {
            uiElement.SetActive(!enteringPhotoMode);
        }
    }

    public void ExitPhotoMode()
    {
        Time.timeScale = 1;
        gameObject.transform.position = cameramain.transform.position;
        gameObject.transform.rotation = cameramain.transform.rotation;
        gameObject.transform.localScale = cameramain.transform.localScale;
        flycameraOn = false;
        TogglePhotoMode(false);
    }
    public void CaptureScreenshot()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = "screenshot_" + timestamp + ".png";
        string path = Path.Combine(Application.persistentDataPath, filename);

        ScreenCapture.CaptureScreenshot(path);
        Debug.Log("Screenshot saved to: " + path);
    }
    void ToggleDisplay(bool enteringPhotoMode)
    {
        cameramain.enabled = !enteringPhotoMode;
        GetComponent<Camera>().enabled = enteringPhotoMode;
    }
}
