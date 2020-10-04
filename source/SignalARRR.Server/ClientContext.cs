﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Reflectensions.HelperClasses;
using SignalARRR.CodeGenerator;
using SignalARRR.Server.ExtensionMethods;

namespace SignalARRR.Server {

    public class ClientContext {
        public string Id { get; }
        internal Type HARRRType { get; }
        public IPAddress RemoteIp { get;  }
        public ClaimsPrincipal User { get; private set; }
        internal DateTime UserValidUntil { get; private set; } = DateTime.Now;

        public DateTime ConnectedAt { get; internal set; }
        public List<DateTime> ReconnectedAt { get; } = new List<DateTime>();

        internal IServiceProvider ServiceProvider { get; }

        //private string AuthData { get; set; }
        //private IAuthenticator Authenticator { get; }


        public ClientContext(HARRR hub, HubCallerContext hubCallerContext) {
            Id = hubCallerContext.ConnectionId;
            //ServiceProvider = serviceProvider;
            ServiceProvider = hubCallerContext.GetHttpContext().RequestServices;
            User = hubCallerContext.User;
            HARRRType = hub.GetType();
            
            RemoteIp = hubCallerContext.GetHttpContext().Connection.RemoteIpAddress;
            

            foreach (var (key, value) in hubCallerContext.GetHttpContext().Request.Headers)
            {
                if (key.StartsWith("#")) {
                    Attributes[key.Substring(1)] = value;
                }
            }

            foreach (var (key, value) in hubCallerContext.GetHttpContext().Request.Query)
            {
                if (key.StartsWith("@")) {
                    Attributes[key.Substring(1)] = value;
                }
            }
        }

        //internal async Task<bool> TryAuthenticate() {

        //    using var scope = ServiceProvider.CreateScope();


        //    var authenticator = scope.ServiceProvider.GetService<IAuthenticator>();

        //    if (authenticator == null) {
        //        return true;
        //    }

        //    //var authorizeAttribute = methodInfo.GetCustomAttribute<AuthorizeAttribute>();
        //    //HttpContext context = new DefaultHttpContext();
            
           

        //    var auth = await authenticator.TryAuthenticate(AuthData);
        //    if (auth.authenticated) {
        //        User = auth.principal;
        //        return true;
        //    }

        //    User = null;
        //    return false;

        //}

        //internal void SetAuthData(string authdata) {
        //    AuthData = authdata;
        //}

        public ClientAttributes Attributes { get; } = new ClientAttributes();


        internal void SetPrincipal(ClaimsPrincipal claimsPrincipal) {
            this.User = claimsPrincipal ?? new ClaimsPrincipal();

            if (this.User.Identity.IsAuthenticated) {
                this.UserValidUntil = DateTime.Now.Add(TimeSpan.FromMinutes(3));
            } else {
                this.UserValidUntil = DateTime.Now;
            }
            

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


        public T GetTypedMethods<T>(string nameSpace = null) {
            var instance = ClassCreator.CreateInstanceFromInterface<T>(new ServerClassCreatorHelper(this), nameSpace);
            return instance;
        }

        public void ProxyToHttpContext<T>( HttpContext httpContext, string nameSpace, Action<T> action) {
            var instance = ClassCreator.CreateInstanceFromInterface<T>(new ServerClassCreatorProxyHelper(this, httpContext), nameSpace);
            action(instance);
        }

        public void ProxyToHttpContext<T>(HttpContext httpContext, Action<T> action) {
            var instance = ClassCreator.CreateInstanceFromInterface<T>(new ServerClassCreatorProxyHelper(this, httpContext), null);
            action(instance);
        }
    }


    public class ClientAttributes: Dictionary<string, StringValues> {

        public ClientAttributes():base(StringComparer.OrdinalIgnoreCase) {

        }

        public new string this[string key] {
            get => TryGetValue(key, out var val) ? val : default;
            set {

                base[key] = value;
            }
        }

        public bool Has(string key) {
            return ContainsKey(key);
        }

        public bool Has(string key, string value) {
            if (TryGetValue(key, out var val)) {
                return val.Any(v => Wildcard.Match(v, value));
            }

            return false;
        }

    }

}
