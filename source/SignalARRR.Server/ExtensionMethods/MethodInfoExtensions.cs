using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace doob.SignalARRR.Server.ExtensionMethods;

public static class MethodInfoExtensions {

    public static List<AuthorizeAttribute> GetAuthorizeData(this MethodInfo methodInfo) {

        var authorizeData = methodInfo.GetCustomAttributes<AuthorizeAttribute>().ToList();

        if (!authorizeData.Any()) {
            var declaringType = methodInfo.DeclaringType;
            if (declaringType != null) {
                authorizeData = declaringType.GetCustomAttributes<AuthorizeAttribute>().ToList();
            }
        }

        return authorizeData;
    }

}
