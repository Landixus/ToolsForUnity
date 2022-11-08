using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetPlayerList : NetworkBehaviour
{
    public static NetPlayerList instance { get; private set; }

    [SerializeField]
    public TMP_Text LobbyText;

    private Dictionary<ulong, bool> m_ClientsInLobby;
    private string m_UserLobbyStatusText;

    public Dictionary<ulong, NetworkBicycle> players;


    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
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
    private void OnGUI()
    {
        if (LobbyText != null) LobbyText.text = m_UserLobbyStatusText;
    }
    private void GenerateUserStatsForLobby()
    {
        m_UserLobbyStatusText = string.Empty;

        foreach (var clientLobbyStatus in m_ClientsInLobby)
        {

            m_UserLobbyStatusText += $"{clientLobbyStatus.Key}: {clientLobbyStatus.Value}\n" + players.Values;
            /*
            if (IsLocalPlayer)
            {
                 m_UserLobbyStatusText += PlayerPrefs.GetString("BikerName") + "\n";
            }*/

            //  m_UserLobbyStatusText += networkBicyle.nameTag.GetComponent<TextMeshPro>().text;

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

}
