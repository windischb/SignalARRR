using System.Net;
using doob.Reflectensions.ExtensionMethods;
using doob.SignalARRR.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using SignalARRR.Tests.SharedModels;
using Xunit;

namespace SignalARRR.Tests;

[Collection("Simple")]
public class TypedHARRRConnectionTests {

    SignalARRRServerInstanceFixture fixture;
    HARRRConnection harrrConnection;


    public TypedHARRRConnectionTests(SignalARRRServerInstanceFixture fixture) {
        this.fixture = fixture;


        var testServer = this.fixture.GetHost().GetTestServer();

        harrrConnection = HARRRConnection.Create(builder => {
            builder.WithUrl($"{testServer.BaseAddress}signalr/testhub", options => {
                options.HttpMessageHandlerFactory = _ => testServer.CreateHandler();
                options.Proxy = new WebProxy("localhost.:8888");
            });
        });
            
    }

    private async Task<T> GetTypeConnection<T>() where T : class {
        await harrrConnection.StartAsync();
        return harrrConnection.GetTypedMethods<T>();
    }

    [Fact]
    public async Task GetString() {


        var serverMethods = await GetTypeConnection<ITestServerMethods>();
        var name = serverMethods.GetName();

        Assert.Equal("MyName", name);
    }

    [Fact]
    public async Task GetStringAsync() {

        var serverMethods = await GetTypeConnection<ITestServerMethods>();
        var name = await serverMethods.GetNameAsync();

        Assert.Equal("MyNameAsync", name);
    }
    
    [Fact]
    public async Task GetGuid() {

        var serverMethods = await GetTypeConnection<ITestServerMethods>();
        var name = serverMethods.GetGuid();

        Assert.Equal(typeof(Guid), name.GetType());
    }
    
    [Fact]
    public async Task GetGuidAsync() {

        var serverMethods = await GetTypeConnection<ITestServerMethods>();
        var name = await serverMethods.GetGuidAsync();

        Assert.Equal(typeof(Guid), name.GetType());
    }
    
    [Fact]
    public async Task Nothing() {

        var serverMethods = await GetTypeConnection<ITestServerMethods>();
        serverMethods.Nothing();
    }
    
    [Fact]
    public async Task NothingAsync() {

        var serverMethods = await GetTypeConnection<ITestServerMethods>();
        await serverMethods.NothingAsync();
    }
}
