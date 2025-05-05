using doob.SignalARRR.Common;
using doob.SignalARRR.Common.Constants;
using Microsoft.AspNetCore.SignalR;

namespace doob.SignalARRR.Server;

internal class ClientContextDispatcher<T>(IHubContext<T> hubContext) : IClientContextDispatcher where T : HARRR {

    private IHubContext<T> HubContext { get; } = hubContext;

    public Task<TResult> InvokeClientAsync<TResult>(string clientId, ServerRequestMessage serverRequestMessage, CancellationToken cancellationToken) {
        return InvokeClientMessageAsync<TResult>(clientId, MethodNames.InvokeServerRequest, serverRequestMessage, cancellationToken);
    }

    public Task SendClientAsync(string clientId, ServerRequestMessage serverRequestMessage, CancellationToken cancellationToken) {
        return SendClientMessageAsync(clientId, MethodNames.InvokeServerMessage, serverRequestMessage, cancellationToken);
    }

    public async Task<string> Challenge(string clientId) {

        try {
            var msg = new ServerRequestMessage(MethodNames.ChallengeAuthentication);
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            return await InvokeClientMessageAsync<string>(clientId, MethodNames.ChallengeAuthentication, msg, ct.Token);
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
            
    }

    public async Task CancelToken(string clientId, Guid id) {

        try {
            var msg = new ServerRequestMessage(MethodNames.CancelTokenFromServer) { CancellationGuid = id };
            await SendClientMessageAsync(clientId, MethodNames.CancelTokenFromServer, msg, CancellationToken.None);
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }

    }

    internal async Task<TResult> InvokeClientMessageAsync<TResult>(string clientId, string methodName, ServerRequestMessage serverRequestMessage, CancellationToken cancellationToken) {
        return await HubContext.Clients.Client(clientId)
            .InvokeCoreAsync<TResult>(methodName, [serverRequestMessage], cancellationToken);
    }

    internal async Task SendClientMessageAsync(string clientId, string methodName, ServerRequestMessage serverRequestMessage, CancellationToken cancellationToken) {
        await HubContext.Clients.Client(clientId).SendCoreAsync(methodName, [serverRequestMessage], cancellationToken);
    }


}
