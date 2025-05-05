namespace SignalARRR.Tests.SharedModels;

public interface ITestServerMethods {

    string GetName();

    Task<string> GetNameAsync();

    Guid GetGuid();
    Task<Guid> GetGuidAsync();

    void Nothing();

    Task NothingAsync();
}
