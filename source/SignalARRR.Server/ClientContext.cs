using System.Net;
using System.Reflection;
using System.Security.Claims;
using doob.Reflectensions.Common;
using doob.SignalARRR.ProxyGenerator;
using doob.SignalARRR.Server.ExtensionMethods;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;


namespace doob.SignalARRR.Server;

public class ClientContext {
    public string Id { get; }
    internal Type HARRRType { get; }
    public IPAddress RemoteIp { get;  }
    public ClaimsPrincipal User { get; private set; }
    internal DateTime UserValidUntil { get; private set; } = DateTime.Now;

    public DateTime ConnectedAt { get; internal set; }
    public List<DateTime> ReconnectedAt { get; } = new();

    internal IServiceProvider ServiceProvider { get; }

    public Uri ConnectedTo { get; }

    public ClientContext(HARRR hub, HubCallerContext hubCallerContext) {

        var httpContext = hubCallerContext.GetHttpContext()!;

        Id = hubCallerContext.ConnectionId;
        ServiceProvider = httpContext.RequestServices;
        User = hubCallerContext.User ?? new ClaimsPrincipal();
        HARRRType = hub.GetType();
            
        RemoteIp = httpContext.Connection.RemoteIpAddress ?? IPAddress.Any;
        var connectedToBuilder = new UriBuilder(httpContext.Request.GetDisplayUrl()) { Query = null };
        ConnectedTo = connectedToBuilder.Uri;

        foreach (var (key, value) in httpContext.Request.Headers)
        {
            if (key.StartsWith("#")) {
                Attributes[key[1..]] = value.ToString();
            }
        }

        foreach (var (key, value) in httpContext.Request.Query)
        {
            if (key.StartsWith("@")) {
                Attributes[key[1..]] = value.ToString();
            }
        }
    }

    public ClientAttributes Attributes { get; } = new();


    internal void SetPrincipal(ClaimsPrincipal? claimsPrincipal) {
        User = claimsPrincipal ?? new ClaimsPrincipal();

        UserValidUntil = User.Identity?.IsAuthenticated == true ? DateTime.Now.Add(TimeSpan.FromMinutes(3)) : DateTime.Now;
    }

    public async Task<PolicyAuthorizationResult> TryAuthenticate(MethodInfo methodInfo) {
            
        if(!methodInfo.GetAuthorizeData().Any())
            return PolicyAuthorizationResult.Success();

        if (UserValidUntil >= DateTime.Now)
            return PolicyAuthorizationResult.Success();


        var hubContextType = typeof(ClientContextDispatcher<>).MakeGenericType(HARRRType);
        var harrrContext = (IClientContextDispatcher)ServiceProvider.GetRequiredService(hubContextType);
        var res = await harrrContext.Challenge(Id);

        var authentication = new SignalARRRAuthentication(ServiceProvider);
        return await authentication.Authorize(this, res, methodInfo);
    }


    public T GetTypedMethods<T>() where T : class {
        var instance = ProxyCreator.CreateInstanceFromInterface<T>(new ServerProxyCreatorHelper(this, null));
        return instance;
    }

    public void ForwardToHttpContext<T>(HttpContext httpContext, Action<T> action) where T : class {
        var instance = ProxyCreator.CreateInstanceFromInterface<T>(new ServerProxyCreatorHelper(this, httpContext));
        action(instance);
    }
}


public class ClientAttributes() : Dictionary<string, StringValues>(StringComparer.OrdinalIgnoreCase) {
    public new string this[string key] {
        get => TryGetValue(key, out var val) ? val.ToString() : "";
        set {

            base[key] = value;
        }
    }

    public bool Has(string key) {
        return ContainsKey(key);
    }

    public bool Has(string key, string value) {
        return TryGetValue(key, out var val) && val.Any(v => v?.Match(value) == true);
    }

}
