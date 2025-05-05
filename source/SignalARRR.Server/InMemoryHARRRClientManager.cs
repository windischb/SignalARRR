using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace doob.SignalARRR.Server;

internal class InMemoryHARRRClientManager : IHARRRClientManager {
    private ConcurrentDictionary<string, ClientContext> ClientStore { get; } = new();
    
    public ClientContext Register(HARRR huc, HubCallerContext hubContext) {

        return ClientStore.AddOrUpdate(hubContext.ConnectionId, _ => new ClientContext(huc, hubContext) {
            ConnectedAt = DateTime.UtcNow
        }, (s, cl) => {
            cl.ReconnectedAt.Add(DateTime.UtcNow);
            return cl;
        });

    }

    public ClientContext? UnRegister(string connectionId) {
        return ClientStore.TryRemove(connectionId, out var client) ? client : null;
    }

    public ClientContext? GetClient(string connectionId) {
        return ClientStore.GetValueOrDefault(connectionId);
    }

    public IEnumerable<ClientContext> GetClients() {
        return ClientStore.Values;
    }
}
