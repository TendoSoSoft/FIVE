﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FIVE.Network
{
    internal class LobbyHandler : NetworkHandler
    {
        public readonly RoomInfo HostRoomInfo;
        public ICollection<RoomInfo> GetRoomInfos => roomInfos.Values;
        public RoomInfo this[Guid guid] => roomInfos[guid];
        /// <summary>
        /// Used for fetching room info from list server.
        /// </summary>
        private TcpClient listServerClient;
        private MD5 md5;

        /// <summary>
        /// Stores all room infos fetched from list server.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, RoomInfo> roomInfos = new ConcurrentDictionary<Guid, RoomInfo>();
        private readonly string listServer;
        private readonly ushort listServerPort;

        public LobbyHandler(string listServer, ushort listServerPort)
        {
            listServerClient = new TcpClient();
            HostRoomInfo = new RoomInfo();
            OnStart += LobbyHandlerOnStart;
            this.listServer = listServer;
            this.listServerPort = listServerPort;
            md5 = MD5.Create();
        }

        protected override async Task Handler()
        {
            while (true)
            {
                NetworkStream stream = listServerClient.GetStream();
                stream.Write(ListServerCode.GetRoomInfos);
                int roomCount = stream.ReadI32();
                roomInfos.Clear();
                for (int i = 0; i < roomCount; i++)
                {
                    byte[] roomInfoBuffer = new byte[stream.ReadI32()];
                    stream.Read(roomInfoBuffer, 0, roomInfoBuffer.Length);
                    var roomInfo = roomInfoBuffer.ToRoomInfo();
                    roomInfos.TryAdd(roomInfo.Guid, roomInfo);
                }
                await Task.Delay(1000 / UpdateRate);
            }
        }

        private void LobbyHandlerOnStart()
        {
            listServerClient.Connect(listServer, listServerPort);
        }


        public void CreateRoom()
        {
            if (!listServerClient.Connected)
            {
                return;
            }
            NetworkStream stream = listServerClient.GetStream();
            stream.Write(ListServerCode.CreateRoom);
            stream.Write(HostRoomInfo);
            HostRoomInfo.Guid = stream.ReadGuid();
        }



        public void RemoveRoom()
        {
            if (!listServerClient.Connected)
            {
                return;
            }
            NetworkStream stream = listServerClient.GetStream();
            stream.Write(ListServerCode.RemoveRoom);
            stream.Write(HostRoomInfo.Guid);
        }

        private void UpdateRoomInfo(ListServerCode code)
        {
            if (listServerClient.Connected)
            {
                NetworkStream stream = listServerClient.GetStream();
                code |= ListServerCode.UpdateRoom;
                stream.Write(code);
                stream.Write(HostRoomInfo.Guid);
                switch (code)
                {
                    case ListServerCode.UpdateName:
                        stream.Write(HostRoomInfo.Name);
                        break;
                    case ListServerCode.UpdateCurrentPlayer:
                        stream.Write(HostRoomInfo.CurrentPlayers);
                        break;
                    case ListServerCode.UpdateMaxPlayer:
                        stream.Write(HostRoomInfo.MaxPlayers);
                        break;
                    case ListServerCode.UpdatePassword:
                        stream.Write(HostRoomInfo.HasPassword);
                        break;
                    default:
                        break;
                }
            }
        }

        public void UpdateRoomName(string name)
        {
            HostRoomInfo.Name = name;
            UpdateRoomInfo(ListServerCode.UpdateName);
        }

        public void UpdateCurrentPlayers(int current)
        {
            HostRoomInfo.CurrentPlayers = current;
            UpdateRoomInfo(ListServerCode.UpdateCurrentPlayer);
        }

        public void UpdateMaxPlayers(int max)
        {
            HostRoomInfo.MaxPlayers = max;
            UpdateRoomInfo(ListServerCode.UpdateMaxPlayer);
        }

        public void UpdatePassword(bool hasPassword, string password)
        {
            HostRoomInfo.HasPassword = hasPassword;
            if (hasPassword)
                HostRoomInfo.SetRoomPassword(password);
            UpdateRoomInfo(ListServerCode.UpdatePassword);
        }

    }
}
