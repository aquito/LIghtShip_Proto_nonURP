using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MandalaShader : MonoBehaviour
{

    private Material mandalaMat;
    // Start is called before the first frame update
    void Start()
    {
        mandalaMat = this.gameObject.GetComponent<Renderer>().material; 
    }

    // Update is called once per frame
    void Update()
    {
        float rotationShift = mandalaMat.GetFloat("_MandalaRotationShift");
        float scaleShift = mandalaMat.GetFloat("_MandalaScaleShift");

        if (rotationShift < 180)
        {
            rotationShift = rotationShift + 1f;
            mandalaMat.SetFloat("_MandalaRotationShift", rotationShift);
        }
        else
        {
            rotationShift = 0;
            mandalaMat.SetFloat("_MandalaRotationShift", rotationShift);
        }

        if(scaleShift < 20 || scaleShift > 1)
        {
            scaleShift++;
            mandalaMat.SetFloat("_MandalaScaleShift", scaleShift);

        }
        else
        {
            scaleShift--;
            mandalaMat.SetFloat("_MandalaScaleShift", scaleShift);
        }
    }
}
