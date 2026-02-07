using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class birdPath : MonoBehaviour
{
     public float r=3;
     public float speedf=0.001f;
     public float angle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         CircularFlying();
    }

    void CircularFlying(){

        transform.localPosition=( new Vector3(r*Mathf.Cos(angle),r*Mathf.Cos(angle),r*Mathf.Sin(angle)));
        angle+=speedf;


    }


}
