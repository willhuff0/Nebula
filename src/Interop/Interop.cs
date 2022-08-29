using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using StreamJsonRpc;

namespace Nebula.Interop;

public class Interop {
    private static TcpListener server;
    private static List<Interop> clients;

    private TcpClient client;
    private JsonRpc rpc;

    private Interop(TcpClient client, JsonRpc rpc) {
        this.client = client;
        this.rpc = rpc;
    }
    
    public static void StartInterop() {
        server = new TcpListener(IPAddress.Parse("127.0.0.1"), 20433);
        server.Start();
        ListenInterop();
    }
    private static async void ListenInterop() {
        while(true) {
            TcpClient client = await server.AcceptTcpClientAsync();
            JsonRpc rpc = JsonRpc.Attach(client.GetStream());
            Interop interop = new Interop(client, rpc);
            rpc.AddLocalRpcTarget(interop);
        }
    }
    public static void DisposeInterop() {
        server.Stop();
    }

    public void Properties(){
        
    }
}