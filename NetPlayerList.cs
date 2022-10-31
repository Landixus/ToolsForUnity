using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Services.Lobbies.Models;

public class NetPlayerList : MonoBehaviour
{
    public static NetPlayerList singleton;

    public Dictionary<string, NetworkBicycle> players = new Dictionary<string, NetworkBicycle>();
          
}
