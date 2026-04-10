public static class ChatTransportFactory
{
    public static IChatTransport Create(TransportType protocol)
    {
        switch (protocol)
        {
            case TransportType.TCP:
                return new TcpChatTransport();

            case TransportType.UDP:
                return new UdpChatTransport();

            default:
                return null;
        }
    }
}