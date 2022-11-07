using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using TMPro;

//Handles network specific functions for multiplayer bicycles
public class NetworkBicycle : NetworkBehaviour {

    public GameObject nameTag;  // try to add to a list
    public GameObject wkg;

    //Section PlayerList
    [SerializeField]
    public TMP_Text LobbyText;
    private Dictionary<ulong, bool> m_ClientsInLobby;
    private string m_UserLobbyStatusText;
    // End Section

    //Network variables can not be nullable, so we have to use a fixed string
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    private NetworkVariable<FixedString128Bytes> playerLobbyId = new NetworkVariable<FixedString128Bytes>();
    private NetworkVariable<FixedString32Bytes> playerWKG = new NetworkVariable<FixedString32Bytes>();


    private void Start()
    {
        LobbyText = GameObject.FindGameObjectWithTag("PLStart").GetComponent<TMP_Text>();
    }

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;
        if (IsOwner)
        {
            SetPlayerNameServerRpc(PlayerPrefs.GetString("BikerName", "Unnamed Player"));
            SetPlayerLobbyIdServerRpc(LobbyManager.singleton.GetCurPlayerId());
      /*  if (IsLocalPlayer)
        {
            SetPlayerNameServerRpc(PlayerPrefs.GetString("BikerName", "Unnamed Player"));
            SetPlayerLobbyIdServerRpc(LobbyManager.singleton.GetCurPlayerId());
        }*/

        } else {
            SetNameTag(playerName.Value.ToString());
            SetWattKG(playerWKG.Value.ToString());
           


        }
        m_ClientsInLobby = new Dictionary<ulong, bool>();

        //Always add ourselves to the list at first
        m_ClientsInLobby.Add(NetworkManager.LocalClientId, false);

        //If we are hosting, then handle the server side for detecting when clients have connected
        //and when their lobby scenes are finished loading.
        if (IsServer)
        {
            //Server will be notified when a client connects
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;
            UpdateAndCheckPlayersInLobby();
        }
        //Update our lobby
        GenerateUserStatsForLobby();

    }

    public override void OnNetworkDespawn()
    {
      //  var playerId = LobbyManager.singleton.GetCurPlayerId();
      //  NetPlayerList.Instance.players.Remove(playerId);
        // remove player from UI
      //  Debug.Log("Removed: ");
    }

    [ServerRpc]
    public void SetPlayerNameServerRpc(string name)
    {
        playerName.Value = name;
    }

    [ServerRpc]
    public void SetPlayerLobbyIdServerRpc(string id)
    {
        playerLobbyId.Value = id;
    }


    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {

        SetNameTag(playerName.Value.ToString());

    }

    private void SetNameTag(string name)
    {
        if (nameTag == null) {
            return;
        }
        nameTag.GetComponent<TextMeshPro>().text = name;
        
    }

    public void SetWattKG(string _wkg)
    {
        if (wkg == null)
        {
            return;
        }
        wkg.GetComponent<TextMeshPro>().text = playerWKG.Value.ToString();
        //Debug.Log("MoreValue" + playerWKG.Value.ToString());
    }

    private void Update()
    {   
        if (IsOwner)
        {
            var bikeUI = GameObject.FindGameObjectWithTag("BikeComputer").GetComponent<BikeComputerUI>();
            SetWattKGServerRpc(bikeUI.wkg.text);
           // Debug.Log("UpdatedValue" + playerWKG.Value.ToString());
            
        }
        SetWattKG(playerWKG.Value.ToString());

        if (LobbyText == null)
        {
            LobbyText = GameObject.FindGameObjectWithTag("PLStart").GetComponent<TMP_Text>();
        }
    }


    [ServerRpc]
    public void SetWattKGServerRpc(string _wkg)
    {
        playerWKG.Value = _wkg;
      //  Debug.Log("We ve got value" + _wkg);
    }




    //Section for PlayerList:
    private void OnGUI()
    {
        if (LobbyText != null) LobbyText.text = m_UserLobbyStatusText;
    }
    private void GenerateUserStatsForLobby()
    {
        m_UserLobbyStatusText = string.Empty;

        foreach (var clientLobbyStatus in m_ClientsInLobby)
        {
            /*
            if (IsLocalPlayer)
            {
                 m_UserLobbyStatusText += PlayerPrefs.GetString("BikerName") + "\n";
            }*/

           //    m_UserLobbyStatusText += nameTag.GetComponent<TextMeshPro>().text + "\n" + OwnerClientId.ToString() ;
           //  m_UserLobbyStatusText += playerName.Value.ToString() + playerLobbyId.Value.ToString();
            m_UserLobbyStatusText += $"{clientLobbyStatus.Key}: {clientLobbyStatus.Value}\n"

        }
    }
    /// <summary>
    ///     UpdateAndCheckPlayersInLobby
    ///     Checks to see if we have at least 2 or more people to start
    /// </summary>
    private void UpdateAndCheckPlayersInLobby()
    {
        foreach (var clientLobbyStatus in m_ClientsInLobby)
        {
            SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key); // clientLobbyStatus.Value);
        }
    }

    /// <summary>
    ///     OnClientConnectedCallback
    ///     Since we are entering a lobby and Netcode's NetworkManager is spawning the player,
    ///     the server can be configured to only listen for connected clients at this stage.
    /// </summary>
    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Add(clientId, false);

            GenerateUserStatsForLobby();
            UpdateAndCheckPlayersInLobby();
        }
    }

    private void OnClientDisconnectCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Remove(clientId);
            Debug.Log("Removed: " + clientId);
            m_UserLobbyStatusText = "";
            GenerateUserStatsForLobby();
            UpdateAndCheckPlayersInLobby();
        }
    }

    /// <summary>
    ///     SendClientReadyStatusUpdatesClientRpc
    ///     Sent from the server to the client when a player's status is updated.
    ///     This also populates the connected clients' (excluding host) player state in the lobby
    /// </summary>
    /// <param name="clientId"></param>
    [ClientRpc]
    //  private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
    private void SendClientReadyStatusUpdatesClientRpc(ulong clientId)
    {
        if (!IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId))
                m_ClientsInLobby.Add(clientId, false);
            GenerateUserStatsForLobby();
        }
    }
    //EndSection


    private void OnDestroy() {
        if (IsServer) {
            LobbyService.Instance.RemovePlayerAsync(LobbyManager.singleton.GetCurLobby().Id, playerLobbyId.Value.ToString());
        }
        if (IsOwner) {
            LobbyManager.singleton.Shutdown(true);
        }
    }
}
