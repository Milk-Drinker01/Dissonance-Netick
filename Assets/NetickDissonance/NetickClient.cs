using Dissonance.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Integrations.Netick
{
    public class NetickClient : BaseClient<NetickServer, NetickClient, NetickPeer>
    {
        [NotNull] private readonly NetickCommsNetwork _network;

        public NetickClient([NotNull] NetickCommsNetwork network)
            : base(network)
        {
            _network = network;
        }

        public override void Connect()
        {
            Connected();
        }

        protected override void ReadMessages()
        {
            _network.ReadClientMessages(this);
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            _network.SendToServer(packet, true);
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            _network.SendToServer(packet, false);
        }
    }
}