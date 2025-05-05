namespace doob.SignalARRR.Common;

public record  ClientRequestMessage(string Method, object[]? Arguments = null, string[]? GenericArguments = null, string? Authorization = null ) {

    //public string? Authorization { get; set; }
    //public object[]? Arguments { get; set; }
    //public string[]? GenericArguments { get; set; }

    //public ClientRequestMessage(string methodName, IEnumerable<object> arguments) : this(methodName) {
    //    Arguments = arguments.ToArray();
    //}

    //public ClientRequestMessage(string methodName, params object[] arguments) : this(methodName, arguments.ToList()) {

    //}

    //public ClientRequestMessage WithAuthorization(string authorization) {
    //    Authorization = authorization;
    //    return this;
    //}

    //public ClientRequestMessage WithAuthorization(Func<Task<string>> authorization) {
    //    Authorization = authorization?.Invoke().GetAwaiter().GetResult();
    //    return this;
    //}

}
