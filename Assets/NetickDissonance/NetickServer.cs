using Dissonance.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Integrations.Netick
{
    public class NetickServer : BaseServer<NetickServer, NetickClient, NetickPeer>
    {
        [NotNull] private readonly NetickCommsNetwork _network;

        public NetickServer([NotNull] NetickCommsNetwork network)
        {
            _network = network;
        }

        protected override void ReadMessages()
        {
            _network.ReadServerMessages(this);
        }

        protected override void SendReliable(NetickPeer connection, ArraySegment<byte> packet)
        {
            _network.SendToClient(connection, packet, true);
        }

        protected override void SendUnreliable(NetickPeer connection, ArraySegment<byte> packet)
        {
            _network.SendToClient(connection, packet, false);
        }

        internal new void ClientDisconnected(NetickPeer connection)
        {
            base.ClientDisconnected(connection);
        }
    }
}
