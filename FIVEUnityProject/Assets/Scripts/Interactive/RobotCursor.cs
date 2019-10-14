﻿using FIVE.CameraSystem;
using FIVE.EventSystem;
using FIVE.UI;
using FIVE.UI.CodeEditor;
using UnityEngine;

namespace FIVE.Interactive
{
    internal class RobotCursor : MonoBehaviour
    {
        void Awake()
        {
            EventManager.Subscribe<OnCameraSwitched, CameraSwitchedEventArgs>(OnCameraSwitched);
        }

        private void OnCameraSwitched(object sender, CameraSwitchedEventArgs e)
        {
            if (UIManager.Get<CodeEditorViewModel>().IsActive)
            {
                UIManager.SetCursor(UIManager.CursorType.Regular);
            }
            else if (e.NewCamera?.name.Contains("fps") ?? false)
            {
                UIManager.SetCursor(UIManager.CursorType.Aim);
            }
        }
    }
}