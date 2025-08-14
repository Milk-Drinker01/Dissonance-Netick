using Dissonance;
using Dissonance.Extensions;
using Dissonance.Networking;
using JetBrains.Annotations;
using Netick;
using Netick.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;
using NetworkPlayer = Netick.NetworkPlayer;

namespace Dissonance.Integrations.Netick
{
    public class NetickCommsNetwork : BaseCommsNetwork<NetickServer, NetickClient, NetickPeer, Unit, Unit>
    {
        private bool _sentNetworkRunnerWarning;

        private readonly Queue<byte[]> _byteArrayPool = new Queue<byte[]>();
        private readonly Queue<(NetickPeer, ArraySegment<byte>)> _serverMessageQueue = new Queue<(NetickPeer, ArraySegment<byte>)>();
        private readonly Queue<ArraySegment<byte>> _clientMessageQueue = new Queue<ArraySegment<byte>>();

        protected override NetickClient CreateClient([CanBeNull] Unit connectionParameters)
        {
            return new NetickClient(this);
        }

        protected override NetickServer CreateServer([CanBeNull] Unit connectionParameters)
        {
            return new NetickServer(this);
        }

        protected override void Update()
        {
            CheckRunningMode();

            base.Update();
        }

        private void CheckRunningMode()
        {
            if (IsInitialized)
            {
                if (!NetickCommsNetworkBase.instance.Sandbox.IsRunning)
                {
                    if (Mode != NetworkMode.None)
                        Stop();
                    NetickCommsNetworkBase.Stopped();
                }
                else
                {
                    switch (NetickCommsNetworkBase.instance.Sandbox.StartMode)
                    {
                        case NetickStartMode.Host:
                            if (Mode != NetworkMode.Host)
                            {
                                NetickCommsNetworkBase.Initialize(this);
                                RunAsHost(Unit.None, Unit.None);
                            }
                            break;
                        case NetickStartMode.Client:
                            if (Mode != NetworkMode.Client)
                            {
                                NetickCommsNetworkBase.Initialize(this);
                                RunAsClient(Unit.None);
                            }
                            break;
                        case NetickStartMode.Server:
                            if (Mode != NetworkMode.DedicatedServer)
                            {
                                NetickCommsNetworkBase.Initialize(this);
                                RunAsDedicatedServer(Unit.None);
                            }
                            break;
                    }
                }
            }
        }

        internal void ReadClientMessages(NetickClient client)
        {
            while (_clientMessageQueue.Count > 0)
            {
                var packet = _clientMessageQueue.Dequeue();
                client.NetworkReceivedPacket(packet);
                RecycleBuffer(packet.Array);
            }
        }

        internal void ReadServerMessages(NetickServer server)
        {
            while (_serverMessageQueue.Count > 0)
            {
                var packet = _serverMessageQueue.Dequeue();
                var (peer, data) = packet;

                server.NetworkReceivedPacket(peer, data);
                RecycleBuffer(data.Array);
            }
        }

        internal void SendToServer(ArraySegment<byte> packet, bool reliable)
        {
            if (Server != null)
            {
                _serverMessageQueue.Enqueue((
                    new NetickPeer(NetickCommsNetworkBase.instance.Sandbox.LocalPlayer, true),
                    CopyForLoopback(packet)
                ));
            }
            else
            {
                NetickCommsNetworkBase.SendToServer(this, packet, reliable);
            }
        }

        internal void SendToClient(NetickPeer dest, ArraySegment<byte> packet, bool reliable)
        {
            if (Client != null && dest.IsLoopback)
            {
                _clientMessageQueue.Enqueue(CopyForLoopback(packet));
            }
            else
            {
                if (!NetickCommsNetworkBase.SendToClient(this, dest, packet, reliable))
                    Server?.ClientDisconnected(dest);
            }
        }

        #region buffer pooling
        private void RecycleBuffer(byte[] buffer)
        {
            if (buffer.Length == 1024)
                _byteArrayPool.Enqueue(buffer);
        }

        private ArraySegment<byte> CopyForLoopback(ArraySegment<byte> packet)
        {
            var array = _byteArrayPool.Count == 0
                      ? new byte[1024]
                      : _byteArrayPool.Dequeue();

            return packet.CopyToSegment(array);
        }
        #endregion

        internal void DeliverMessageToServer(byte[] data, NetworkPlayer source)
        {
            _serverMessageQueue.Enqueue((
                new NetickPeer(source, false),
                new ArraySegment<byte>(data)
            ));
        }

        internal void DeliverMessageToClient(byte[] data)
        {
            _clientMessageQueue.Enqueue(new ArraySegment<byte>(data));
        }

        public void NetickPlayerLeft(NetworkPlayer player)
        {
            Server?.ClientDisconnected(new NetickPeer(player, false));
        }
    }
}