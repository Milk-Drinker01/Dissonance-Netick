using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netick;
using Netick.Unity;
using JetBrains.Annotations;
using System;
using Dissonance.Extensions;
using NetworkPlayer = Netick.NetworkPlayer;
using Dissonance.Networking;
using System.Threading;
namespace Dissonance.Integrations.Netick
{
    public class NetickCommsNetworkBase : NetickBehaviour
    {
        const int VoiceDataID = 0;

        public static List<NetickProximityChat> AllUnregisteredProxChats;
        private static NetickCommsNetworkBase _instance;
        private static NetickCommsNetwork _commsNetworkInstance;
        private static bool Started = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            ResetAll();
        }

        static void ResetAll()
        {
            AllUnregisteredProxChats = new List<NetickProximityChat>();
            _instance = null;
            _commsNetworkInstance = null;
            Started = false;
        }

        private void Awake()
        {
            //GetComponent<DissonanceComms>().enabled = false;
        }

        private void OnDestroy()
        {
            ResetAll();
        }

        public override unsafe void NetworkStart()
        {
            GetComponent<DissonanceComms>().LocalPlayerName = Sandbox.LocalPlayer.PlayerId.ToString();
            GetComponent<DissonanceComms>().enabled = true;
            GetComponent<VoiceBroadcastTrigger>().enabled = true;
            GetComponent<VoiceReceiptTrigger>().enabled = true;
            GetComponent<VoiceProximityBroadcastTrigger>().enabled = true;
            GetComponent<VoiceProximityReceiptTrigger>().enabled = true;

            Sandbox.Events.OnDataReceived += OnDataReceived;
            Sandbox.Events.OnPlayerDisconnected += PlayerDisconnected;
            _instance = this;
        }

        public static bool CheckDissonanceStarted(NetickProximityChat chat)
        {
            if (!Started)
            {
                AllUnregisteredProxChats.Add(chat);
                return false;
            }
            return true;
        }

        private void PlayerDisconnected(NetworkSandbox sandbox, NetworkPlayer player, TransportDisconnectReason reason)
        {
            if (_commsNetworkInstance != null)
                _commsNetworkInstance.NetickPlayerLeft(player);
        }

        public static void Initialize(NetickCommsNetwork instance)
        {
            _commsNetworkInstance = instance;
            Started = true;
            RegisterWaitingClients();
        }

        static void RegisterWaitingClients()
        {
            foreach (NetickProximityChat chat in AllUnregisteredProxChats)
                chat.StartTracking();
            AllUnregisteredProxChats.Clear();
        }

        public static void Stopped()
        {

        }

        [NotNull]
        private static T[] ConvertToArray<T>(ArraySegment<T> segment)
            where T : struct
        {
            var arr = new T[segment.Count];
            segment.CopyToSegment(arr);
            return arr;
        }

        public static void SendToServer(NetickCommsNetwork instance, ArraySegment<byte> packet, bool reliable)
        {
            if (_commsNetworkInstance != instance)
                throw new InvalidOperationException("Cannot send from mismatched instance");

            if (_instance == null)
                return;

            byte[] data = ConvertToArray(packet);
            
            if (reliable)
                _instance.Sandbox.ConnectedServer.SendData(VoiceDataID, data, data.Length, TransportDeliveryMethod.Reliable);
            else
                _instance.Sandbox.ConnectedServer.SendData(VoiceDataID, data, data.Length, TransportDeliveryMethod.Unreliable);
        }

        public static bool SendToClient(NetickCommsNetwork instance, NetickPeer dest, ArraySegment<byte> packet, bool reliable)
        {
            
            if (_commsNetworkInstance != instance)
                throw new InvalidOperationException("Cannot send from mismatched instance");

            byte[] data = ConvertToArray(packet);
            if (reliable)
                ((NetworkConnection)dest.PlayerRef).SendData(VoiceDataID, data, data.Length, TransportDeliveryMethod.Reliable);
            else
                ((NetworkConnection)dest.PlayerRef).SendData(VoiceDataID, data, data.Length, TransportDeliveryMethod.Unreliable);

            return true;
        }

        unsafe void OnDataReceived(NetworkSandbox sandbox, NetworkConnection sender, byte id, byte* data, int length, TransportDeliveryMethod transportDeliveryMethod)
        {
            if (id != VoiceDataID)
                return;

            byte[] rawData = new byte[length];
            for (int i = 0; i < length; i++)
                rawData[i] = data[i];

            if (sandbox.IsServer)
            {
                _commsNetworkInstance.DeliverMessageToServer(rawData, sender);
            }
            else
            {
                _commsNetworkInstance.DeliverMessageToClient(rawData);
            }
        }

        unsafe void SendVoiceDataToClients(NetworkSandbox sandbox, int playerID, int length, NetworkConnection clientsConnection = null)
        {
            ////append player id to the end of the voice data buffer
            //byte* idPointer = (byte*)&playerID;
            //for (int i = 0; i < 4; i++)
            //    compressedVoiceData[length + i] = idPointer[i];

            ////send the voice chat data
            //foreach (NetworkConnection conn in sandbox.ConnectedClients)
            //{
            //    if (conn != clientsConnection)
            //        conn.SendData(VoiceDataID, compressedVoiceData, length + 4, TransportDeliveryMethod.Unreliable);
            //}
        }
    }
}

