﻿using UnityEngine;
namespace FIVE.Network.Serializers
{
    public static class TransformSerialize
    {
        public static int GetSize(this Serializer<Transform> transform)
        {
            return 25;
        }

        public static void Deserialize(this Serializer<Transform> _, in byte[] bytes, Transform obj)
        {
            var eulerAngles = bytes.ToVector3();
            var position = bytes.ToVector3(12);
            obj.eulerAngles = eulerAngles;
            obj.position = position;
        }

        public static unsafe void Deserialize(this Serializer<Transform> _, byte* bytes, Transform transform)
        {
            transform.rotation = Quaternion.Euler(*(Vector3*)bytes);
            transform.position = *((Vector3*)bytes + 1);
        }

        public static void Serialize(this Serializer<Transform> _, in Transform transform, in byte[] bytes, int startIndex = 0)
        {
            bytes.CopyFrom(transform.ToBytes(), startIndex);
        }

        public static void Serialize(this Serializer<Transform> _, in Transform transform, out byte[] bytes)
        {
            bytes = transform.ToBytes();
        }

        private static unsafe byte[] ToBytes(this Transform transform)
        {
            byte[] bytes = new byte[25];
            
            bytes[0] = (byte)ComponentType.Transform;
            fixed (byte* pBytes = bytes)
            {
                *(Vector3*)(pBytes + 1) = transform.rotation.eulerAngles;
                *(Vector3*)(pBytes + 13) = transform.position;
            }
            return bytes;
        }
    }
}
