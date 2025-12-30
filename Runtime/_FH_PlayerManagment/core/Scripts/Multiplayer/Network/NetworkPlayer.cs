using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


public class NetworkPlayer : NetworkBehaviour
{
    public static int OwnerId = 0;
    private void Start()
    {
        if (!IsOwner || !IsSpawned) return;

        foreach (var player in LocalPlayerManager.Instance.LocalPlayerDatas())
        {
            CharacterManager.Instance.CreateCharacterRPC(new Vector2Int(OwnerId, player.PlayerIndex),player.GetValue<int>("Character"));

        }

    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner || !IsSpawned) return;
        OwnerId = (int)OwnerClientId;
    }
}