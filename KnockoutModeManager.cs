using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

//using Photon.Realtime;

public class KnockoutModeManager : MonoBehaviourPunCallbacks //MonoBehaviour 
{
    [Header("General")]
    [SerializeField] float startCountDownTime = 3f;

    [Header("Knockout")]
   // [SerializeField] public GameObject[] players;
   // [SerializeField] public List<KnockoutModeManager> players = new List<KnockoutModeManager>();
    [SerializeField] public List<PlayerTicket> players = new List<PlayerTicket>();

    //[HideInInspector]
    public int tickets;
    //[HideInInspector]
    public int playerCount;
    int startLapCount;

    float countDownTimer;

    [HideInInspector]
    public bool start;
    //[HideInInspector]
    public bool startLap = true;

    public bool raceStart;


    void Awake()
    {
        //Set
        tickets = playerCount - 1;
        countDownTimer = startCountDownTime;
        startLap = true;
        startLapCount = playerCount;
    }

    void Update()
    {

        //Calls
        if (raceStart)
        {
            StartLap();
            KnockoutControl();

        }

        //Get player list
        
    }

    void StartCountdown()
    {
        //Countdown
        if (countDownTimer >= 0)
            countDownTimer -= Time.deltaTime;

        //Start race when countdown is done
        if (countDownTimer <= 0)
            start = true;
    }

    void StartLap()
    {
        //Ignore first tresspass
        if (startLapCount <= 0)
            startLap = false;
    }

    void KnockoutControl()
    {
        //Reset

    }

    public void Knockout()
    {
        //Count knockout tickets
        if (!startLap && tickets >= 0)
            tickets -= 1;
        else if (startLap)
            startLapCount -= 1;
    }

    public void Reset()
    {
        //Reset tickets
        if (playerCount > 2)
        {
            playerCount -= 1;
            tickets = playerCount - 1;
        }
        else if (playerCount == 2)
        {
            playerCount -= 1;
            tickets = playerCount;
        }

        //Reset tickets for all players
     //   for (int i = 0; i < players.Length; i++)
     //   {
       //     players[i].GetComponent<PlayerTicket>().ticket = false;
      //  }
    }

    

/*
    private GameObject go;
   // private GameObject go2;

    public void AddRiders()
    {

        GameObject[] tag;
      //  GameObject[] tag_2;


        tag = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log("Found TAG_1");
      //  tag_2 = GameObject.FindGameObjectsWithTag("OtherPlayer");
      //  Debug.Log("Found TAG_2");


        for (int i = 0; i < tag.Length; i++)

        {
            players.Add(tag[i].GetComponent<PlayerTicket>());
            Debug.LogFormat("Found {0} objects with TAG_1", tag.Length);
        }

       // for (int i = 0; i < tag_2.Length; i++)

       // {
       //     players.Add(tag_2[i].GetComponent<PlayerTicket>());
       //     Debug.Log("Add TAG_2");
       // }

    }

    //private GameObject go;
    private GameObject go2;

    public void AddOthers()
    {

       // GameObject[] tag;
          GameObject[] tag_2;


        //tag = GameObject.FindGameObjectsWithTag("Player");
        //Debug.Log("Found TAG_1");
          tag_2 = GameObject.FindGameObjectsWithTag("OtherPlayer");
          Debug.LogFormat("Found {0} objects with TAG_2", tag_2.Length);
      //  Debug.Log("Found TAG_2");


        for (int i = 0; i < tag_2.Length; i++)

        {
            players.Add(tag_2[i].GetComponent<PlayerTicket>());
            Debug.LogFormat("Found {0} objects with TAG_2", tag_2.Length);
        }

        // for (int i = 0; i < tag_2.Length; i++)

        // {
        //     players.Add(tag_2[i].GetComponent<PlayerTicket>());
        //     Debug.Log("Add TAG_2");
        // }

    }

*/

}
    
