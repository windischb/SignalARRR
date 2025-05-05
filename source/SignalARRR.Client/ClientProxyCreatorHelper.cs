using doob.Reflectensions.Helper;
using doob.SignalARRR.Common;
using doob.SignalARRR.ProxyGenerator;

namespace doob.SignalARRR.Client;

public class ClientProxyCreatorHelper(HARRRConnection harrrConnection) : ProxyCreatorHelper {
    public override T Invoke<T>(string methodName, IEnumerable<object> arguments, string[] genericArguments, CancellationToken cancellationToken = default) {
        var msg = new ClientRequestMessage(methodName, [..arguments], genericArguments.ToArray());
        return SimpleAsyncHelper.RunSync(() => harrrConnection.InvokeCoreAsync<T>(msg, cancellationToken));
    }

    public override Task<T> InvokeAsync<T>(string methodName, IEnumerable<object> arguments, string[] genericArguments, CancellationToken cancellationToken = default) {
        var msg = new ClientRequestMessage(methodName, [..arguments], genericArguments.ToArray());
        return harrrConnection.InvokeCoreAsync<T>(msg, cancellationToken);
    }


    public override void Send(string methodName, IEnumerable<object> arguments, string[] genericArguments, CancellationToken cancellationToken = default) {
        var msg = new ClientRequestMessage(methodName, [.. arguments], genericArguments.ToArray());
        SimpleAsyncHelper.RunSync(() => harrrConnection.SendCoreAsync(msg, cancellationToken));
    }

    public override Task SendAsync(string methodName, IEnumerable<object> arguments, string[] genericArguments, CancellationToken cancellationToken = default) {
        var msg = new ClientRequestMessage(methodName, [.. arguments], genericArguments.ToArray());
        return harrrConnection.SendCoreAsync(msg, cancellationToken);
    }

    public override async Task<IAsyncEnumerable<TResult>> StreamAsync<TResult>(string methodName, IEnumerable<object> arguments, string[] genericArguments, CancellationToken cancellationToken = default) {
        var msg = new ClientRequestMessage(methodName, [.. arguments], genericArguments.ToArray());
        return await harrrConnection.StreamAsyncCore<TResult>(msg, cancellationToken);
    }
}
