using System.Net.Sockets;

[System.Serializable]
public class SendState
{
    public byte[] dataToSend;
    public int dataSent = 0;
    public Socket socket;
}