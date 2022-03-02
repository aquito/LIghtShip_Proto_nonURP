using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities;

public class ThrowDrop : MonoBehaviour
{
    bool dragging = false;
    float distance;
    public float throwSpeed;
    public float archSpeed;
    public float speed;


    void Update()
    {

        if (PlatformAgnosticInput.touchCount <= 0) { return; }

        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            dragging = true;
        }

        if (touch.phase == TouchPhase.Ended)
        {
            this.GetComponent<Rigidbody>().useGravity = true;
            this.GetComponent<Rigidbody>().velocity += Camera.main.transform.forward * throwSpeed;
            this.GetComponent<Rigidbody>().velocity += Camera.main.transform.forward * archSpeed;
            dragging = false;
        }

            if (dragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);
            transform.position = Vector3.Lerp(this.transform.position, rayPoint, speed * Time.deltaTime);
        }
    }
}
