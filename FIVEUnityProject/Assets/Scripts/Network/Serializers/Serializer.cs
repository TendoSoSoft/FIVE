﻿using System.Collections.Generic;
using UnityEngine;
namespace FIVE.Network.Serializers
{
    public abstract class Serializer
    {
        public static void Serialize(List<Component> components, out byte[] bytes)
        {
            int offset = 0;
            bytes = new byte[GetTotalSize(components)];
            foreach (Component component in components)
            {
                DoSerialize(component, bytes, ref offset);
            }
        }

        public static int GetTotalSize(List<Component> components)
        {
            int result = 0;
            foreach (Component component in components)
            {
                switch (component)
                {
                    case Transform _:
                        result += Serializer<Transform>.Instance.GetSize();
                        break;
                }
            }
            return result;
        }

        private static void DoSerialize<T>(T obj, byte[] bytes, ref int offset)
        {
            switch (obj)
            {
                case Transform transform:
                    bytes.CopyFromUnsafe(ComponentType.Transform.ToBytes());
                    offset += 4;
                    Serializer<Transform>.Instance.Serialize(transform, bytes, offset);
                    offset += Serializer<Transform>.Instance.GetSize();
                    break;
            }
        }
    }

    public sealed class Serializer<T> : Serializer
    {
        public static Serializer<T> Instance { get; } = new Serializer<T>();
        private Serializer() { }
    }
}
