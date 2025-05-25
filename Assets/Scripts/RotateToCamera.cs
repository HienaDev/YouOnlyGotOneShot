using System;
using UnityEngine;

public class RotateToCamera : MonoBehaviour
{

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        transform.rotation = _cam.transform.rotation;
    }
}
