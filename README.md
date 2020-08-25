# RSGNetworkingLibrary
Networking Library for Unity 3D
Performance is not a key factor at the moment, getting reliable communication across the network is the key purpose of this library. Optimization will come over time when the library becomes more stable

Support's

TCP & RPC communication
UDP & RUDP will be added in the future.


Start a Server/Client
```
private void Start()
{
    RSGNetwork.ServerIPAddress = serverIp;
    RSGNetwork.PortNumber = serverPort;

    switch (networkType)
    {
        case NetworkType.SERVER:
            RSGNetwork.StartServer();          
            break;

        case NetworkType.CLIENT:
            RSGNetwork.ConnectToServer();
            break;
    }
}
```


//Send an RPC to the server, with no parameters
```
NetworkView.ServerRPC("Login");
```

//Send an RPC to the server with parameters
```
NetworkView.ServerRPC("Login", inputField.text);
```
//Send RPC to client, The Clients GUID's can be found in a lookup in RSGNetwork, these are generated when new clients connect to the server
```
RSGNetwork.GetClient(GUID).RPC("AuthenticateLogin", (int)LoginRequest.Success);
```
//Send RPC to Server with a callback with custom data types
```
RSGNetwork.ServerRPCCallback("Login","LoginResultCallback","USERNAME:PASSWORD");


LoginResult Login(string user)
{
    return new LoginResult(user);
}

void LoginResultCallback(LoginResult result)
{
    switch(result.LoginCode)
    {
        case LoginCode.Success:
        //Handle a successful login
        break;
        
        case LoginCode.Failed:
        //Handle a failed Login
        break;        
    }
}


class LoginResult
{
    public LoginResult(user)
    {
        //Validate Login
        if(validation success)
        {
            loginCode = LoginCode.Success;
            int level = 1;
            //return back all other useful User information
        }
        else
        {
            loginCode = LoginCode.Failed;
            int level = 0;
        }        
    }
    

    public enum LoginCode
    {
        SUCCESS,
        FAILED
    }
    
    public LoginCode loginCode;
    public int level;   
}
```


You need to mark methods with RPC Attribute, Their will be conflicting namespaces with unity, so add this to the top of your script
using RPC = RealSoftGames.Network.RPC;

otherwise you will need to put this as the RPC itself
```
[RealSoftGames.Network.RPC]
```
```
[RPC]
public void Login(string GUID, string user = "")
{
    Debug.Log($"Client:{clientID} called Login for user {user}");
    if (RSGNetwork.IsServer)
    {
        if (string.Equals(user, "12345"))
            RSGNetwork.GetClient(clientID).RPC("AuthenticateLogin", (int)LoginRequest.Success);
        else
            RSGNetwork.GetClient(clientID).RPC("AuthenticateLogin", (int)LoginRequest.Failed);
    }
}
```

RPC Supports both Static and Instanced types

Instance Type RPC's need to be added to also contain a NetworkView component and all RPC's for that object need to be assigned to the NetworkView Components array

1 time initialization for Static references, Instanced methods are initialized in Awake.

Support for cross scene network communication, yes you can have multiple clients in another scene all communicating with the server this is what the library is specifically designed for. However their are issues if you use an instanced RPC from 1 client to another. You can not invoke an RPC over the network from 1 scene to another if the recieving client does not have an object that can recieve that RPC


//IObservable is not yet ready and will be available in a future update as its next on ym list to complete
Sync Variables reliably by implementing IObservable interface and assigning it to the NetworkView
```
public class Sync : MonoBehaviour, IObservable
{
    private float xAxis;
    private float yAxis;

    public void OnSerializeView(Stream stream)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(xAxis);
            stream.SendNext(yAxis);
        }
        else
        {
            xAxis = stream.RecieveNext<float>();
            yAxis = stream.RecieveNext<float>();
        }
    }
 }
```


Join us on Discord: https://discord.gg/AUrh5Xd
