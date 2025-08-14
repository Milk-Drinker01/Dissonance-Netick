using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netick;
using Netick.Unity;
using JetBrains.Annotations;
using System;
using Dissonance.Extensions;

namespace Dissonance.Integrations.Netick
{
    public class NetickCommsNetworkBase : NetickBehaviour
    {
        const int VoiceDataID = 0;

        public static NetickCommsNetworkBase instance;
        public static DissonanceComms CommsInstance;

        public static List<NetickProximityChat> AllRegisteredProxChats;
        public static List<NetickProximityChat> AllUnregisteredProxChats;
        
        private static NetickCommsNetwork _commsNetworkInstance;
        private static bool Started = false;
        private static GameObject dissonancePrefabInstance;


        public GameObject DissonancePrefab;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            ResetAll();
        }

        static void ResetAll()  //we reset everything on game startup, and on disconnect
        {
            instance = null;
            _commsNetworkInstance = null;

            AllRegisteredProxChats = new List<NetickProximityChat>();
            AllUnregisteredProxChats = new List<NetickProximityChat>();

            ResetWhenDestroyInstance();
        }

        static void ResetWhenDestroyInstance()  //we need to respawn the dissonance instance when switching scenes, cause it doesnt like long pauses
        {
            foreach (NetickProximityChat chat in AllRegisteredProxChats)
                AllUnregisteredProxChats.Add(chat);
            AllRegisteredProxChats = new List<NetickProximityChat>();

            Started = false;
        }

        private void Awake()
        {
            //GetComponent<DissonanceComms>().enabled = false;
        }

        private void OnDestroy()
        {
            DestroyDissoananceInstance();
            ResetAll();
        }

        public override unsafe void NetworkStart()
        {
            Sandbox.Events.OnDataReceived += OnDataReceived;
            Sandbox.Events.OnPlayerLeft += PlayerDisconnected;
            instance = this;

            SpawnDissonanceObject();
        }

        [ContextMenu("Debug Restart Dissonance")]
        public void SpawnDissonanceObject()
        {
            DestroyDissoananceInstance();
            dissonancePrefabInstance = Instantiate(DissonancePrefab);
            DontDestroyOnLoad(dissonancePrefabInstance);
            CommsInstance = dissonancePrefabInstance.GetComponent<DissonanceComms>();

            CommsInstance.LocalPlayerName = Sandbox.LocalPlayer.PlayerId.ToString();
            CommsInstance.enabled = true;
            dissonancePrefabInstance.GetComponent<VoiceBroadcastTrigger>().enabled = true;
            dissonancePrefabInstance.GetComponent<VoiceReceiptTrigger>().enabled = true;
            dissonancePrefabInstance.GetComponent<VoiceProximityBroadcastTrigger>().enabled = true;
            dissonancePrefabInstance.GetComponent<VoiceProximityReceiptTrigger>().enabled = true;
        }

        [ContextMenu("Debug Stop Dissonance")]
        public void DestroyDissoananceInstance()
        {
            if (dissonancePrefabInstance != null)
                Destroy(dissonancePrefabInstance);
            ResetWhenDestroyInstance();
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

        private void PlayerDisconnected(NetworkSandbox sandbox, NetworkPlayerId player)
        {
            if (!sandbox.IsServer)
                return; 
            if (_commsNetworkInstance != null)
                _commsNetworkInstance.NetickPlayerLeft(sandbox.GetPlayerById(player));
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

            if (NetickCommsNetworkBase.instance == null)
                return;

            byte[] data = ConvertToArray(packet);
            
            if (reliable)
                NetickCommsNetworkBase.instance.Sandbox.ConnectedServer.SendData(VoiceDataID, data, data.Length, TransportDeliveryMethod.Reliable);
            else
                NetickCommsNetworkBase.instance.Sandbox.ConnectedServer.SendData(VoiceDataID, data, data.Length, TransportDeliveryMethod.Unreliable);
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

