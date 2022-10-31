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

    

    //Network variables can not be nullable, so we have to use a fixed string
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    private NetworkVariable<FixedString128Bytes> playerLobbyId = new NetworkVariable<FixedString128Bytes>();
    private NetworkVariable<FixedString32Bytes> playerWKG = new NetworkVariable<FixedString32Bytes>();


    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;
        if (IsOwner)
        {
            SetPlayerNameServerRpc(PlayerPrefs.GetString("BikerName", "Unnamed Player"));
            SetPlayerLobbyIdServerRpc(LobbyManager.singleton.GetCurPlayerId());
        if (IsLocalPlayer)
        {
            SetPlayerNameServerRpc(PlayerPrefs.GetString("BikerName", "Unnamed Player"));
            SetPlayerLobbyIdServerRpc(LobbyManager.singleton.GetCurPlayerId());
        }

        } /*else {
            SetNameTag(playerName.Value.ToString());
            SetWattKG(playerWKG.Value.ToString());
     
        }*/
    }

    public override void OnNetworkDespawn()
    {
        var playerId = LobbyManager.singleton.GetCurPlayerId();
        NetPlayerList.singleton.players.Remove(playerId);
        // remove player from UI
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
        Debug.Log("Player OnPlayerNameChanged playerName: " + newValue);
        Debug.Log("Player OnPlayerNameChanged playerId: " + LobbyManager.singleton.GetCurPlayerId());

        //uint playerId = Convert.ToUInt32(LobbyManager.singleton.GetCurPlayerId());
        var playerId = LobbyManager.singleton.GetCurPlayerId();
        NetPlayerList.singleton.players.Add(playerId, this );
        SetNameTag(playerName.Value.ToString());
    }

    /* private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
     {
         Debug.Log("Player OnPlayerNameChanged playerName: " + newValue);

         //NetPlayerList netPlayerList;
        // netPlayerList.ins
         // display tag on UI
     }
     */
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
    }


    [ServerRpc]
    public void SetWattKGServerRpc(string _wkg)
    {
        playerWKG.Value = _wkg;
      //  Debug.Log("We ve got value" + _wkg);
    }

    

    private void OnDestroy() {
        if (IsServer) {
            LobbyService.Instance.RemovePlayerAsync(LobbyManager.singleton.GetCurLobby().Id, playerLobbyId.Value.ToString());
        }
        if (IsOwner) {
            LobbyManager.singleton.Shutdown(true);
        }
    }
}
