using System;
using System.Collections.Generic;
using System.Linq;
using AustinHarris.JsonRpc;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Nebula.Interop;

public class Interop {
    private static readonly object[] _jsonRpcServices;
    private static WebSocketServer server;

    static Interop() {
        _jsonRpcServices = new object[] { new EngineService() };
    }

    public static void StartInterop() {
        server = new WebSocketServer(20433);
        server.AddWebSocketService<JsonRpc>("/json-rpc");
        server.Start();
    }

    public static void DisposeInterop() {
        server.Stop();
    }

    public void SetProperty(string nodePath, string propertyPath, dynamic value) {
        Nebula.activeWindow.model.transform.Position = new Vector3(value[0], value[1], value[3]);
    }
    public dynamic GetProperty(string nodePath, string propertyPath) {
        return Nebula.activeWindow.model.transform.Position;
    }
}

public class JsonRpc : WebSocketBehavior {
    protected override void OnMessage(MessageEventArgs e) {
        var asyncState = new JsonRpcStateAsync(ar => {
            string responseString = ((JsonRpcStateAsync)ar).Result;
            if (!string.IsNullOrWhiteSpace(responseString)) {
                Send(responseString);
            }
        }, null);

        asyncState.JsonRpc = e.Data;
        JsonRpcProcessor.Process(asyncState);
    }
}

public class EngineService : JsonRpcService {
    [JsonRpcMethod("setProperty")]
    public void SetProperty(string nodePath, string propertyPath, object _value) {
        var value = ((JArray)_value).ToObject<List<double>>().Select(e => Convert.ToSingle(e)).ToArray();
        Nebula.activeWindow.model2.transform.Position = new Vector3(value[0], value[1], value[2]);
    }

    [JsonRpcMethod("getProperty")]
    public dynamic GetProperty(string nodePath, string propertyPath) {
        return Nebula.activeWindow.model.transform.Position;
    }
}