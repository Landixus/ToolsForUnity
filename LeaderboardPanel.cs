using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardPanel : MonoBehaviour {
    public TextMeshProUGUI[] placeTexts;
    public TextMeshProUGUI header;
    public int displayTime = 5;

    public void ShowLeaderboard(Sprint sprint, LeaderboardEntry[] entries) {
        gameObject.SetActive(true);
        header.text = sprint.name;
        entries = entries.OrderBy(a => a.time).ToArray();
        //Extra check to prevent repeating names
        List<string> names = new List<string>();
        int indexMinus = 0;
        for (int i = 0; i < placeTexts.Length + indexMinus; i++) {
            if (i < entries.Length) {
                if (!names.Contains(entries[i].playerName)) {
                    placeTexts[i - indexMinus].text = (i + 1 - indexMinus).ToString() + "- " + entries[i].playerName + " " + SprintKomManager.strFromSec(entries[i].time);
                    names.Add(entries[i].playerName);
                } else {
                    indexMinus++;
                }
            } else {
                placeTexts[i - indexMinus].text = "";
            }
        }
        StartCoroutine(ShowBoard(displayTime));
    }



    public void ShowPlayerBoard(Sprint sprint, LeaderboardEntry[] entries)
    {
        gameObject.SetActive(true);
        header.text = sprint.name;
        entries = entries.OrderBy(a => a.time).ToArray();
        //Extra check to prevent repeating names
        List<string> names = new List<string>();
        int indexMinus = 0;
        for (int i = 0; i < placeTexts.Length + indexMinus; i++)
        {
            if (i < entries.Length)
            {
                if (!names.Contains(entries[i].playerName))
                {
                    placeTexts[i - indexMinus].text = (i + 1 - indexMinus).ToString() + "- " + entries[i].playerName; // + " " + SprintKomManager.strFromSec(entries[i].time);
                    names.Add(entries[i].playerName);
                }
                else
                {
                    indexMinus++;
                }
            }
            else
            {
                placeTexts[i - indexMinus].text = "";
            }
        }
        StartCoroutine(ShowBoard(displayTime));
    }



    private IEnumerator ShowBoard(int seconds) {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}
