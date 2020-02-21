using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Net_OnCreateAccount : NetMsg
{
    public Net_OnCreateAccount()
    {
        OP = NetOP.OnCreateAccount;
    }

    public byte Success { set; get; }
    public string Infomation { set; get; }
}


