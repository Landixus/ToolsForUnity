using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class SprintKomManager : MonoBehaviour {
    /*
    NOTE: Syncing a global json file as a leaderboard can not be a permanent approach.
    This should only be temporary until strava or a real database is implemented
    Possible future problems could range from leaderboard entries not being saved to leaderboard file being corrupted and all data being lost
    */

    public Material sprintMat;
    public Material komMat;
    public Material playersMat;

    //Array of all sprints, to be set in the inspector
    public Sprint[] sprints;

    private Sprint curSprint;
    private float startTime;

    private ES3Cloud cloud;

    //Used to prevent uploading an invalid json
    private bool canUpload = false;

    [SerializeField] private LeaderboardPanel sprintPanel;
    [SerializeField] private LeaderboardPanel komPanel;
    [SerializeField] private LeaderboardPanel playerPanel;

    [SerializeField] private TextMeshProUGUI timer;

    public float moveLeftTimer = 800f;
    public float moveDownTimer = -100f;


   // [HideInInspector]
    public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

    private void Start() {
        timer.gameObject.SetActive(false);
        if (cloud == null) {
            cloud = new ES3Cloud("https://freegroupride.de/fgrtoplist/ES3Cloud.php", "c91824e4f5");
        }
        StartCoroutine(DownloadLeaderboard());
    }

    private void Update() {
        if (curSprint != null) {
            timer.text = strFromSec(Time.time - startTime);
        }
    }

    public Sprint getSprintFromId(int id) {
        foreach (Sprint sprint in sprints) {
            if (sprint.sprintId == id) {
                return sprint;
            }
        }
        return null;
    }

    public List<LeaderboardEntry> getEntriesFromId(int id) {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        foreach (LeaderboardEntry entry in leaderboard) {
            if (entry.sprintId == id) {
                entries.Add(entry);
            }
        }
        return entries;
    }

    //Download the leaderboard file from the cloud and decode it into a json
    public IEnumerator DownloadLeaderboard() {
        canUpload = true;
        yield return StartCoroutine(cloud.DownloadFile("Leaderboard.json"));
        if (cloud.isError) {
            Debug.LogError(cloud.error);
            canUpload = false;
            leaderboard = new List<LeaderboardEntry>();
        } else {
            string json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "Leaderboard.json"));
            if (json != "{}") {
                try {
                    leaderboard.Clear();
                    leaderboard.AddRange(JsonHelper.FromJson<LeaderboardEntry>(json));
                } catch (System.Exception) {
                    Debug.LogError("Cannot decode leaderboard json, disabling uploading");
                    canUpload = false;
                    leaderboard = new List<LeaderboardEntry>();
                }
            }
        }
    }

    //Encode the leaderboard list into json and upload to the cloud
    public IEnumerator UploadLeaderboard() {
        if (canUpload == false) {
            yield break;
        }
        string json = JsonHelper.ToJson<LeaderboardEntry>(leaderboard.ToArray());
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "Leaderboard.json"), json);
        yield return StartCoroutine(cloud.UploadFile("Leaderboard.json"));
        if (cloud.isError) {
            Debug.LogError(cloud.error);
        }
    }

    //Downloads the current leaderboard from the server, adds the entry, uploads the new leaderboard to the server
    public IEnumerator NewLeaderboardEntry(LeaderboardEntry entry) {
        try {
            yield return StartCoroutine(DownloadLeaderboard());
        } finally {
            //Checks for entries from the same sprint and player
            bool entryfound = false;
            foreach (LeaderboardEntry _entry in leaderboard) {
                if (_entry.sprintId == entry.sprintId && _entry.playerName == entry.playerName) {
                    if (_entry.time > entry.time) {
                        leaderboard.Remove(_entry);
                        leaderboard.Add(entry);
                    }
                    entryfound = true;
                    break;
                }
            }
            if (entryfound == false) {
                leaderboard.Add(entry);
            }
            ShowUI(entry.sprintId);
        }
        yield return StartCoroutine(UploadLeaderboard());
    }

    public IEnumerator RemoveLeaderboardEntry(LeaderboardEntry entry)
    {
        try
        {
            yield return StartCoroutine(DownloadLeaderboard());
        }
        finally
        {
            //Checks for entries from the same sprint and player
            bool entryfound = false;
            foreach (LeaderboardEntry _entry in leaderboard)
            {
                if (_entry.sprintId == entry.sprintId && _entry.playerName == entry.playerName)
                {
                   
                        leaderboard.Remove(_entry);
                        leaderboard.Remove(entry);
                                     
                }
                entryfound = true;
                break;
            }
            if (entryfound == true)
            {
                leaderboard.Remove(entry);
            }
            ShowUI(entry.sprintId);
        }
        yield return StartCoroutine(UploadLeaderboard());
    }

    //Sends leaderboard data to the panel
    public void ShowUI(int sprintId) {
        Sprint sprint = getSprintFromId(sprintId);
        LeaderboardEntry[] entries = getEntriesFromId(sprintId).ToArray();
        if (!sprint.isKom)
        {
            sprintPanel.ShowLeaderboard(sprint, entries);
        }
        if (!sprint.isKom && sprint.isPList)
        {
            playerPanel.ShowPlayerBoard(sprint, entries);
        }

        else
        {
            komPanel.ShowLeaderboard(sprint, entries);
        }
       
    }

    public void StartSprint(Sprint sprint) {
        if (curSprint != null) {
            Debug.LogWarning("Cannot start sprint, already in a sprint");
            return;
        }
        curSprint = sprint;
        startTime = Time.time;
        timer.gameObject.SetActive(true);
        if (sprint.isPList)
        {
            timer.gameObject.GetComponent<TextMeshProUGUI>().enabled = false;
        }
        LeanTween.moveLocalX(timer.gameObject, moveLeftTimer, 1f);
        LeanTween.moveLocalY(timer.gameObject, moveDownTimer, 1f);
    }

    public void EndSprint(Sprint sprint) {
        if (curSprint == null) {
            Debug.LogWarning("Cannot end sprint, not in a sprint");
            return;
        }
        if (curSprint != sprint) {
            Debug.LogWarning("Cannot end sprint, id's are not matching");
            return;
        }
        float duration = Time.time - startTime;
        Debug.Log("Sprint named " + curSprint.name + " lasted for " + duration + " seconds.");

        //Add the sprint to the leaderboard
        LeaderboardEntry entry = new LeaderboardEntry();
        entry.sprintId = curSprint.sprintId;
        entry.sprintName = curSprint.name;
        entry.time = duration;
        entry.sceneName = SceneManager.GetActiveScene().name;
        entry.playerName = PlayerPrefs.GetString("BikerName", "Unnamed Player");
        StartCoroutine(NewLeaderboardEntry(entry));
        curSprint = null;
        timer.gameObject.SetActive(false);
    }

    public void RemoveSprint(Sprint sprint)
    {
        
        float duration = Time.time - startTime;
         //Remove the sprint from the leaderboard
        LeaderboardEntry entry = new LeaderboardEntry();
        entry.sprintId = curSprint.sprintId;
        entry.sprintName = curSprint.name;
        entry.time = duration;
        entry.sceneName = SceneManager.GetActiveScene().name;
        entry.playerName = PlayerPrefs.GetString("BikerName", "Unnamed Player");
        StartCoroutine(RemoveLeaderboardEntry(entry));
        curSprint = null;
        timer.gameObject.SetActive(false);
    }



    //Turns seconds into timer string
    public static string strFromSec(float time) {
        int minutes = Mathf.FloorToInt(time / 60);
        time -= minutes * 60;
        int seconds = Mathf.FloorToInt(time);
        time -= seconds;
        int miliseconds = Mathf.FloorToInt(time * 10);
        return string.Format("{0:00}:{1:00}:{2:0}", minutes, seconds, miliseconds);
    }
}

[System.Serializable]
public class Sprint {
    public string name;
    public int sprintId;
    public bool isKom;
    public bool isPList;
}

[System.Serializable]
public class LeaderboardEntry {
    //Id of the sprint
    public int sprintId;

    //Sprint name should still be recorded incase the order of sprint id's change in an update
    public string sprintName;

    //Name of the player
    public string playerName;

    //Time in seconds
    public float time;

    //Name of the scene
    public string sceneName;
}

public static class JsonHelper {
    public static T[] FromJson<T>(string json) {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T> {
        public T[] Items;
    }
}