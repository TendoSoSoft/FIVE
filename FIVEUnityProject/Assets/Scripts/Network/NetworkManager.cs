﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace FIVE.Network
{
    public class NetworkManager : MonoBehaviour
    {
        /// <summary>
        /// Describes state of this instance.
        /// </summary>
        public enum NetworkState
        {
            /// <summary>
            /// Networked game is not running.
            /// </summary>
            Idle,

            
            /// <summary>
            /// Hosting a networked game.
            /// </summary>
            Host,

            /// <summary>
            /// Connecting a game.
            /// </summary>
            Connecting,

            /// <summary>
            /// Connected to a game hosted.
            /// </summary>
            Client,
        }
        public static NetworkManager Instance { get; private set; }
        [SerializeField] private string listServer;
        [SerializeField] private ushort listServerPort;
        [SerializeField] private int updateRate = 30;
        [SerializeField] private ushort gameServerPort = 8889;
        public NetworkState State { get; internal set; } 

        public ICollection<RoomInfo> RoomInfos => lobbyHandler.GetRoomInfos;
        private LobbyHandler lobbyHandler;

        public int PlayerIndex { get; internal set; }
        private NetworkGameHandler networkGameHandler;
        private ConcurrentQueue<MainThreadTask> tasks;

        public void Awake()
        {
            if (Instance != null)
            {
                throw new Exception($"{nameof(NetworkManager)} can only be instantiated once");
            }
            Instance = this;
            State = NetworkState.Idle;
            tasks = new ConcurrentQueue<MainThreadTask>();
            lobbyHandler = new LobbyHandler(listServer, listServerPort);
        }

        public IEnumerator Start()
        {
            lobbyHandler.Start();
            yield return null;
        }

        public void JoinRoom(Guid guid, string password)
        {
            RoomInfo roomInfo = lobbyHandler[guid];
            roomInfo.SetRoomPassword(password);
            networkGameHandler = new ClientGameHandler(new TcpClient(), HandShaker.GetClientHandShaker(roomInfo));
            networkGameHandler.Start();
        }

        public void CreateRoom(string roomName, int maxPlayers, bool hasPassword, string password)
        {
            RoomInfo roomInfo = lobbyHandler.HostRoomInfo;
            roomInfo.Name = roomName;
            roomInfo.Port = gameServerPort;
            roomInfo.CurrentPlayers = 1; //Host self
            roomInfo.HasPassword = hasPassword;
            roomInfo.MaxPlayers = maxPlayers;
            if (hasPassword)
            {
                roomInfo.SetRoomPassword(password);
            }

            lobbyHandler.CreateRoom();
            //networkGameHandler = new HostGameHandler(new TcpListener(IPAddress.Loopback, gameServerPort),
            //    HandShaker.GetHostHandShaker(lobbyHandler.HostRoomInfo));
            //networkGameHandler.Start();
            State = NetworkState.Host;
        }

        public void Disconnect()
        {
            lobbyHandler.RemoveRoom();
        }

        public void Schedule(MainThreadTask task)
        {
            tasks.Enqueue(task);
        }

        public void LateUpdate()
        {
            while (tasks.TryDequeue(out MainThreadTask task))
            {
                task.Run();
            }
        }
    }
}