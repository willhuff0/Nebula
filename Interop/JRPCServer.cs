using System.Net;
using System.Net.WebSockets;
using System.Text;
using AustinHarris.JsonRpc;
using Nebula.Interop.Services;
using Newtonsoft.Json;

namespace Nebula.Interop;

public class JRPCServer : IDisposable {
    private static CancellationTokenSource cancellation = null!;
    private static HttpListener listener = null!;

    private static readonly string Url = $"http://127.0.0.1:24980";

    public static HttpListenerWebSocketContext Client;

    public static void Start(string authToken) {
        listener = new HttpListener();
        listener.Prefixes.Add(Url);
        listener.Start();

        cancellation = new CancellationTokenSource();
        Task.Run(() => AcceptWebSocketClientAsync(listener, cancellation.Token));
    }

    static readonly object[] Services = {
        new NebulaService(),
        new GraphicsService(),
        new WindowingService()
    };

    private static async Task AcceptWebSocketClientAsync(HttpListener listener, CancellationToken token) {
        while (!token.IsCancellationRequested && Client == null) {
            try {
                var context = await listener.GetContextAsync();
                if (!context.Request.IsWebSocketRequest) {
                    HttpListenerResponse response = context.Response;

                    byte[] buffer = Encoding.UTF8.GetBytes("HTTP NOT ALLOWED");
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 403;

                    Stream output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    
                    output.Close();
                    response.Close();
                }
                else {
                    var ws = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
                    if (ws != null) {
                        Client = ws;
                        Task.Run(() => HandleConnectionAsync(ws.WebSocket, token));
                    }
                }
            }
            catch (Exception) { }
        }
    }

    private static async Task HandleConnectionAsync(WebSocket ws, CancellationToken token) {
        try {
            while(ws.State == WebSocketState.Open && !token.IsCancellationRequested) {
                String message = await ReadString(ws).ConfigureAwait(false);
                if (message.Contains("method")) {
                    string returnString = await JsonRpcProcessor.Process(message);
                    if (returnString.Length > 0) {
                        ArraySegment<byte> outputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(returnString));
                        if (ws.State == WebSocketState.Open) await ws.SendAsync(outputBuffer, WebSocketMessageType.Text, true, token).ConfigureAwait(false);
                    }
                }
            }

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Success", CancellationToken.None);
        }
        catch {
            try {
                await ws.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error", CancellationToken.None);
            }
            catch {}
        }
        finally {
            Client = null;
            ws.Dispose();
            
            cancellation = new CancellationTokenSource();
            Task.Run(() => AcceptWebSocketClientAsync(listener, cancellation.Token));
        }
    }

    private static async Task<string> ReadString(WebSocket ws) {
        ArraySegment<Byte> buffer = new ArraySegment<byte>(new Byte[8192]);
        WebSocketReceiveResult result;
        using (var ms = new MemoryStream()) {
            do {
                result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                await ms.WriteAsync(buffer.Array!, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);

            using (var reader = new StreamReader(ms, Encoding.UTF8)) return await reader.ReadToEndAsync();
        }
    }

    public static async Task Notify(string rpcMethod, object rpcParams) {
        if (Client == null) return;

        JsonNotification request = new JsonNotification(rpcMethod, rpcParams);
        string notification = JsonConvert.SerializeObject(request);

        ArraySegment<byte> outputBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(notification));
        if (Client.WebSocket.State == WebSocketState.Open) {
            try {
                await Client.WebSocket.SendAsync(outputBuffer,WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch {}
        }
    }

    public void Dispose()
    {
        try {
            if (cancellation != null) {
                cancellation.Cancel();
                cancellation = null;
            }
            if (listener != null) {
                listener.Stop();
                listener = null;
            }
        }
        catch {}
    }
}

internal class JsonNotification
{
    public JsonNotification(string Method, object Params) {
        this.Method = Method;
        this.Params = Params;
    }

    [JsonProperty("jsonrpc")]
    public string JsonRpc => "2.0";

    [JsonProperty("method")]
    public string Method { get; set; }

    [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
    public object Params { get; set; }
}