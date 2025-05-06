using System.Dynamic;
using System.Threading.Channels;
using doob.Reflectensions.ExtensionMethods;
using TaskExtensions = doob.Reflectensions.ExtensionMethods.TaskExtensions;

namespace doob.SignalARRR.ProxyGenerator;

/// <summary>
/// A dynamic proxy that forwards calls on an interface <typeparamref name="T"/>
/// to a SignalR-based communication channel via a helper.
/// Implements DynamicObject to intercept member invocations at runtime.
/// </summary>
public class SignalARRRDynamicProxy<T> : DynamicObject {
    private readonly ProxyCreatorHelper _classCreatorHelper;

    /// <summary>
    /// Initializes a new instance of <see cref="SignalARRRDynamicProxy{T}"/>.
    /// </summary>
    /// <param name="classCreatorHelper">
    /// Helper responsible for sending/invoking hub methods under the hood.
    /// </param>
    public SignalARRRDynamicProxy(ProxyCreatorHelper classCreatorHelper) {
        _classCreatorHelper = classCreatorHelper;
    }

    /// <summary>
    /// Intercepts calls to any method on T and redirects them to the SignalR hub.
    /// Handles void, Task, Task&lt;TResult&gt;, streaming and synchronous return types.
    /// </summary>
    /// <param name="binder">Provides the invoked method name and type arguments.</param>
    /// <param name="args">Arguments passed to the method.</param>
    /// <param name="result">The result to return to the caller.</param>
    /// <returns>True if the invocation was handled; otherwise false.</returns>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result) {
        // 1. Extract generic type arguments and actual parameter types
        var argumentTypes = binder.Reflect()
                                 .GetPropertyValue<Type[]>("TypeArguments")!;
        var parameterTypes = args?
            .Where(a => a != null)
            .Select(a => a!.GetType())
            .ToArray()
            ?? new Type[0];

        // 2. Find matching method(s) on T by name and generic arity
        var methods = typeof(T).GetMethods().ToList()
                                 .WithName(binder.Name)
                                 .Where(m => m.HasGenericArgumentsLengthOf(argumentTypes.Length))
                                 .ToList();

        // 3. Validate that exactly one method matches
        if (methods.Count == 0) {
            throw new Exception($"No matching Methods with Name '{binder.Name}' found!");
        }
        if (methods.Count > 1) {
            throw new Exception($"Multiple matching Methods with Name '{binder.Name}' found!");
        }

        var methodInfo = methods.First();

        // Prepare metadata for sending/invoking
        var methodName    = $"{typeof(T).FullName}|{methodInfo.Name}"; // unique method key
        var isVoid        = methodInfo.ReturnType == typeof(void);
        var isTask        = methodInfo.ReturnType == typeof(Task);
        var isTaskOfT     = methodInfo.ReturnType.IsGenericTypeOf(typeof(Task<>));
        var methodParams  = methodInfo.GetParameters()
                                      .WithoutAttribute("FromServicesAttribute");
        var cancellationToken = args?
            .Where(a => a is CancellationToken)
            .Cast<CancellationToken>()
            .FirstOrDefault()
            ?? default;
        var isStreamingMethod = IsStreamingType(methodInfo.ReturnType);
        var genericArguments  = argumentTypes.Select(a => a.FullName!).ToArray();

        // 4. Handle void return (fire-and-forget)
        if (isVoid) {
            _classCreatorHelper.Send(methodName, args!, genericArguments, cancellationToken);
            result = null;
            return true;
        }
        // 5. Handle Task (no result)
        else if (isTask) {
            result = _classCreatorHelper.SendAsync(methodName, args!, genericArguments, cancellationToken);
            return true;
        }
        // 6. Handle streaming methods (IAsyncEnumerable<>, ChannelReader<>, IObservable<>)
        else if (isStreamingMethod.IsStreamingType) {
            var stream = _classCreatorHelper
                .StreamAsync<T>(methodName, args!, genericArguments, cancellationToken)
                .GetAwaiter()
                .GetResult();

            switch (isStreamingMethod.StreamingType) {
                case StreamingType.ChannelReader:
                    result = _classCreatorHelper.ToChannelReader(stream, cancellationToken);
                    return true;

                case StreamingType.AsyncEnumerable:
                    result = stream;
                    return true;

                case StreamingType.Observable:
                    result = AsyncEnumerable.ToObservable(stream);
                    return true;

                default:
                    // Falls aus irgendeinem Grund doch kein bekannter Streaming-Typ
                    throw new NotSupportedException(
                        $"Unsupported streaming type: {isStreamingMethod.StreamingType}"
                    );
            }
        }
        else if (isTaskOfT) {
            var returnType = methodInfo.ReturnType.GetGenericArguments()[0];
            // Call the generic helper InvokeAsync<TResult>
            var genericMethod = _classCreatorHelper
                .GetType()
                .GetMethod("InvokeAsync")!
                .MakeGenericMethod(returnType);

            // Invoke and get Task
            var task = (Task)genericMethod.Invoke(
                _classCreatorHelper,
                new object[] { methodName, args!, genericArguments, cancellationToken }
            )!;

            // Cast Task to Task<TResult>
            var castMethod = typeof(TaskExtensions)
                .GetMethod("CastToTaskOf")!
                .MakeGenericMethod(returnType);

            result = castMethod.Invoke(null, new object[] { task });
            return true;
        }
        // 8. Handle synchronous return type TResult
        else {
            var returnType = methodInfo.ReturnType;
            var genericMethod = _classCreatorHelper
                .GetType()
                .GetMethod("Invoke")!
                .MakeGenericMethod(returnType);

            result = genericMethod.Invoke(
                _classCreatorHelper,
                new object[] { methodName, args!, genericArguments, cancellationToken }
            );
            return true;
        }

        // Fallback: falls wirklich keiner der Zweige greift
        result = null;
        return false;
    }

    /// <summary>
    /// Casts a non-generic Task to Task&lt;TResult&gt; by extracting `.Result` after completion.
    /// </summary>
    /// <param name="task">The original Task.</param>
    /// <param name="type">The target result type.</param>
    /// <returns>A Task wrapping the result of the original task.</returns>
    public static object CastToTaskOf(Task task, Type type) =>
        task.ContinueWith(t =>
            t.Reflect()
             .GetPropertyValue("Result")?
             .Reflect()
             .To(type)
        );

    /// <summary>
    /// Detects whether the given type represents a streaming pattern
    /// (IObservable&lt;T&gt;, ChannelReader&lt;T&gt; or IAsyncEnumerable&lt;T&gt;).
    /// </summary>
    /// <param name="type">Return type of a method.</param>
    /// <returns>
    /// A tuple indicating if it's streaming, and which streaming interface.
    /// </returns>
    private static (bool IsStreamingType, StreamingType StreamingType) IsStreamingType(Type type) {
        if (type.IsGenericTypeOf(typeof(IObservable<>))) {
            return (true, StreamingType.Observable);
        }
        if (type.IsGenericTypeOf(typeof(ChannelReader<>))) {
            return (true, StreamingType.ChannelReader);
        }
        if (type.IsGenericTypeOf(typeof(IAsyncEnumerable<>))) {
            return (true, StreamingType.AsyncEnumerable);
        }
        return (false, StreamingType.None);
    }
}
