using System;
using UnityEngine;

namespace FIVE.EventSystem
{
    public abstract class OnCameraCreated : IEventType<EventHandler<OnCameraCreatedArgs>, OnCameraCreatedArgs>
    {
    }

    public sealed class OnCameraCreatedArgs : EventArgs
    {
        public string Id;
        public Camera Camera;
    }
}