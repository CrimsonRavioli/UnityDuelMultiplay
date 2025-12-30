using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class CharacterManager : NetworkBehaviour
{

    const int Inactive_ID = 100;
    public static CharacterManager Instance { get; private set; }

    [Header("Spawn Configuration")]
    [SerializeField] private List<GameObject> _characterPrefabList;
    [SerializeField] private Vector3 spawnAreaMin = new(-10, 0, -10);
    [SerializeField] private Vector3 spawnAreaMax = new(10, 0, 10);
    [SerializeField] private int initialAICharacters = 5;

    private readonly Dictionary<int, NetworkObject> _allVessels = new();
    private readonly Dictionary<Vector2Int, int> _playerVessels = new();

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }



    public override void OnNetworkSpawn()
    {
       
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void CreateCharacterRPC(Vector2Int newOwner,int vessel = -1) 
    {
        CreateCharacter(GetCharacterPrefab(vessel), newOwner );
    }

    private GameObject GetCharacterPrefab(int vessel)
    {

        if (vessel >= 0 && vessel < _characterPrefabList.Count)
        {
            return _characterPrefabList[vessel % _characterPrefabList.Count ];
        }

        return _characterPrefabList[UnityEngine.Random.Range(0, _characterPrefabList.Count)];
    }

    public void CreateCharacter(GameObject characterPrefab, Vector2Int newOwner )
    {
        if (!IsServer) return;
        Vector3 spawnPos = GetRandomSpawnPosition();
        GameObject go = Instantiate(characterPrefab, spawnPos, Quaternion.identity);
       
        NetworkObject no = go.GetComponent<NetworkObject>();

        int vesselId = _allVessels.Keys.Count + 1;

        _allVessels.Add(vesselId, no);
        no.Spawn(true);
       // go.transform.parent = this.transform;
        TransferCharacterRPC(newOwner, vesselId);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            UnityEngine.Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void TransferCharacterRPC(Vector2Int newOwner, int vessel)
    {
        if (!IsValidVessel(vessel)) return;
        if (IsSameOwner(newOwner, vessel)) return;

        ClearPreviousOwner(vessel);
        AssignNewOwner(newOwner, vessel);
        UpdateNetworkOwnership(newOwner, vessel);
        UpdateVesselControllerRPC(_allVessels[vessel].NetworkObjectId, newOwner);
    }

    private bool IsValidVessel(int vessel)
    {
        return _allVessels.ContainsKey(vessel);
    }

    private bool IsSameOwner(Vector2Int newOwner, int vessel)
    {
        Vector2Int oldOwner = GetOwnerOfVessel(vessel);
        return oldOwner == newOwner;
    }

    private void ClearPreviousOwner(int vessel)
    {
        Vector2Int oldOwner = GetOwnerOfVessel(vessel);
        if (oldOwner.x != Inactive_ID)
        {
            _playerVessels[oldOwner] = 0;
        }
    }

    private void AssignNewOwner(Vector2Int newOwner, int vessel)
    {
        if (_playerVessels.ContainsKey(newOwner))
        {
            _playerVessels[newOwner] = vessel;
        }
        else
        {
            _playerVessels.Add(newOwner, vessel);
        }
    }

    private void UpdateNetworkOwnership(Vector2Int newOwner, int vessel)
    {
        if (newOwner.x < 100)
        {
            _allVessels[vessel].ChangeOwnership((ulong)newOwner.x);
        }
        else
        {
            _allVessels[vessel].ChangeOwnership(OwnerClientId);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void UpdateVesselControllerRPC(ulong objectID, Vector2Int controllerID)
    {

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out NetworkObject no);
        no.GetComponent<BaseVesselController>().SetControllerID(controllerID);
    }

    private Vector2Int GetOwnerOfVessel(int vessel)
    {
        if (vessel == 0)
        {
            return GetNextInactiveSlot();
        }

        foreach (var kvp in _playerVessels)
        {
            if (kvp.Value == vessel)
            {
                return kvp.Key;
            }
        }
        return GetNextInactiveSlot();
    }

    private Vector2Int GetNextInactiveSlot() 
    {
        int i = 0;
        while (true)
        {
            Vector2Int slot = new(Inactive_ID,i);
            if (!_playerVessels.ContainsKey(slot))
            {
                return slot;
            }
            if (_playerVessels[slot] == 0)
            {
                return slot;
            }
            i++;
        }
    }
}