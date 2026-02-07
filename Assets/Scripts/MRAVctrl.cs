using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRAVctrl : MonoBehaviour
{


    public GameObject [] BackGroundFX;
    public GameObject [] OneShotFx;

    public GameObject [] ControllerHolo;



    public OSC osc;

    // Start is called before the first frame update
 void Start()
    {

        osc=FindObjectOfType<OSC>();
        osc.SetAllMessageHandler(Recive);
      
        
    }

    void Recive(OscMessage oscMessage){


        switch(oscMessage.address)
        
        {
            //Background
            case "/TestBackGround":
              TriggerBackGround((int)Random.Range(0,BackGroundFX.Length));
            break;

            //OneShot
            case "/TestOneshot":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/RU":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/LU":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;


            case "/RD":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/LD":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;


                        //OneShot
            case "/T1":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/T2":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/T3":
            TriggerBackGround((int)Random.Range(0,OneShotFx.Length));
           //TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;


            case "/T4":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;


                                   //OneShot
            case "/TM1":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/TM2":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/TM3":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/TM4":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;



                                               //OneShot
            case "/BM1":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/BM2":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/BM3":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/BM4":
           TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;



            case "/B1":
                TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/B2":
            TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/B3":
                TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;

            case "/B4":
                TriggerOnshot((int)Random.Range(0,OneShotFx.Length));
            break;










        }
      

    }




    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B)){
            TriggerBackGround((int)Random.Range(0,BackGroundFX.Length));


        }

        if(Input.GetKeyDown(KeyCode.S)){
            TriggerOnshot((int)Random.Range(0,OneShotFx.Length));


        }







        
    }
    void HoloFx(int i){



    }




    void TriggerOnshot(int i){

        GameObject fx =Instantiate(OneShotFx[i],new Vector3(Random.Range(-3,3),Random.Range(-2,3),Random.Range(3,5)),Quaternion.identity);

        fx.AddComponent<OneShotFly>();
        fx.GetComponent<OneShotFly>().speedf=0.02f;

    





    }

    void TriggerBackGround(int i){

        if(BackGroundFX[i].activeInHierarchy ==true){
            BackGroundFX[i].SetActive(false);

        }else{

            BackGroundFX[i].SetActive(true);

        }



    }
}
