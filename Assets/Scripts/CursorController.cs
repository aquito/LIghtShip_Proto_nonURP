using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{

    public GameObject cursor;

    private Transform cursorStartingPosition;

    private void Start()
    {
        cursorStartingPosition = cursor.transform;
    }

    private void Update()
    {
        if(cursor.transform.position.z != 0)
        {
            cursor.transform.position = cursorStartingPosition.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("mandala moved away from collision range");

        if (other.gameObject.tag == "cursor")
        {
            cursor.transform.position = cursorStartingPosition.position;
        }
    }
        

            

    
}
