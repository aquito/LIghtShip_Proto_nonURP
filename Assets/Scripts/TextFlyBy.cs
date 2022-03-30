using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFlyBy : MonoBehaviour
{

    public Vector3 movementVector;

    public Vector3 rotationVector;

    public float textSpeed;

  

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.Translate(movementVector * Time.deltaTime * textSpeed);
        this.gameObject.transform.Rotate(rotationVector, Space.Self);
    }
}
