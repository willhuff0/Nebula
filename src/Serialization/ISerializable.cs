using Newtonsoft.Json.Linq;

public interface ISerializable {
    public JObject Serialize();
    public void Deserialize(JObject from);
}