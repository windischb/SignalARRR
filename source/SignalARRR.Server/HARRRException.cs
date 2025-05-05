using Microsoft.AspNetCore.SignalR;

namespace doob.SignalARRR.Server;

public class HARRRException(string type, string message) : HubException($"[{type}] {message}") {

    public HARRRException(Exception exception): this(exception.GetBaseException().GetType().FullName ?? "", exception.GetBaseException().Message) {

    }
}
