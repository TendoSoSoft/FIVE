﻿using FIVE.UI;
using FIVE.UI.CodeEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace FIVE.CameraSystem
{
    public class CameraManager : MonoBehaviour
    {
        private static CameraManager instance;
        private GameObject cameraPrefab;
        private readonly Dictionary<string, Camera> name2cam = new Dictionary<string, Camera>();
        private readonly Dictionary<Camera, string> cam2name = new Dictionary<Camera, string>();
        private int index = 0;
        public static Camera CurrentActiveCamera { get; private set; }

        public static IEnumerable<Camera> GetFpsCameras =>
            from c in instance.name2cam where c.Key.ToLower().Contains("fps") select c.Value;

        public static Dictionary<string, Camera> Cameras => instance.name2cam;

        private void Awake()
        {
            Assert.IsNull(instance);
            instance = this;
            cameraPrefab = Resources.Load<GameObject>("InfrastructurePrefabs/Camera/Camera");

            CurrentActiveCamera = Camera.current ?? Camera.main;
        }

        public static Camera AddCamera(string cameraName = null, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null, bool enableAudioListener = false)
        {
            GameObject gameObject = Instantiate(instance.cameraPrefab, parent);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            if (enableAudioListener) //Make sure only one audio listener active simutaneously
            {
                foreach (Camera camerasValue in instance.name2cam.Values)
                {
                    AudioListener audioListener = camerasValue.GetComponent<AudioListener>();
                    if (audioListener != null)
                    {
                        audioListener.enabled = false;
                    }
                }
            }

            gameObject.GetComponent<AudioListener>().enabled = enableAudioListener;
            gameObject.name = cameraName ?? nameof(Camera) + gameObject.GetInstanceID();
            Camera camera = gameObject.GetComponent<Camera>();
            instance.name2cam.Add(gameObject.name, camera);
            instance.cam2name.Add(camera, gameObject.name);
            instance.RaiseEvent<OnCameraCreated>(new CameraCreatedEventArgs(gameObject.name, camera));
            return gameObject.GetComponent<Camera>();
        }

        public static void Remove(Camera camera)
        {
            string name = instance.cam2name[camera];
            instance.cam2name.Remove(camera);
            instance.name2cam.Remove(name);
            Destroy(camera.gameObject);
        }

        public static void Remove(string cameraName)
        {
            Camera c = instance.name2cam[cameraName];
            instance.name2cam.Remove(cameraName);
            instance.cam2name.Remove(c);
            Destroy(c.gameObject);
        }

        private void Update()
        {
            if (UIManager.Get<CodeEditorViewModel>()?.IsActive ?? false)
            {
                return;
            }

            if (Input.GetKeyUp(KeyCode.C) && name2cam.Count > 0)
            {
                foreach (Camera c in name2cam.Values)
                {
                    c.enabled = false;
                    (c.gameObject.GetComponent<AudioListener>() ?? c.gameObject.GetComponentInChildren<AudioListener>())
                        .enabled = false;
                }

                index %= name2cam.Count;
                Camera ca = name2cam.ElementAt(index).Value;
                ca.enabled = true;
                CurrentActiveCamera = ca;
                (ca.gameObject.GetComponent<AudioListener>() ?? ca.gameObject.GetComponentInChildren<AudioListener>())
                    .enabled = true;
                this.RaiseEvent<OnCameraSwitched, CameraSwitchedEventArgs>(
                    new CameraSwitchedEventArgs(activeCamera: ca));
                index++;
            }
        }
    }
}