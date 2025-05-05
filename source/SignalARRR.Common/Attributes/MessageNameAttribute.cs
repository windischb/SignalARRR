namespace doob.SignalARRR.Common.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MessageNameAttribute(string ns) : Attribute {
    public string Name { get; } = ns;
}
