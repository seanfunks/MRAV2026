using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCSendTest : MonoBehaviour
{
    public OSC oSC;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B)){
            OscMessage message=new OscMessage();

            message.address="/TestBackGround";

            oSC.Send(message);
           


        }

        if(Input.GetKeyDown(KeyCode.S)){
                 OscMessage message=new OscMessage();

            message.address="/TestOneshot";

            oSC.Send(message);


        }


        
    }
}
