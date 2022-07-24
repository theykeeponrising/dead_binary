using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    // Used for Camera controls

    // Update is called once per frame
    void Update()
    {
        float xAxisValue = Input.GetAxis("Horizontal");
        float zAxisValue = Input.GetAxis("Vertical");
        if (Camera.current != null)
        {
            // Camera.current.transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue));
            Camera.current.transform.Translate(new Vector3(xAxisValue * 0.50f, zAxisValue * 0.25f, zAxisValue * 0.25f));
        }
    }
}
