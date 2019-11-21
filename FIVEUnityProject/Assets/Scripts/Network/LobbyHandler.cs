﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace FIVE.Network
{
    /// <summary>
    /// Used for communicating with list server.
    /// </summary>
    public enum ListServerHeader : byte
    {
        AliveTick = 1,
        RoomInfos = 2,
        AssignGuid = 3,
        CreateRoom = 4,
        RemoveRoom = 5,
        UpdateName = 6,
        UpdateCurrentPlayer = 7,
        UpdateMaxPlayer = 8,
        UpdatePassword = 9
    }

    internal class LobbyHandler : IDisposable
    {
        public ICollection<RoomInfo> GetRoomInfos => roomInfos.Values;
        public RoomInfo this[Guid guid] => roomInfos[guid];
        /// <summary>
        /// Used for fetching room info from list server.
        /// </summary>
        private readonly TcpClient listServerClient;
        private readonly MD5 md5;

        /// <summary>
        /// Stores all room infos fetched from list server.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, RoomInfo> roomInfos = new ConcurrentDictionary<Guid, RoomInfo>();
        private readonly string listServer;
        private readonly ushort listServerPort;
        private Task sendTask;
        private Task readTask;
        private CancellationTokenSource cts;
        private readonly Dictionary<ListServerHeader, Action<TcpClient>> handlers;

        private readonly ConcurrentQueue<ListServerHeader> sendQueue;

        public LobbyHandler(string listServer, ushort listServerPort)
        {
            listServerClient = new TcpClient();
            this.listServer = listServer;
            this.listServerPort = listServerPort;
            md5 = MD5.Create();
            sendQueue = new ConcurrentQueue<ListServerHeader>();
            handlers = new Dictionary<ListServerHeader, Action<TcpClient>>
            {
                {ListServerHeader.AliveTick , AliveTickHandler},
                {ListServerHeader.RoomInfos, RoomInfosHandler},
                {ListServerHeader.AssignGuid, AssignGuidHandler},
                {ListServerHeader.CreateRoom, CreateRoomHandler},
                {ListServerHeader.RemoveRoom, RemoveRoomHandler},
                {ListServerHeader.UpdateName, UpdateNameHandler},
                {ListServerHeader.UpdateCurrentPlayer, UpdateCurrentPlayer},  
                {ListServerHeader.UpdateMaxPlayer, UpdateMaxPlayer},
                {ListServerHeader.UpdatePassword, UpdatePassword}
            };
        }

        private void AliveTickHandler(TcpClient client)
        {
            client.GetStream().WriteByte((byte)ListServerHeader.AliveTick);
        }

        private void RoomInfosHandler(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            roomInfos.Clear();
            int roomCount = stream.ReadI32();
            for (int i = 0; i < roomCount; i++)
            {
                byte[] roomInfoBuffer = new byte[stream.ReadI32()];
                stream.Read(roomInfoBuffer, 0, roomInfoBuffer.Length);
                var roomInfo = roomInfoBuffer.ToRoomInfo();
                roomInfos.TryAdd(roomInfo.Guid, roomInfo);
            }
        } 
        
        private void CreateRoomHandler(TcpClient client)
        {
            client.GetStream().Write(NetworkManager.Instance.CurrentRoomInfo.FullPacket());
        }

        private void AssignGuidHandler(TcpClient client)
        {
            NetworkManager.Instance.CurrentRoomInfo.Guid = client.GetStream().ReadGuid();
        }

        private void RemoveRoomHandler(TcpClient client)
        {
            client.GetStream().WriteByte((byte)ListServerHeader.RemoveRoom);
        }

        private void UpdateNameHandler(TcpClient client)
        {
            client.GetStream().Write(NetworkManager.Instance.CurrentRoomInfo.NamePacket());
        }  
        
        private void UpdateCurrentPlayer(TcpClient client)
        {
            client.GetStream().Write(NetworkManager.Instance.CurrentRoomInfo.CurrentPlayerPacket());
        }
        
        private void UpdateMaxPlayer(TcpClient client)
        {
            client.GetStream().Write(NetworkManager.Instance.CurrentRoomInfo.MaxPlayerPacket());
        }

        private void UpdatePassword(TcpClient client)
        {
            client.GetStream().Write(NetworkManager.Instance.CurrentRoomInfo.PasswordPacket());
        }

        public void Start()
        {
            listServerClient.Connect(listServer, listServerPort);
            cts = new CancellationTokenSource();
            sendTask = SendAsync(cts.Token);
            readTask = ReadAsync(cts.Token);
            //timerTask = TimerAsync(cts.Token);
        }

        public void Stop()
        {
            cts.Cancel();
            listServerClient.GetStream().Close();
            listServerClient.Close();
        }

        private async Task SendAsync(CancellationToken ct)
        {
            if (!listServerClient.Connected)
            {
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                if (sendQueue.TryDequeue(out ListServerHeader header))
                {
                    handlers[header](listServerClient);
                }
                else
                {
                    handlers[ListServerHeader.AliveTick](listServerClient);
                }
                await Task.Delay(30, ct);
            }
        }

        private async Task ReadAsync(CancellationToken ct)
        {
            if (!listServerClient.Connected)
            {
                return;
            }
            NetworkStream stream = listServerClient.GetStream();
            while (!ct.IsCancellationRequested)
            {
                ListServerHeader header = stream.Read(1).As<ListServerHeader>();
                handlers[header](listServerClient);
                await Task.Delay(500, ct);
            }
        }

        public void RemoveRoom()
        {
            sendQueue.Enqueue(ListServerHeader.RemoveRoom);
        }

        public void CreateRoom()
        {
            sendQueue.Enqueue(ListServerHeader.CreateRoom);
        }

        public void UpdateRoomName(string name)
        {
            NetworkManager.Instance.CurrentRoomInfo.Name = name;
            sendQueue.Enqueue(ListServerHeader.UpdateName);
        }

        public void UpdateCurrentPlayers(int current)
        {
            NetworkManager.Instance.CurrentRoomInfo.CurrentPlayers = current;
            sendQueue.Enqueue(ListServerHeader.UpdateCurrentPlayer);
        }

        public void UpdateMaxPlayers(int max)
        {
            NetworkManager.Instance.CurrentRoomInfo.MaxPlayers = max;
            sendQueue.Enqueue(ListServerHeader.UpdateMaxPlayer);
        }

        public void UpdatePassword(bool hasPassword, string password)
        {
            if (hasPassword)
            {
                NetworkManager.Instance.CurrentRoomInfo.SetRoomPassword(password);
            }
            if (NetworkManager.Instance.CurrentRoomInfo.HasPassword != hasPassword)
            {
                sendQueue.Enqueue(ListServerHeader.UpdatePassword);
            }
        }

        public void Dispose()
        {
            listServerClient?.Dispose();
            md5?.Dispose();
            sendTask?.Dispose();
            readTask?.Dispose();
        }
    }

    internal static class LobbyHandlerExtension
    {
        internal static unsafe byte[] FullPacket(this RoomInfo roomInfo)
        {
            const int headerSize =
                sizeof(byte) + //header
                sizeof(int); //size of following
            const int roomInfoFixedSize =
                sizeof(int) + //current player
                sizeof(int) + //max player
                sizeof(bool) + //password flag
                sizeof(ushort); //listening port
            string name = roomInfo.Name;
            int nameBytesCount = Encoding.Unicode.GetByteCount(name);
            byte[] buffer = new byte[headerSize + roomInfoFixedSize + nameBytesCount];
            buffer[0] = (byte)ListServerHeader.CreateRoom;
            fixed (byte* pBuffer = &buffer[1])
            {
                *(int*)pBuffer = roomInfoFixedSize + nameBytesCount;
                *(int*)(pBuffer + sizeof(int)) = roomInfo.CurrentPlayers;
                *(int*)(pBuffer + sizeof(int) + sizeof(int)) = roomInfo.MaxPlayers;
                *(bool*)(pBuffer + sizeof(int) + sizeof(int) + sizeof(int)) = roomInfo.HasPassword;
                *(ushort*)(pBuffer + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(bool)) = roomInfo.Port;
            }
            Encoding.Unicode.GetBytes(name, 0, name.Length, buffer, headerSize + roomInfoFixedSize);
            return buffer;
        }

        internal static unsafe Guid ReadGuid(this NetworkStream stream)
        {
            byte[] buffer = new byte[16];
            stream.Read(buffer, 0, buffer.Length);
            fixed (byte* pBytes = buffer)
            {
                Guid* guid = (Guid*)pBytes;
                return *guid;
            }
        }

        internal static byte[] NamePacket(this RoomInfo roomInfo)
        {
            string str = roomInfo.Name;
            byte[] buffer = new byte[sizeof(ListServerHeader) + sizeof(int) + Encoding.Unicode.GetByteCount(str)];
            buffer[0] = (byte)ListServerHeader.UpdateName;
            Encoding.Unicode.GetBytes(str, 0, str.Length, buffer, 5);
            return buffer;
        }        
        
        internal static unsafe byte[] CurrentPlayerPacket(this RoomInfo roomInfo)
        {
            byte[] buffer = new byte[sizeof(ListServerHeader) + sizeof(int)];
            buffer[0] = (byte)ListServerHeader.UpdateCurrentPlayer;
            fixed (byte* pBuffer = &buffer[sizeof(ListServerHeader)])
            {
                *(int*)pBuffer = roomInfo.CurrentPlayers;
            }
            return buffer;
        }        
        
        internal static unsafe byte[] MaxPlayerPacket(this RoomInfo roomInfo)
        {
            byte[] buffer = new byte[sizeof(ListServerHeader) + sizeof(int)];
            buffer[0] = (byte)ListServerHeader.UpdateMaxPlayer;
            fixed (byte* pBuffer = &buffer[sizeof(ListServerHeader)])
            {
                *(int*)pBuffer = roomInfo.MaxPlayers;
            }
            return buffer;
        }

        internal static byte[] PasswordPacket(this RoomInfo roomInfo)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)ListServerHeader.UpdatePassword;
            buffer[1] = roomInfo.HasPassword ? (byte)1 : (byte)0;
            return buffer;
        }
    }
}
