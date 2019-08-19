using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MultiplayerManager : MonoBehaviourPunCallbacks

{
    
    [SerializeField]
    private Text connectionText;
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject[] playerModel;
    [SerializeField]
    private GameObject serverWindow;
    [SerializeField]
    private GameObject messageWindow;
    [SerializeField]
   
    private InputField username;
    [SerializeField]
    private InputField roomName;
    [SerializeField]
    private InputField roomList;
    [SerializeField]
    private InputField messagesLog;

    private GameObject player;
    private GameObject rooGO;

    public GameObject SPPlayer;
    private GameObject pairScreen;
    
    private Queue<string> messages;
    private const int messageCount = 10;
    private string nickNamePrefKey = "PlayerName";

   // private GameObject otherplayer;
    /// The RPS Storage script for race position
    public RPS_Storage storageScript;
    //PhotonNetwork.Send

    public GameObject raceGameManager;
    public GameObject TimeAttackPanel;
    public GameObject TimeAttackCountdown;

    public GameObject knockOutUI;
    public GameObject knockOutManager;
    public GameObject knockOutButton;


        /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {

        serverWindow = GameObject.Find("DemoUI/NetworkPanel");
        messageWindow = GameObject.Find("DemoUI/MessagePanel");
        connectionText = GameObject.Find("DemoUI/NetworkMessage").GetComponent<Text>();
        username = GameObject.Find("DemoUI/NetworkPanel/PlayerNameInputField").GetComponent<InputField>();
        roomName = GameObject.Find("DemoUI/NetworkPanel/RoomNameInputField").GetComponent<InputField>();
        roomList = GameObject.Find("DemoUI/NetworkPanel/RoomListInputField").GetComponent<InputField>();
        messagesLog = GameObject.Find("DemoUI/MessagePanel/MessageLog").GetComponent<InputField>();
        pairScreen = GameObject.Find("DemoUI/Pairing");
        SPPlayer = GameObject.Find("Bicycle");
        knockOutUI = GameObject.Find("DemoUI/KnockOutModeUI");
        knockOutButton = GameObject.Find("DemoUI/NetworkPanel/KnockOutModeButton");
        //RPS
        /*RPS_Storage*/ storageScript = GameObject.Find("RPS_Storage").GetComponent<RPS_Storage>();
      //  knockOutUI.SetActive (false);
        knockOutButton.SetActive(false);
        TimeAttackCountdown = GameObject.Find("CountDownManager");



        GameObject rootGO = GameObject.Find("SpawnPointers");
        spawnPoints = new Transform[rootGO.transform.childCount];

        for (int i = 0; i < rootGO.transform.childCount; i++)
            spawnPoints[i] = rootGO.transform.GetChild(i); 
            messages = new Queue<string>(messageCount);

        if (PlayerPrefs.HasKey(nickNamePrefKey))
        {
           // username.text = PlayerPrefs.GetString(nickNamePrefKey);
            ProfileSessionManager.GetUsername();
            username.text = ProfileSessionManager.curName.ToString();
        }
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        connectionText.text = "Connecting to lobby...";
    }

    void Update()
    {
        //New from John
        knockOutManager.GetComponent<KnockoutModeManager>().playerCount = PhotonNetwork.CountOfPlayers;
    }

    /// <summary>
    /// Called on the client when you have successfully connected to a master server.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// Called on the client when the connection was lost or you disconnected from the server.
    /// </summary>
    /// <param name="cause">DisconnectCause data associated with this disconnect.</param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionText.text = cause.ToString();
    }

    /// <summary>
    /// Callback function on joined lobby.
    /// </summary>
    public override void OnJoinedLobby()
    {
        serverWindow.SetActive(true);
        connectionText.text = "";
    }

    /// <summary>
    /// Callback function on reveived room list update.
    /// </summary>
    /// <param name="rooms">List of RoomInfo.</param>
    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        roomList.text = "";
        foreach (RoomInfo room in rooms)
        {
            roomList.text += room.Name + "\n";
        }
    }

    /// <summary>
    /// The button click callback function for join room.
    /// </summary>
    public void JoinRoom()
    {
        serverWindow.SetActive(false);
       // connectionText.text = "Joining room...";
        PhotonNetwork.LocalPlayer.NickName = username.text;
        PlayerPrefs.SetString(nickNamePrefKey, username.text);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            MaxPlayers = 8

        };
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinOrCreateRoom(roomName.text, roomOptions, TypedLobby.Default);
        }
        else
        {
            connectionText.text = "PhotonNetwork connection is not ready, try restart it.";
        }
       
        
    }

    /// <summary>
    /// Callback function on joined room.
    /// </summary>
    ///
   

    public override void OnJoinedRoom()
    {
       // connectionText.text = "joined10";
        Respawn(1.0f);

       
    }
    float spawnTime = 2.0f;
    /// <summary>
    /// Start spawn or respawn a player.
    /// </summary>
    /// <param name="spawnTime">Time waited before spawn a player.</param>
    void Respawn(float spawnTime)
    {
           // sceneCamera.enabled = true;
        StartCoroutine(RespawnCoroutine(spawnTime));
       

    }

    /// <summary>
    /// The coroutine function to spawn player.
    /// </summary>
    /// <param name="spawnTime">Time waited before spawn a player.</param> 
    
    IEnumerator RespawnCoroutine(float spawnTime)
    {
        yield return new WaitForSeconds(spawnTime);
        messageWindow.SetActive(true);
       
        int playerIndex = Random.Range(0, playerModel.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        player = PhotonNetwork.Instantiate(playerModel[playerIndex].name, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation, 0);

        
        //  sceneCamera.enabled = false;
        if (spawnTime == 0)
        {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Joined the Ride again.");
            SPPlayer.SetActive(false);
            Destroy(SPPlayer);
            pairScreen.SetActive(true);
          //  if (PhotonNetwork.IsMasterClient)
         //   {
        //        serverWindow.SetActive(true);
         //       knockOutButton.SetActive(true);
         //   }
          //  knockOutManager.GetComponent<KnockoutModeManager>().AddRiders();
          //  Debug.Log("Knockout Adder1");
        }
        else
        {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Joined the Ride");
            Destroy(SPPlayer);
            pairScreen.SetActive(true);
            RPS_Position posScript = (RPS_Position)player.GetComponent<RPS_Position>() as RPS_Position;
            Debug.Log("RPS Adder");
            yield return new WaitForSeconds(2.0f);
            knockOutManager.GetComponent<KnockoutModeManager>().AddRiders();
            Debug.Log("Knockout Adder2");

            // add the position to the storageScript
            if (!storageScript.positionScript.Contains(posScript))
            {
                storageScript.positionScript.Add(posScript);
            }
            if (PhotonNetwork.IsMasterClient)
            {
                serverWindow.SetActive(true);
                knockOutButton.SetActive(true);
            }

        }

    }

      
    
    /// <summary>
    /// Add message to message panel.
    /// </summary>
    /// <param name="message">The message that we want to add.</param>
    void AddMessage(string message)
    {
        photonView.RPC("AddMessage_RPC", RpcTarget.All, message);
    }

    /// <summary>
    /// RPC function to call add message for each client.
    /// </summary>
    /// <param name="message">The message that we want to add.</param>
    [PunRPC]
    void AddMessage_RPC(string message)
    {
        messages.Enqueue(message);
        if (messages.Count > messageCount)
        {
            messages.Dequeue();
        }
        messagesLog.text = "";
        foreach (string m in messages)
        {
            messagesLog.text += m + "\n";
        }
    }
    /// <summary>
    /// Callback function when other player disconnected.
    /// </summary>
    public override void OnPlayerLeftRoom(Player other)
    {
       

        if (PhotonNetwork.IsMasterClient)
        {
            AddMessage("Player " + other.NickName + " Left Ride.");
        }
        
    }
    
    void OnGUI()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        //Show the Room name
        //string RoomName = (PhotonNetwork.CurrentRoom.Name "RaumName");
        GUI.Label(new Rect(5, 5, 200, 25), "RoomName" + " " + " " + PhotonNetwork.CurrentRoom.Name);

        //Show the list of the players connected to this Room
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            //Show if this player is a Master Client. There can only be one Master Client per Room so use this to define the authoritative logic etc.)
            string isMasterClient = (PhotonNetwork.PlayerList[i].IsMasterClient ? ": MasterClient" : "");
            GUI.Label(new Rect(5, 35 + 30 * i, 200, 25), PhotonNetwork.PlayerList[i].NickName + isMasterClient);
            
        }
    }

    public void CloseWindow()
    {
       serverWindow.SetActive(!serverWindow.activeSelf);
       //InputSystem.EnableDevice(Keyboard.current);
       
    }

    public void StartTimeAttack()
    {
        raceGameManager.SetActive (true);
        TimeAttackPanel.SetActive(true);
        TimeAttackCountdown.GetComponent<CountdownVeloDrome>().enabled = true;

        serverWindow.SetActive(!serverWindow.activeSelf);
    }

    public void StartKnockOutMode()
    {
       // knockOutManager.GetComponent<KnockoutModeManager>().enabled = true;
        knockOutManager.GetComponent<KnockoutModeManager>().raceStart = true;
        serverWindow.SetActive(!serverWindow.activeSelf);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = false
        };

    }
    

    



    /*
    public void SetRoomProperties()
    {

       //Set Room properties (Only Master Client is allowed to set Room properties)
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable setRoomProperties = new Hashtable();
            setRoomProperties.Add("GameMode", "FFA");
            setRoomProperties.Add("AnotherProperty", "Test");
            PhotonNetwork.CurrentRoom.SetCustomProperties(setRoomProperties);
        }
            //Will print "FFA"
            print((string) PhotonNetwork.CurrentRoom.CustomProperties["GameMode"]);
            //Will print "Test"
            print((string) PhotonNetwork.CurrentRoom.CustomProperties["AnotherProperty"]);
        }*/
}











/*{
    //public PhotonView photonView;
    //for messages join and leave room
    [SerializeField]
    private GameObject messageWindow;
    [SerializeField]
    private InputField messagesLog;
    [SerializeField]
    private Text connectionText;
    [SerializeField]
    //private InputField username;
    private Text username;
    private GameObject player;
    private Queue<string> messages;
    private const int messageCount = 10;

    [SerializeField]
    private GameObject serverWindow;
    [SerializeField]
    private InputField roomName;
    [SerializeField]
    private InputField roomList;

    private string nickNamePrefKey = "PlayerName";





    // private string nickNamePrefKey = "PlayerName";


    // Start is called before the first frame update
    void Start()
    {
        
        //Fill MessageWindowsFields
        messageWindow = GameObject.Find("DemoUI/MessagePanel");
        messagesLog = GameObject.Find("DemoUI/MessagePanel/MessageLog").GetComponent<InputField>();
        connectionText = GameObject.Find("DemoUI/NetworkMessage").GetComponent<Text>();
        username = GameObject.Find("DemoUI/MessagePanel/PlayerName").GetComponent<Text>();


        //for messages from player
        messages = new Queue<string>(messageCount);
        username.text = PlayerPrefs.GetString("Profile" + PlayerPrefs.GetInt("SelectedPlayerProfile", 1).ToString());
       
        ////end new add

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        connectionText.text = "Connecting to lobby...";


        GameObject offlinePlayer, go;
        Vector3 playerPos = new Vector3(0, 0, 0);
        Vector3 playerScale = new Vector3(1, 1, 1);
        Quaternion playerRot = Quaternion.identity;
        //Transform playerTransform;

        if (PhotonNetwork.IsConnected)
        {
           // connectionText.text = "Joining room...";
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("Profile" + PlayerPrefs.GetInt("SelectedPlayerProfile", 1).ToString());
            PlayerPrefs.SetString("Profile" + PlayerPrefs.GetInt("SelectedPlayerProfile", 1).ToString(), username.text);
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Joined the Ride.");

            offlinePlayer = GameObject.FindGameObjectWithTag("Player");
            if (offlinePlayer != null)
            {
                playerPos = offlinePlayer.transform.position;
                playerRot = offlinePlayer.transform.rotation;
                //playerScale = offlinePlayer.transform.sc;

                GameObject.Destroy(offlinePlayer);

            }
            //
        
           
            // GameObject go =  PhotonNetwork.Instantiate("Bicycle_Multi", playerPos, playerRot);
            switch (PhotonNetwork.PlayerList.Length - 1)
            {
                case 0:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), playerPos, playerRot);
                    break;
                case 1:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), offlinePlayer.transform.position + offlinePlayer.transform.right * 0.5f, playerRot);
                    break;
                case 2:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), offlinePlayer.transform.position - offlinePlayer.transform.right * 0.5f, playerRot);
                    break;
                case 3:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), offlinePlayer.transform.position + offlinePlayer.transform.right * 1f, playerRot);
                    break;
                case 4:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), offlinePlayer.transform.position - offlinePlayer.transform.right * 1f, playerRot);
                    break;
                case 5:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), offlinePlayer.transform.position + offlinePlayer.transform.right * 1.5f, playerRot);
                    break;
                case 6:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), offlinePlayer.transform.position - offlinePlayer.transform.right * 1.5f, playerRot);
                    break;
                case 7:
                    go = PhotonNetwork.Instantiate("Bicycle" + (PhotonNetwork.PlayerList.Length - 1).ToString(), offlinePlayer.transform.position + offlinePlayer.transform.right * 2f, playerRot);
                    break;
            }
            //GameObject go = PhotonNetwork.Instantiate("Bicycle"+(PhotonNetwork.PlayerList.Length - 1).ToString(), playerPos, playerRot);
            //go.transform.sc= playerTransform;
        }
    }

    /// <summary>
    /// Called on the client when you have successfully connected to a master server.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    /// <summary>
    /// Called on the client when the connection was lost or you disconnected from the server.
    /// </summary>
    /// <param name="cause">DisconnectCause data associated with this disconnect.</param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionText.text = cause.ToString();
    }

    /// <summary>
    /// Callback function on joined lobby.
    /// </summary>
    public override void OnJoinedLobby()
    {
        serverWindow.SetActive(true);
        connectionText.text = "";
    }

    /// <summary>
    /// Callback function on reveived room list update.
    /// </summary>
    /// <param name="rooms">List of RoomInfo.</param>
    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        roomList.text = "";
        foreach (RoomInfo room in rooms)
        {
            roomList.text += room.Name + "\n";
        }
    }

    /// <summary>
    /// The button click callback function for join room.
    /// </summary>
    public void JoinRoom()
    {
        serverWindow.SetActive(false);
        connectionText.text = "Joining room...";
        PhotonNetwork.LocalPlayer.NickName = username.text;
        PlayerPrefs.SetString(nickNamePrefKey, username.text);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            MaxPlayers = 8
        };
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinOrCreateRoom(roomName.text, roomOptions, TypedLobby.Default);
        }
        else
        {
            connectionText.text = "PhotonNetwork connection is not ready, try restart it.";
        }
    }

    /// <summary>
    /// Callback function on joined room.
    /// </summary>
    public override void OnJoinedRoom()
    {
        connectionText.text = "";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //Respawn(0.0f);
    }
    
    /// <summary>
    /// Start spawn or respawn a player.
    /// </summary>
    /// <param name="spawnTime">Time waited before spawn a player.</param>
    void Respawn(float spawnTime)
    {
        sightImage.SetActive(false);
        sceneCamera.enabled = true;
        StartCoroutine(RespawnCoroutine(spawnTime));
    }

    /// <summary>
    /// The coroutine function to spawn player.
    /// </summary>
    /// <param name="spawnTime">Time waited before spawn a player.</param>
    IEnumerator RespawnCoroutine(float spawnTime)
    {
        yield return new WaitForSeconds(spawnTime);
        messageWindow.SetActive(true);
        sightImage.SetActive(true);
        int playerIndex = Random.Range(0, playerModel.Length);
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        player = PhotonNetwork.Instantiate(playerModel[playerIndex].name, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation, 0);
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        playerHealth.RespawnEvent += Respawn;
        playerHealth.AddMessageEvent += AddMessage;
        sceneCamera.enabled = false;
        if (spawnTime == 0)
        {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Joined the Ride.");
        }
        else
        {
            AddMessage("Player " + PhotonNetwork.LocalPlayer.NickName + " Respawned.");
        }
    }
    
    /// <summary>
    /// Add message to message panel.
    /// </summary>
    /// <param name="message">The message that we want to add.</param>
    void AddMessage(string message)
    {
        photonView.RPC("AddMessage_RPC", RpcTarget.All, message);
    }

    /// <summary>
    /// RPC function to call add message for each client.
    /// </summary>
    /// <param name="message">The message that we want to add.</param>
    [PunRPC]
    void AddMessage_RPC(string message)
    {
        messages.Enqueue(message);
        if (messages.Count > messageCount)
        {
            messages.Dequeue();
        }
        messagesLog.text = "";
        foreach (string m in messages)
        {
            messagesLog.text += m + "\n";
        }
    }

    /// <summary>
    /// Callback function when other player disconnected.
    /// </summary>
    public override void OnPlayerLeftRoom(Player other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AddMessage("Player " + other.NickName + " Left Game.");
        }
    }


}
*/
