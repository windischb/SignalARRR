using System.Text;
using Microsoft.AspNetCore.Http;

namespace doob.SignalARRR.Server.ExtensionMethods;

public static class HttpContextExtensions {

    public static async Task<string?> GetRawBodyStringAsync(this HttpContext httpContext, Encoding encoding) {

        if (httpContext.Request.ContentLength is not > 0)
            return null;

        using var reader = new StreamReader(httpContext.Request.Body, encoding, true, 1024, true);
        return await reader.ReadToEndAsync();

    }

    public static void ProxyFromHARRRClient<TInterface>(this HttpContext httpContext, ClientContext clientContext,
        Action<TInterface> action) where TInterface : class {

        clientContext.ForwardToHttpContext(httpContext, action);

    }


}
