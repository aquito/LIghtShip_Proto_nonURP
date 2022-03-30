using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCursor : MonoBehaviour
{

    public RectTransform cursor;

    public float degrees;

    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cursor.eulerAngles += Vector3.forward * degrees * Time.deltaTime;
    }
}
