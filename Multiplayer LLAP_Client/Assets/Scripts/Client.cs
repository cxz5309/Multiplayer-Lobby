using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    public static Client instance { private set; get; }

    private byte reliableChannel;
    private int hostId;
    private int connectionId;
    private byte error;

    private bool isStarted = false;

    private const int MAX_USER = 100;
    private const int PORT = 26000;
    private const int WEB_PORT = 26001;
    private const string SERVER_IP = "127.0.0.1";
    private const int BYTE_SIZE = 1024;

    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }
    private void Update()
    {
        UpdateMessagePump();
    }
    public void Init()
    {
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, MAX_USER);

        //클라이언트만 가진 코드
        hostId = NetworkTransport.AddHost(topo, 0);

#if UNITY_WEBGL && !UNITY_EDITOR
        //웹 클라이언트
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, WEB_PORT, 0, out error);
        Debug.Log("Connection from WEBGL");
#else
        //독립 클라이언트
        connectionId = NetworkTransport.Connect(hostId, SERVER_IP, PORT, 0, out error);
        Debug.Log("Connecting from standalone");
#endif
        Debug.Log(string.Format("Attemping to connect on {0}", SERVER_IP));
        isStarted = true;
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
                Debug.Log("We have connected to the server");
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("We have been disconnected");
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
    #region Send
    public void SendServer(NetMsg msg)
    {
        //우리의 데이터가 저장되는 곳
        byte[] buffer = new byte[BYTE_SIZE];
        //충돌된 깨진 데이터가 저장되는 곳
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        NetworkTransport.Send(hostId, connectionId, reliableChannel, buffer, BYTE_SIZE, out error);
    }
    public void SendCreateAccount(string username, string password, string email)
    {
        Net_CreateAccount ca = new Net_CreateAccount();

        ca.Username = username;
        ca.Password = password;
        ca.Email = email;

        SendServer(ca);
    }
    public void SendLoginRequest(string usernameOrEmail, string password)
    {
        Net_LoginRequest lr = new Net_LoginRequest();

        lr.UserEmailorName = usernameOrEmail;
        lr.Password = password;

        SendServer(lr);
    }
    #endregion

    #region OnData
    private void OnData(int cnnId, int channelId, int recHostId, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetOP.None:
                Debug.Log("Unexpected NetOP");
                break;
            case NetOP.OnCreateAccount:
                OnCreateAccount((Net_OnCreateAccount)msg);
                break;
            case NetOP.OnLoginRequest:
                OnLoginRequest((Net_OnLoginRequest)msg);
                break;
        }
    }
    
    private void OnCreateAccount(Net_OnCreateAccount oca)
    {
        LobbyScene.instance.EnableInputs();
        LobbyScene.instance.ChangeAuthenticationMessage(oca.Infomation);
    }
    private void OnLoginRequest(Net_OnLoginRequest olr)
    {
        LobbyScene.instance.ChangeAuthenticationMessage(olr.Infomation);

        if(olr.Success != 1)
        {
            LobbyScene.instance.EnableInputs();
        }
        else
        {
        }
    }
    #endregion

    public void TEST()
    {
        Net_CreateAccount ca = new Net_CreateAccount();

        ca.Username = "sm";
        ca.Password = "12";
        ca.Email = "cxz5309@naver.com";

        SendServer(ca);
    }
}
