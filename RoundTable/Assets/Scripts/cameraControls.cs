using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControls : MonoBehaviour
{
    [Header("Input Sensitivity")]
    [SerializeField] int sensHori;
    [SerializeField] int sensVert;
    [Header("CameraLock")] // Default to straight vertical(s)
    [SerializeField] int lockVerMin;
    [SerializeField] int lockVerMax;
    [Header("accessibility")]
    [SerializeField] bool invertY;

    float xRotation;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensVert;
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensHori;


        if (invertY)
        {
            xRotation += mouseY;
        }
        else
        {
            xRotation -= mouseY;
        }

        // Clamp
        xRotation = Mathf.Clamp(xRotation, lockVerMin, lockVerMax);

        // Rotate Camera on X Axis
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Rotate Parent/Capsule on Y-Axis
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
