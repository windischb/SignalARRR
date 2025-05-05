namespace doob.SignalARRR.Common.Helper;

public class TypeHelper {

    private static Dictionary<string, Type> TypeFromString { get; } = new();
    private static object TypeFromStringLock { get; } = new();

    public static Type FindType(string typeName) {

        if (string.IsNullOrWhiteSpace(typeName))
            return typeof(void);


        lock (TypeFromStringLock) {
            if (TypeFromString.TryGetValue(typeName, out var type))
                return type;

            Type? foundType = null;

            if (!typeName.Contains(".")) {
                foundType = Type.GetType($"System.{typeName}", false, true);
            }

            if (foundType == null) {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies) {

                    foundType = assembly.GetType(typeName, false, false);
                    if (foundType != null) {
                        break;
                    }

                }

                if (foundType == null) {
                    foreach (var assembly in assemblies) {
                        foundType = assembly.GetType(typeName, false, true);
                        if (foundType != null) {
                            break;
                        }
                    }
                }
            }

            if (foundType != null) {
                TypeFromString.Add(typeName, foundType);
            }

            return foundType!;
        }

    }

}
