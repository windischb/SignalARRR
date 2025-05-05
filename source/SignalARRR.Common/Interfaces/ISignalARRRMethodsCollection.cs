using System.Reflection;

namespace doob.SignalARRR.Common.Interfaces;

public interface ISignalARRRMethodsCollection {
    void AddMethod(string name, MethodInfo methodInfo);
    void AddMethod(string name, MethodInfo methodInfo, object instance);
    void AddMethod<T>(string name, MethodInfo methodInfo, Func<IServiceProvider, T> factory = null);

    (Delegate Factory, MethodInfo MethodInfo) GetMethodInformation(string name);
}
