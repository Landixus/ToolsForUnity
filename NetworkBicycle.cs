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

    public int sprintId;
    public SprintKomManager sprintKomManager;
    private Sprint sprint;

    //Network variables can not be nullable, so we have to use a fixed string
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();
    private NetworkVariable<FixedString128Bytes> playerLobbyId = new NetworkVariable<FixedString128Bytes>();
    private NetworkVariable<FixedString32Bytes> playerWKG = new NetworkVariable<FixedString32Bytes>();


    private void Start()
    {
       
    }

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;
          if (IsOwner)
         // if (IsServer)
        {

            SetPlayerNameServerRpc(PlayerPrefs.GetString("BikerName", "Unnamed Player"));
            SetPlayerLobbyIdServerRpc(LobbyManager.singleton.GetCurPlayerId());


        } else
        {
            SetNameTag(playerName.Value.ToString());
            SetWattKG(playerWKG.Value.ToString());
        }
      //  Debug.Log("Spawned Bicycle and added " + this.OwnerClientId + " Key, and " + playerName.Value + " as value");
      //  NetPlayerList.instance.players.Add(this.OwnerClientId, this);
      //  PlayerListManager.instance.AddPlayerToList();
        Debug.Log("Added to List called");
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            sprint = sprintKomManager.getSprintFromId(sprintId);
            sprintKomManager.RemoveSprint(sprint);
        }

        playerName.OnValueChanged -= OnPlayerNameChanged;
        //  var playerId = LobbyManager.singleton.GetCurPlayerId();
      //    NetPlayerList.instance.players.Remove(pl;
        // remove player from UI
          Debug.Log("Removed: " + this.OwnerClientId);
    }

    
    [ServerRpc]
    public void SetPlayerNameServerRpc(string name)
    {
        Debug.Log(playerName.Value + " changed to " + name);
        playerName.Value = name;
    }


    [ServerRpc]
    public void SetPlayerLobbyIdServerRpc(string id)
    {
        playerLobbyId.Value = id;
    }

   
  
    //SetPlayerLobbyNameServerRpc


    private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {

        SetNameTag(newValue.ToString());
       // SetNameTag(playerName.Value.ToString());

    }

    private void SetNameTag(string name)
    {
        if (nameTag == null)
        {
            Debug.Log("nametag was null");
            return;
        }
        nameTag.GetComponent<TextMeshPro>().text = name;
        Debug.Log("Nametag changed to " + name);

    }
    /*
    private void SetNameTag(string name)
    {
        if (nameTag == null) {
            return;
        }
        nameTag.GetComponent<TextMeshPro>().text = name;
        
    }*/

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
        if (IsHost) {
            LobbyService.Instance.RemovePlayerAsync(LobbyManager.singleton.GetCurLobby().Id, playerLobbyId.Value.ToString());
        }
        if (IsOwner) {
            LobbyManager.singleton.Shutdown(true);
        }
    }
}
