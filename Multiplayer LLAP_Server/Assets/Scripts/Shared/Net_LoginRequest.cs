using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Net_LoginRequest : NetMsg
{
    public Net_LoginRequest()
    {
        OP = NetOP.LoginRequest;
    }
    public string UserEmailorName { set; get; }
    public string Password { set; get; }
 }
