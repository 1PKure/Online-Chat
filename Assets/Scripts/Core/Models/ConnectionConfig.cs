using System;

[Serializable]
public class ConnectionConfig
{
    public ConnectionMode Mode;
    public TransportType TransportType;
    public string IPAddress;
    public int Port;
    public string UserName;
}