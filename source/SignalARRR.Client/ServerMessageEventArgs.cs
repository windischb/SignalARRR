using doob.SignalARRR.Common;

namespace doob.SignalARRR.Client;

public class ServerRequestEventArgs(ServerRequestMessage serverRequestMessage) : EventArgs {

    public ServerRequestMessage ServerRequestMessage { get; } = serverRequestMessage;
}
