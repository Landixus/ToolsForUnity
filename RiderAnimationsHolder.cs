
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

    public bool isAnimating = false;

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

        
      if (centralSensor.cadence >= 10 && centralSensor.speed >= 1)
        {
            if (!isAnimating)
            {
                isAnimating = true;
                bikeAnimator.SetTrigger("Start Pedaling");
                riderAnimator.SetTrigger("Start Pedaling");
            }
        }
        else
        {
            isAnimating = false;
        }

        

    }

    
}

