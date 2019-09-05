using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Utility;
using Dreamteck.Splines;
using Smooth;
using System.Linq;

public class NetworkPlayer : MonoBehaviourPunCallbacks  //,IPunObservable
{
  
    //List of the scripts that should only be active for the local player (ex. PlayerController, MouseLook etc.)
    public MonoBehaviour[] localScripts;
   // List of the GameObjects that should only be active for the local player (ex. Camera, AudioListener etc.)
    public GameObject[] localObject;
    SmoothSyncPUN2 smoothSync;
    public GameObject otherplayer;
    // private GameObject player;
    public RPS_Storage storageScript;
    public GameObject knockOutManager;
    public KnockoutModeManager knockManagerList;

    private PlayerTicket test;
    // Start is called before the first frame update
    void Start()
    {


      //  RPS_Storage storageScript = GameObject.Find("RPS_Storage").GetComponent<RPS_Storage>();
        storageScript = GameObject.Find("RPS_Storage").GetComponent<RPS_Storage>();
        knockOutManager = GameObject.Find("KnockOutManager");
        knockManagerList = GameObject.Find("KnockOutManager").GetComponent<KnockoutModeManager>();

        if (!GetComponent<PhotonView>().IsMine)
        {
            Destroy(GetComponent<SplineFollower>());
            Destroy(GetComponent<BicyclePowerSim>());
            Destroy(GetComponent<BicycleSplineController>());
            Destroy(GetComponent<WaypointProgressTracker>());
            Destroy(GetComponent<EVP.VehicleCameraController>());
            Destroy(GetComponent<BikeFrontLight>());
            Destroy(GetComponent<AzureAudioDayNightController>());
            //  Destroy(GetComponent<RPS_Inspector>());
            //  Destroy(GetComponent<RPS_Position>());
            //  Destroy(GetComponent<RPS_Lap>());
            //  Destroy(GetComponent<RPS_LapUI>());
            //  Destroy(GetComponent<EndOfRace>());
            Destroy(GetComponent<RPS_ScreenUI>());
                                 
            tag = "OtherPlayer";

            for (int i = 0; i < localScripts.Length; i++)
            {
                localScripts[i].enabled = false;
            }
            for (int i = 0; i < localObject.Length; i++)
            {
                localObject[i].SetActive(false);

            }

            smoothSync = GetComponent<SmoothSyncPUN2>();

            if (smoothSync)
            {
                // Set up a validation method to check incoming States to see if cheating may be happening. 
                smoothSync.validateStateMethod = validateStateOfPlayer;
            }

           // otherplayer = GameObject.FindGameObjectWithTag("OtherPlayer");
            wait();
            Debug.Log("Ok Wait 2 seconds!");


            RPS_Position posScript = (RPS_Position)otherplayer.GetComponent<RPS_Position>() as RPS_Position;
            Debug.Log("RPS Adder2");

            test = GameObject.FindGameObjectWithTag("OtherPlayer").GetComponent<PlayerTicket>();
            knockManagerList.players.Add(test);
            //remove duplicates
           // knockManagerList.players = knockManagerList.players.Distinct().ToList();
            Debug.Log("testOtherPlayer");

            //  knockOutManager.GetComponent<KnockoutModeManager>().AddOthers();
            //  Debug.Log("AddOtherNetWorkScript");

            // add the position to the storageScript
            if (!storageScript.positionScript.Contains(posScript))
            {
                storageScript.positionScript.Add(posScript);
            }

        }
        

    }

    void Update()
    {
       if (storageScript == null)
       {
          storageScript = GameObject.Find("RPS_Storage").GetComponent<RPS_Storage>();
       }
       if (knockOutManager == null)
        {
            knockOutManager = GameObject.Find("KnockOutManager");
        }
        

    }


    IEnumerator wait()

    {
        // adds delays between instantiating new AI players
        //if (!GetComponent<PhotonView>().IsMine)
       // {

            yield return new WaitForSeconds(2);
            RPS_Position posScript = (RPS_Position)otherplayer.GetComponent<RPS_Position>() as RPS_Position;
            Debug.Log("RPS Adder3");
           

        // add the position to the storageScript
        if (!storageScript.positionScript.Contains(posScript))
            {
                storageScript.positionScript.Add(posScript);
            }
        //}
    }
        public static bool validateStateOfPlayer(StatePUN2 latestReceivedState, StatePUN2 latestValidatedState)
    {
        // Here I do a simple distance check using State.receivedOnServerTimestamp. This variable is updated
        // by Smooth Sync whenever a State is validated. If the object has gone more than 9000 units 
        // in less than a half of a second then I ignore the message. You might want to kick 
        // players here, add them to a ban list, or collect your own data to see if it keeps 
        // happening. 
        if (Vector3.Distance(latestReceivedState.position, latestValidatedState.position) > 9000.0f &&
            (latestReceivedState.ownerTimestamp - latestValidatedState.receivedOnServerTimestamp < .5f))
        {
            // Return false and refuse to accept the State. The State will not be added locally
            // on the server or sent out to other clients.
            return false;
        }
        else
        {
            // Return true to accept the State. The State will be added locally on the server and sent out 
            // to other clients.
            return true;
        }
    }

}
