using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private float verticalRotation = 0f;

    public void FixedUpdate()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        verticalRotation = -mouseY;

        transform.RotateAround(target.position, Vector3.up, mouseX * 3);
        transform.RotateAround(target.position, transform.right, -mouseY * 3);

        transform.LookAt(target);
    }
}
