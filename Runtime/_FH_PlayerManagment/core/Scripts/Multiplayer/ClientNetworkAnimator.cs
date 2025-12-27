using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ClientNetworkAnimator : NetworkAnimator
{

    public void SetFloat(string name, float value) 
    {
        if (IsOwner)
        {
            SetFloatRpc(name, value);
        }
    }

    public void SetBool(string name, bool value)
    {
        if (IsOwner)
        {
            SetBoolRpc(name, value);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetFloatRpc(string name, float value)
    {
        this.Animator.SetFloat(name, value);
    }

    [Rpc(SendTo.Server)]
    private void SetBoolRpc(string name, bool value)
    {
        this.Animator.SetBool(name, value);
    }

}
