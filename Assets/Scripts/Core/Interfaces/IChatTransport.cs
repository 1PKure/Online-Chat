using System;

public interface IChatTransport
{
    event Action<string> OnRawMessageReceived;
    event Action OnConnected;
    event Action OnDisconnected;
    event Action<string> OnError;

    bool IsRunning { get; }

    void StartAsServer(string ip, int port);
    void StartAsClient(string ip, int port);
    void Send(string message);
    void Stop();
}