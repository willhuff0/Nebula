using Nebula.Interop.Services;

namespace Nebula.Interop;

public class Interop {
    public static readonly object[] Services = {
        new NebulaService(),
        new GraphicsService(),
        new WindowingService()
    };

    private static List<object> store = new List<object>();

    public static int Store(object value) {
        int index = store.FindIndex((value) => value == null);
        if (index == -1) {
            store.Add(value);
            return store.Count - 1;
        }
        store[index] = value;
        return index;
    }

    public static T Retrieve<T>(int key) {
        var value = store[key];
        if (!value.GetType().IsAssignableFrom(typeof(T))) throw new InvalidCastException($"Tried to retrieve value of type {typeof(T)}; got {value.GetType().Name}.");
        return (T)value;
    }

    public static void Free(int key) => store[key] = null;
}