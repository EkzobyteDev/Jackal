using Godot;
using System;
using System.Collections.Generic;

public partial class Communicator : Node
{
    List<long> connectedPeers = new List<long>();
    ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
    [Export] Label label;




    public void StartServer()
    {
        peer.CreateServer(8000, 4);
        peer.PeerConnected += ClientConnected;
        Multiplayer.MultiplayerPeer = peer;

        PrintLabel("Server started");
        PrintLabel("Id = " + Multiplayer.GetUniqueId());
    }
    public void StartClient()
    {
        peer.CreateClient("localhost", 8000);
        Multiplayer.MultiplayerPeer = peer;
        Multiplayer.ConnectedToServer += OnConnectedToServer;

        PrintLabel("Connected to server");
        PrintLabel("Id = " + Multiplayer.GetUniqueId());
    }

    private async void ClientConnected(long id)
    {
        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        connectedPeers.Add(id);

        RpcId(id, nameof(OnGetMessageFromServer), "Hello, " + id);
        foreach (long peerId in connectedPeers)
        {
            if (peerId == id) continue;
            RpcId(peerId, nameof(OnGetMessageFromServer), id + " connected");
        }
    }

    private async void OnConnectedToServer()
    {
        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        RpcId(1, nameof(OnGetMessageFromClient), "Server, please send me data, P.S " + Multiplayer.GetUniqueId());
    }


    [Rpc]
    void OnGetMessageFromServer(string msg)
    {
        PrintLabel(msg);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    void OnGetMessageFromClient(string msg)
    {
        PrintLabel(msg);
    }

    void PrintLabel(string msg)
    {
        label.Text += msg + '\n';
    }
}
