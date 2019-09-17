﻿using FIVE.ControllerSystem;
using FIVE.EventSystem;
using UnityEngine;

namespace FIVE
{
    public class RobotSphere : MonoBehaviour
    {
        public enum RobotState { Idle, Walk, Jump, Open };

        private enum ControllerOp { FPS, RTS, };

        private readonly ControllerOp currOp = ControllerOp.FPS;
        public RobotState currState = RobotState.Idle;

        private bool editingCode = false;
        private LauncherEditorArgs code = new LauncherEditorArgs { Code = "" };

        public Camera CameraPrefab;

        // Script References
        private RobotFreeAnim animator;

        private FpsController fpsController;

        private Camera fpsCamera;

        private void Awake()
        {
            fpsCamera = Instantiate(CameraPrefab);
            Transform eye = transform.GetChild(0).GetChild(1); // HACK
            fpsCamera.transform.parent = eye;
            fpsCamera.transform.localPosition = new Vector3(0, 0, 0);
            fpsCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Util.RaiseEvent<OnCameraCreated>(this, new OnCameraCreatedArgs { Id = "", Camera = fpsCamera });
        }

        private void Start()
        {
            animator = new RobotFreeAnim(gameObject);
            fpsController = new FpsController(GetComponent<CharacterController>(), gameObject);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.E))
            {
                editingCode = true;
                Util.RaiseEvent<DoLaunchEditor, LauncherEditorArgs>(this, code);
            }

            if (editingCode)
            {
                if (code.Saved)
                {
                    editingCode = false;
                }
            }
            else
            {
                animator.Update(currState);
                fpsController.Update();
            }
        }
    }
}