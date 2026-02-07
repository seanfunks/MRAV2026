using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotFly : MonoBehaviour
{
    // Start is called before the first frame update

    public float speedf;
    void Start()
    {
        Destroy(this.gameObject,3f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(new Vector3(0,0,-speedf));
        
    }
}
