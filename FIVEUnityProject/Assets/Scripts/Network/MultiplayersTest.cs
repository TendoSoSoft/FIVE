﻿using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace FIVE.Network
{
    public class MultiplayersTest : MonoBehaviour
    {
        private GameObject Player;
        private PhotonView photonView;
        void Start()
        {
            Player = PhotonNetwork.Instantiate("EntityPrefabs/robotSphere", new Vector3(3, 0, 3), Quaternion.identity);
            photonView = Player.GetComponent<PhotonView>();
            photonView.ObservedComponents = new List<Component>{
                Player.GetComponent<PhotonAnimatorView>(),
                Player.GetComponent<PhotonTransformView>(),
            };
            photonView.Synchronization = ViewSynchronization.ReliableDeltaCompressed;


        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
