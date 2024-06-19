using System;
using Netick;

namespace Dissonance.Integrations.Netick
{
    public readonly struct NetickPeer : IEquatable<NetickPeer>
    {
        public readonly NetworkPlayer PlayerRef;
        public readonly bool IsLoopback;

        public NetickPeer(NetworkPlayer playerRef, bool loopback)
        {
            PlayerRef = playerRef;
            IsLoopback = loopback;
        }

        public bool Equals(NetickPeer other)
        {
            return PlayerRef == other.PlayerRef
                && IsLoopback == other.IsLoopback;
        }

        public override bool Equals(object obj)
        {
            return obj is NetickPeer other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return PlayerRef.GetHashCode()
                    + (IsLoopback ? 1 : 0);
            }
        }
    }
}

