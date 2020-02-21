using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{
    private byte reliableChannel;
    private int hostId;
    private int webHostId;

    private bool isStarted = false;
    private byte error;

    private const int MAX_USER = 100;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const int BYTE_SIZE = 1024;

    private Mongo db;

    private Dictionary<int, byte> usersPlatform = new Dictionary<int, byte>();

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Init();
    }
    private void Update()
    {
        UpdateMessagePump();
    }
    public void Init()
    {
        db = new Mongo();
        db.Init();

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, MAX_USER);

        //서버만 가진 코드
        hostId = NetworkTransport.AddHost(topo, PORT, null);
        webHostId = NetworkTransport.AddWebsocketHost(topo, WEB_PORT, null);

        Debug.Log(string.Format("Opening Connection on port {0} and webport {1}", PORT, WEB_PORT));
        isStarted = true;

        //db 테스트
        db.InsertAccount("ak", "dlrj", "dlaemfek@naver.com");
    }
    public void Shutdown()
    {
        isStarted = false;
        NetworkTransport.Shutdown();
    }

    private void UpdateMessagePump()
    {
        if (!isStarted)
            return;

        int recHostId;//웹인지 독립형인지
        int connectionId;//어떤 유저가 이걸 보냈는지
        int channelId;//어떤 채널로 보냈는지

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, BYTE_SIZE, out dataSize, out error);
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected through host {1}", connectionId, recHostId));
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected :(", connectionId));
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);

                OnData(connectionId, channelId, recHostId, msg);
                break;
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network event type");
                break;
        }
    }
    #region OnData
    private void OnData(int cnnId, int channelId, int recHostId, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetOP.None:
                Debug.Log("Unexpected NetOP");
                break;
            case NetOP.CreateAccount:
                CreateAccount(cnnId, channelId, recHostId, (Net_CreateAccount)msg);
                break;
            case NetOP.LoginRequest:
                LoginRequest(cnnId, channelId, recHostId, (Net_LoginRequest)msg);
                break;
        }
    }

    private void CreateAccount(int cnnId, int channelId, int  recHostId, Net_CreateAccount ca)
    {
        Debug.Log(string.Format("{0}, {1}, {2}", ca.Username, ca.Password, ca.Email));

        Net_OnCreateAccount oca = new Net_OnCreateAccount();
        oca.Success = 0;
        oca.Infomation = "Account was created!";

        SendClient(recHostId, cnnId, oca);
    }
    public void LoginRequest(int cnnId, int channelId, int recHostId, Net_LoginRequest Lr)
    {
        Debug.Log(string.Format("{0}, {1}", Lr.UserEmailorName, Lr.Password));

        Net_OnLoginRequest olr = new Net_OnLoginRequest();
        olr.Success = 0;
        olr.Infomation = "Everythig is good";
        olr.Username = "Kim";
        olr.Discriminator = "0000";
        olr.Token = "TOKEN";

        SendClient(recHostId, cnnId, olr);
    }
    #endregion

    #region Send
    public void SendClient(int recHost, int cnnId, NetMsg msg)
    {
        //우리의 데이터가 저장되는 곳
        byte[] buffer = new byte[BYTE_SIZE];

        //충돌된 깨진 데이터가 저장되는 곳
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        if (recHost == 0)
            NetworkTransport.Send(hostId, cnnId, reliableChannel, buffer, BYTE_SIZE, out error);
        else
            NetworkTransport.Send(webHostId, cnnId, reliableChannel, buffer, BYTE_SIZE, out error);
    }
    #endregion
}
