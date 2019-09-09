using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTicket : MonoBehaviour
{
    public bool ticket;

    [SerializeField] KnockoutModeManager knockoutModeManager;

    [Header("UI")]
    [SerializeField] GameObject won;
    [SerializeField] GameObject lost;
    [SerializeField] GameObject button;


    void Start()
    {
        knockoutModeManager = GameObject.Find("KnockOutManager").GetComponent<KnockoutModeManager>();
        won = GameObject.Find("DemoUI/KnockOutModeUI/WonKnockOut");
        lost = GameObject.Find("DemoUI/KnockOutModeUI/LostKnockOut");
        button = GameObject.Find("DemoUI/KnockOutModeUI/CloseKnockOut");

    }

    void Update()
    {
        if (knockoutModeManager == null)
        {
            knockoutModeManager = GameObject.Find("KnockOutManager").GetComponent<KnockoutModeManager>();
        }
        if (won == null)
        {
            won = GameObject.Find("DemoUI/KnockOutModeUI/WonKnockOut");
        }
        if (lost == null)
        {
            lost = GameObject.Find("DemoUI/KnockOutModeUI/LostKnockOut");
        }
        if (button == null)
        {
            button = GameObject.Find("DemoUI/KnockOutModeUI/CloseKnockOut");
        }

        //Calls
        TicketCheck();
    }

    void TicketCheck()
    {
        //If you are only left, win the game
        if(knockoutModeManager.playerCount == 1)
            Win();

        //Control tickets
        if(knockoutModeManager.tickets == 0)
        {
            //If all players have tickets but you, you lose
            if(!ticket)
            {
                knockoutModeManager.Reset();
                gameObject.SetActive(false);
                Lose();
            }
        }

        //Test
        if(ticket)
            gameObject.GetComponent<Renderer>().material.color = Color.green;
        else
            gameObject.GetComponent<Renderer>().material.color = Color.white;
    }

    public void Win()
    {
        //Screen
        won.SetActive(true);

        //Set best time
        //bestTime.text = "Best Time: " + FormatTime(timerKeep[i - 1]);
        //bestTime.gameObject.SetActive(true);

        //Button
        button.SetActive(true);

        //Clock
        //clock.gameObject.SetActive(false);
    }

    public void Lose()
    {
        //Screen
        lost.SetActive(true);

        //Set best time
        //bestTime.text = "Best Time: " + FormatTime(timerKeep[i - 1]);
        //bestTime.gameObject.SetActive(true);

        //Button
        button.SetActive(true);

        //Clock
        //clock.gameObject.SetActive(false);
    }
}
