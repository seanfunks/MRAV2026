using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fromAlbeton : MonoBehaviour
{

    public OSC osc;
    // Start is called before the first frame update
    void Start()
    {
        osc.SetAddressHandler("/Ableton/SweepVol",Recive);
        
    }

    void Recive(OscMessage oscMessage){
      print( oscMessage.GetFloat(0));
    

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
