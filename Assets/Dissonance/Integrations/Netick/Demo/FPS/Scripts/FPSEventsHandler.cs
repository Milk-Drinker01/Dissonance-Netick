﻿using UnityEngine;
using Netick;
using Netick.Unity;

namespace Netick.Samples.FPS
{
    public class FPSEventsHandler : NetworkEventsListener
    {
        public Transform    SpawnPos;
        public GameObject   PlayerPrefab;

        // This is called to read inputs.
        public override void OnInput(NetworkSandbox sandbox)
        {
            var input         = sandbox.GetInput<FPSInput>();

            input.Movement    = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            input.ShootInput |= Input.GetMouseButton(0);
            sandbox.SetInput<FPSInput>(input);
        }

        // This is called on the server when a player has connected.
        public override void OnPlayerConnected(NetworkSandbox sandbox, NetworkPlayer networkPlayer)
        {
            var spawnPos               = SpawnPos.position + Vector3.left * (1 + sandbox.ConnectedPlayers.Count);
            var player                 = sandbox.NetworkInstantiate(PlayerPrefab, spawnPos, Quaternion.identity, networkPlayer).GetComponent<FPSController>();
            networkPlayer.PlayerObject = player.gameObject;
        }
    }
}