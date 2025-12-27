// Pseudocode / Plan (detailed):
// 1. Validate input joinCode (not null/empty/whitespace).
// 2. Validate NetworkManager.Singleton exists.
// 3. Get UnityTransport component and validate it's present.
// 4. Attempt to join a relay allocation via RelayService.Instance.JoinAllocationAsync(joinCode).
//    - Wrap call in try/catch to handle service/network exceptions and log useful messages.
// 5. Validate returned JoinAllocation and nested RelayServer data.
// 6. Attempt to set client relay data on the UnityTransport instance.
//    - Wrap in try/catch and log any failures.
// 7. Start the NetworkManager as a client only if it's neither host nor client.
//    - Check the boolean return value of StartClient() and log success/failure.
// 8. Ensure all error paths return early after logging so caller can observe failure in logs.

// NOTE: This file replaces the existing RelayUtility implementation for improved validation and error handling.

using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayUtility
{
    public RelayUtility()
    {
    }

    public async Task<string> CreateRelay(int maxPlayers)
    {
        maxPlayers -= 1; // Host is not included in allocation
        Debug.Log($"Make players = {maxPlayers}");
        // Create allocation with correct max connections
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

        Debug.Log($"allocation {allocation}");

        // Get join code for other players
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log($"Created Relay with join code: {joinCode}");

        // Set relay data on transport
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Cannot set host relay data.");
            return string.Empty;
        }



        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (transport != null)
        {
            try
            {
                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set host relay data: {ex}");
                return string.Empty;
            }
        }
        else
        {
            Debug.LogError("UnityTransport component not found on NetworkManager!");
            return string.Empty;
        }
        return joinCode;
    }

        public async Task JoinRelay(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                Debug.LogError("Join code cannot be null, empty, or whitespace.");
                return;
            }

            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("NetworkManager.Singleton is null. Ensure a NetworkManager exists in the scene.");
                return;
            }

            // Get transport early and validate
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component not found on NetworkManager!");
                return;
            }

            JoinAllocation joinAllocation;
            try
            {
                // Join allocation using the join code
                joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                Debug.Log($"Joined Relay with code: {joinCode}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to join relay with code '{joinCode}': {ex}");
                return;
            }

            if (joinAllocation == null)
            {
                Debug.LogError("JoinAllocation returned null. Cannot proceed.");
                return;
            }

            if (joinAllocation.RelayServer == null)
            {
                Debug.LogError("JoinAllocation.RelayServer is null. Invalid allocation data.");
                return;
            }

            // Set relay data on transport
            try
            {
                transport.SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to set client relay data: {ex}");
                return;
            }

        // Start as client only if not already host or client
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsClient)
        {
            bool started;
            try
            {
                started = NetworkManager.Singleton.StartClient();
            }
            catch (Exception ex)
            {
                Debug.LogError($"NetworkManager.StartClient threw an exception: {ex}");
                return;
            }

            if (started)
            {
                Debug.Log("Started as Client");
            }
            else
            {
                Debug.LogError("NetworkManager failed to start client. Check NetworkManager configuration and logs.");
            }
        }
        else
        {
            Debug.Log("NetworkManager is already running as host or client; not attempting to StartClient().");
        }
    }
}
