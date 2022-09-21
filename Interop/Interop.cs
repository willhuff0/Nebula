namespace Nebula.Interop;

public class Interop {
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

    public static T Retrieve<T>(int index) {
        var value = store[index];
        if (!value.GetType().IsAssignableFrom(typeof(T))) throw new InvalidCastException($"Tried to retrieve value of type {typeof(T)}; got {value.GetType().Name}.");
        return (T)value;
    }
}