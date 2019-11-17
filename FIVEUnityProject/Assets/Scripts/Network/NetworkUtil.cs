﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace FIVE.Network
{
    internal static class NetworkUtil
    {
        public static bool Has(this int @enum, int flag)
        {
            return (@enum & flag) != 0;
        }
        public static unsafe T CastEnum<T>(byte[] bytes) where T : unmanaged
        {
            fixed (byte* pBytes = bytes)
            {
                return *(T*)pBytes;
            }
        }

        public static int ToI32(this byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToInt32(bytes, startIndex);
        }
        public static ushort ToU16(this byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToUInt16(bytes, startIndex);
        }

        public static float ToF32(this byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToSingle(bytes, startIndex);
        }

        public static bool ToBool(this byte[] bytes, int startIndex = 0)
        {
            return BitConverter.ToBoolean(bytes, startIndex);
        }

        public static Vector3 ToVector3(this byte[] bytes, int startIndex = 0)
        {
            return new Vector3(bytes.ToF32(startIndex), bytes.ToF32(startIndex + 4), bytes.ToF32(startIndex + 8));
        }

        public static Guid ToGuid(this byte[] bytes, int startIndex = 0)
        {
            byte[] guidBytes = new byte[16];
            Buffer.BlockCopy(bytes, startIndex, guidBytes, 0, 16);
            return new Guid(guidBytes);
        }

        public static string ToName(this byte[] bytes, int startIndex = 0)
        {
            return Encoding.Unicode.GetString(bytes, startIndex, bytes.Length - startIndex);
        }

        public static RoomInfo ToRoomInfo(this byte[] bytes)
        {
            var guid = bytes.ToGuid();
            int currentPlayers = bytes.ToI32(16);
            int maxPlayers = bytes.ToI32(20);
            bool hasPassword = bytes.ToBool(24);
            int host = bytes.ToI32(25);
            ushort port = bytes.ToU16(29);
            string name = bytes.ToName(31);
            return new RoomInfo { Guid = guid, CurrentPlayers = currentPlayers, MaxPlayers = maxPlayers, HasPassword = hasPassword, Host = host, Port = port, Name = name };
        }

        public static byte[] ToBytes(this int i)
        {
            return BitConverter.GetBytes(i);
        }

        public static unsafe byte[] ToBytes(int a, int b)
        {
            byte[] buffer = new byte[8];
            fixed (byte* numPtr = buffer)
            {
                *(int*)numPtr = a;
                *(int*)(numPtr + 4) = b;
            }
            return buffer;
        }        
        
        public static unsafe byte[] ToBytes(int a, int b, int c)
        {
            byte[] buffer = new byte[12];
            fixed (byte* numPtr = buffer)
            {
                *(int*)numPtr = a;
                *(int*)(numPtr + 4) = b;
                *(int*)(numPtr + 8) = c;
            }
            return buffer;
        }

        public static byte[] ToBytes(this float f)
        {
            return BitConverter.GetBytes(f);
        }

        public static byte[] ToBytes(this Vector3 v)
        {
            return Combine(v.x.ToBytes(), v.y.ToBytes(), v.z.ToBytes());
        }

        public static byte[] ToBytes(this Quaternion q)
        {
            return q.eulerAngles.ToBytes();
        }

    

        public static byte[] ToBytes(this Guid guid)
        {
            return guid.ToByteArray();
        }

        public static byte[] ToBytes(this bool value)
        {
            return BitConverter.GetBytes(value);
        }
        public static byte[] ToBytes(this ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes<T>(this T value) where T: unmanaged, Enum
        {
            return BitConverter.GetBytes(Unsafe.As<T, int>(ref value));
        }

        public static byte[] Combine(byte[] arr1, byte[] arr2)
        {
            byte[] rv = new byte[arr1.Length + arr2.Length];
            Buffer.BlockCopy(arr1, 0, rv, 0, arr1.Length);
            Buffer.BlockCopy(arr2, 0, rv, arr1.Length, arr2.Length);
            return rv;
        }

        public static byte[] Combine(byte[] arr1, byte[] arr2, byte[] arr3)
        {
            byte[] rv = new byte[arr1.Length + arr2.Length + arr3.Length];
            Buffer.BlockCopy(arr1, 0, rv, 0, arr1.Length);
            Buffer.BlockCopy(arr2, 0, rv, arr1.Length, arr2.Length);
            Buffer.BlockCopy(arr3, 0, rv, arr1.Length + arr2.Length, arr3.Length);
            return rv;
        }

        public static unsafe byte[] CombineUnsafe(params byte[][] bytesArray)
        {
            byte[] combined = new byte[bytesArray.Sum(a => a.Length)];
            int offset = 0;
            fixed (byte* pdest = combined)
            {
                foreach (byte[] bytes in bytesArray)
                {
                    fixed (byte* pbytes = bytes)
                        Unsafe.CopyBlock(ref *(pdest + offset), ref (*pbytes), (uint)bytes.Length);
                    offset += bytes.Length;
                }
            }
            return combined;
        }        
        
        public static unsafe byte[] CombineUnsafe(List<byte[]> bytesArray)
        {
            byte[] combined = new byte[bytesArray.Sum(a => a.Length)];
            int offset = 0;
            fixed (byte* pdest = combined)
            {
                foreach (byte[] bytes in bytesArray)
                {
                    fixed (byte* pbytes = bytes)
                        Unsafe.CopyBlock(ref *(pdest + offset), ref (*pbytes), (uint)bytes.Length);
                    offset += bytes.Length;
                }
            }
            return combined;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                rv.CopyFrom(array, offset);
                offset += array.Length;
            }
            return rv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom(this byte[] dest, int source, int destStartIndex)
        {
            fixed (byte* pDest = dest)
            {
                byte* p = pDest + destStartIndex;
                Unsafe.CopyBlock(p, &source, 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom(this byte[] dest, int source1, int source2, int destStartIndex)
        {
            fixed (byte* pDest = dest)
            {
                byte* p = pDest + destStartIndex;
                Unsafe.CopyBlock(p, &source1, 4);
                Unsafe.CopyBlock(p + 4, &source2, 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom(this byte[] dest, byte[] source, int destStartIndex = 0)
        {
            Unsafe.CopyBlock(ref dest[destStartIndex], ref source[0], (uint)source.Length);
        }        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void CopyFrom<T>(this byte[] dest, T source, int destStartIndex = 0) where T : unmanaged
        {
            fixed (byte* pDest = dest)
            {
                byte* p = pDest + destStartIndex;
                Unsafe.CopyBlock(p, &source, (uint)sizeof(T));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom(this byte[] dest, byte[] source1, byte[] source2, int destStartIndex = 0)
        {
            dest.CopyFrom(source1, destStartIndex);
            dest.CopyFrom(source2, destStartIndex + source1.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyFrom(this byte[] dest, byte[] source1, byte[] source2, byte[] source3, int destStartIndex = 0)
        {
            dest.CopyFrom(source1, source2, destStartIndex);
            dest.CopyFrom(source3, destStartIndex + source1.Length + source2.Length);
        }

        public static bool Equals(this byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length)
            {
                return false;
            }

            return !arr1.Where((t, i) => t != arr2[i]).Any();
        }

        public static void Write(this NetworkStream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static unsafe void Write(this NetworkStream stream, int i)
        {         
            byte[] buffer = new byte[4];
            fixed (byte* pBuffer = buffer)
            {
                *pBuffer = *(byte*)&i;
            }
            stream.Write(buffer);
        }

        public static unsafe void Write(this NetworkStream stream, ushort us)
        {
            byte[] buffer = new byte[2];
            fixed (byte* pBuffer = buffer)
            {
                *pBuffer = *(byte*)&us;
            }
            stream.Write(buffer);
        }

        public static void Write(this NetworkStream stream, bool b)
        {
            stream.Write(b.ToBytes());
        }

        public static void Write(this NetworkStream stream, Guid guid)
        {
            stream.Write(guid.ToBytes());
        }

        public static void Write(this NetworkStream stream, Enum i)
        {
            stream.Write((int)(object)i);
        }

        public static void Read(this NetworkStream stream, byte[] bytes)
        {
            stream.Read(bytes, 0, bytes.Length);
        }

        public static byte ReadAByte(this NetworkStream stream)
        {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            return buffer[0];
        }

        public static byte[] Read(this NetworkStream stream, int size)
        {
            byte[] bytes = new byte[size];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        public static unsafe int ReadI32(this NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            fixed (byte* pBytes = buffer)
            {
                return *(int*)pBytes;
            }
        }



        public static T Read<T>(this NetworkStream stream) where T : Enum
        {
            return (T)(object)stream.ReadI32();
        }

        
        public static unsafe bool BytesCompare(byte[] a1, int a1StartIdx, byte[] a2, int a2StartIdx, int length)
        {
            //Validation
            if (a1 == null || a2 == null || a1.Length < a1StartIdx + length || a2.Length < a2StartIdx + length)
            {
                return false;
            }
            //Pin memory
            fixed (byte* p1 = a1, p2 = a2)
            {
                //Offset pointer by starting index
                byte* x1 = p1 + a1StartIdx, x2 = p2 + a2StartIdx;
                //Compare 8 bytes by 8 bytes
                for (int i = 0; i < length / 8; i++, x1 += 8, x2 += 8)
                {
                    if (*(long*)x1 != *(long*)x2)
                    {
                        return false;
                    }
                }
                //Compare remaining bytes
                if ((length & 4) != 0)
                {
                    if (*((int*)x1) != *((int*)x2))
                    {
                        return false;
                    } 
                    x1 += 4; 
                    x2 += 4;
                }

                if ((length & 2) != 0)
                {
                    if (*(short*)x1 != *(short*)x2)
                    {
                        return false;
                    } 
                    x1 += 2; 
                    x2 += 2;
                }

                if ((length & 1) == 0)
                {
                    return true;
                }

                return *x1 == *x2;
            }
        }

    }
}
