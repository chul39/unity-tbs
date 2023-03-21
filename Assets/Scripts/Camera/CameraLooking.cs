using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLooking : MonoBehaviour
{
    [SerializeField] private bool invert;
    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (invert) transform.LookAt(transform.position + (cameraTransform.position - transform.position).normalized * -1);        
        else transform.LookAt(cameraTransform);        
    }

}
