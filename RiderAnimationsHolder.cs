using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiderAnimationsHolder : bl_PhotonHelper
{

    public CentralSensor centralSensor;

    public Animator bikeAnimator;
    public Animator riderAnimator;

    private float currentPower;
    private float lastPower;

    public float speed;

    private float currentSpeed;
    private float lastSpeed;

    public bool StartedPedaling = false;

    public bool isStartPedaling = false;
    public bool isEasyPedaling = false;
    public bool isHardPeadling = false;
    public bool isGoToIdle = false;
    public bool isStartHardPedaling = false;
    public bool isVictory = false;
    public bool isGliding = false;

    private Animation anim;
    /// <summary>
    /// We need to handle the animations here and play them in the right way, and we need to add end, start, and end the animation before the loop animation starts playing
    // Parameters are:
    //Braking
    //Easy Pedaling
    //Easy Pedaling Left
    //Easy Pedaling Right
    //End Hard Pedaling
    //Go To Idle
    //Hard Pedaling
    //Idle
    //Medium Pedaling
    //Medium Pedaling Left
    //Medium Pedaling Right
    //Start Hard Pedaling
    //Start Pedaling
    //Victory
    //--
    //if we have a big > powerChange StartHardPedaling -- >and play Hard Pedaling for 5 seconds -> end hardPedaling then go to --> easy pedaling
    //if we are slow = <3 cadence and <3 speed we show Go To idle -> idle
    // if we finish a workout we play Victory
    // find a way to detect, are we are turn left or right to play the different animation, alternative dreamteck spline rotate points
    /// </summary>


    // Start is called before the first frame update
    void Start()
    {
        if (!isMine)
        {
            this.enabled = false;
        }
        bikeAnimator = GameObject.FindGameObjectWithTag("AniBike").GetComponent<Animator>();
        riderAnimator = GameObject.FindGameObjectWithTag("AniCyclist").GetComponent<Animator>();
        centralSensor = GameObject.Find("CentralSensor").GetComponent<CentralSensor>();
        anim = gameObject.GetComponent<Animation>();


        speed = centralSensor.speed;



    }

    // Update is called once per frame
    void Update()
    {
        // Check if we have all in
        if (centralSensor == null)
        {
            centralSensor = GameObject.Find("CentralSensor").GetComponent<CentralSensor>();
        }
        if (bikeAnimator == null)
        {
            bikeAnimator = GameObject.FindGameObjectWithTag("AniBike").GetComponent<Animator>();
        }
        if (riderAnimator == null)
        {
            riderAnimator = GameObject.FindGameObjectWithTag("AniCyclist").GetComponent<Animator>();
        }
        speed = centralSensor.speed;

        //if we StartPedaling
        if (centralSensor.cadence >= 15 && centralSensor.cadence <= 20 && centralSensor.speed >= 1)
        {
            if (!isStartPedaling)
            {
                isStartPedaling = true;
                bikeAnimator.SetTrigger("Start Pedaling");
                riderAnimator.SetTrigger("Start Pedaling");
            }
        }
        else
        {
            isStartPedaling = false;
        }

        //easy Pedaling
        if (centralSensor.cadence >= 20 && centralSensor.speed >= 2 && centralSensor.power < 400)
        {
            if (!isEasyPedaling)
            {
                isEasyPedaling = true;
                bikeAnimator.Play("Base Layer.ANIM_Racing_Bike_Easy_Pedaling");
                bikeAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3);

                riderAnimator.Play("Base Layer.ANIM_Male_Cyclist_Easy_Pedaling");
                riderAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3);
                /* 
                bikeAnimator.SetTrigger("Easy Pedaling");
                bikeAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3) / 2;
                riderAnimator.SetTrigger("Easy Pedaling");
                riderAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3) / 2;
                */
                Debug.Log("Easy Pedaling");
            }
        }
        else
        {
            isEasyPedaling = false;
        }

        //StartHardPedaling
        //check that we have a hard pedal in watts
        /*currentPower = centralSensor.power;
        if (currentPower - lastPower >= 100 && centralSensor.cadence >= 30)
        {
            if (!isStartHardPedaling)
                isStartHardPedaling = true;
            riderAnimator.SetTrigger("Start Hard Pedaling");
            bikeAnimator.SetTrigger("Start Hard Pedaling");
            lastPower = currentPower;
            Debug.Log("StartHardPedaling");
        }
        else
        {
            isStartHardPedaling = false;
        }*/

        //check if we ride with  a high power
        if (centralSensor.power >= 500 && centralSensor.cadence >= 30 && centralSensor.speed >= 10)
        {
            if (!isHardPeadling)
            {
                isHardPeadling = true;
                /*
                 bikeAnimator.SetTrigger("Hard Pedaling");
                 bikeAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3) / 2;
                 riderAnimator.SetTrigger("Hard Pedaling");
                 riderAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3) / 2;
                 */
                bikeAnimator.Play("Base Layer.ANIM_Racing_Bike_Hard_Pedaling");
                bikeAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3 /2); 

                riderAnimator.Play("Base Layer.ANIM_Male_Cyclist_Hard_Pedaling");
                riderAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3 /2);
                Debug.Log("HardPedaling");
            }
            else
            {
                isHardPeadling = false;
            }



        }

        // if workout message send we play the victory anmation
        //Is Victory
        //  if (workout Done = 1)
        /* {
           if (!isVictory)
           {
               isVictory = true;
               bikeAnimator.SetTrigger("Victory");
               riderAnimator.SetTrigger("Victory");

               Debug.Log("Victory");
           }
           else
           {
               isVictory = false;
           }
       */

        //Go To idle
        if (centralSensor.cadence <= 2 && centralSensor.speed <= 2 && centralSensor.power <= 2)
        {
            if (!isGoToIdle)
            {
                isGoToIdle = true;
                bikeAnimator.SetTrigger("Go To Idle");
                riderAnimator.SetTrigger("Go To Idle");
                Debug.Log("Got To Idle");
            }


        }
        else
        {
            isGoToIdle = false;
            
        }

        //Gliding
        if (centralSensor.cadence <= 0 && centralSensor.speed >= 10 && centralSensor.power <= 0)
        {
            if (!isGliding)
            {
                isGliding = true;
                bikeAnimator.Play("Base Layer.ANIM_Racing_Bike_Easy_Pedaling");
                bikeAnimator.speed = 0.001f; //Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3);

                riderAnimator.Play("Base Layer.ANIM_Male_Cyclist_Easy_Pedaling");
                riderAnimator.speed = 0.001f; //Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3);
                /* 
                bikeAnimator.SetTrigger("Easy Pedaling");
                bikeAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3) / 2;
                riderAnimator.SetTrigger("Easy Pedaling");
                riderAnimator.speed = Mathf.Min(centralSensor.GetComponent<CentralSensor>().cadence / 72f, 3) / 2;
                */
                Debug.Log("Gliding");
            }
        }
        else
        {
            isGliding = false;
        }
    }


}

