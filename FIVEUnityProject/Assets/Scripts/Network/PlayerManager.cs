﻿using Photon.Pun;
using UnityEngine;

namespace FIVE.Network
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private string PlayerNickName = "";
        private GameObject player;
        private PhotonView playerPhotonView;
        void Start()
        {

            player = PhotonNetwork.Instantiate("EntityPrefabs/robotSphere", new Vector3(Random.Range(0,10), Random.Range(0,5), Random.Range(0,10)), Quaternion.identity);
            playerPhotonView = player.GetComponent<PhotonView>();
            player.name = PlayerNickName;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

        }
    }
}
