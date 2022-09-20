using AustinHarris.JsonRpc;

namespace Nebula.Interop.Services;

public class NebulaService : JsonRpcService {
    [JsonRpcMethod("internal.keepAlive")]
    private string Handle_KeepAlive(string s) => s;
}