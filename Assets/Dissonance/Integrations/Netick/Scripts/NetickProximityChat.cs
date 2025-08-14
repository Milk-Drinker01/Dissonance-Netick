using Dissonance;
using Dissonance.Integrations.Netick;
using Netick;
using Netick.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dissonance
{
    public class NetickProximityChat : NetworkBehaviour, IDissonancePlayer
    {
        #region IDissonancePlayer Interface
        public string PlayerId => _playerID;
        private string _playerID;

        public Vector3 Position => transform.position;

        public Quaternion Rotation => transform.rotation;

        public NetworkPlayerType Type => IsInputSource ? NetworkPlayerType.Local : NetworkPlayerType.Remote;

        public bool IsTracking => _isTracking;
        private bool _isTracking;
        #endregion

        public override void NetworkStart()
        {
            _playerID = InputSourcePlayerId.ToString();
            if (NetickDissonanceManager.CheckDissonanceStarted(this))
                StartTracking();
        }

        public override void NetworkDestroy()
        {
            if (IsTracking)
                StopTracking();
        }

        public void StartTracking()
        {
            if (string.IsNullOrEmpty(PlayerId))
                return;

            if (IsTracking)
                Debug.Log("cannot stop tracking if you are already tracking!");

            //UnityEngine.Debug.Log("adding tracker");
            NetickDissonanceManager.CommsInstance.TrackPlayerPosition(this);
            NetickDissonanceManager.AllRegisteredProxChats.Add(this);
            _isTracking = true;
        }

        private void StopTracking()
        {
            if (!IsTracking)
                Debug.Log("cannot stop tracking if you are not tracking!");

            if (NetickDissonanceManager.CommsInstance != null)
            {
                NetickDissonanceManager.CommsInstance.StopTracking(this);
                _isTracking = false;
            }
        }
    }
}