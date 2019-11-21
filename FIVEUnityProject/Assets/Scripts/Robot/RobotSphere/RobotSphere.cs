﻿using FIVE.AWSL;
using FIVE.CameraSystem;
using FIVE.ControllerSystem;
using FIVE.EventSystem;
using FIVE.RobotComponents;
using FIVE.UI;
using FIVE.UI.CodeEditor;
using System.Collections;
using UnityEngine;

namespace FIVE.Robot
{
    [RequireComponent(typeof(Movable))]
    [RequireComponent(typeof(CPU))]
    [RequireComponent(typeof(Battery))]
    public class RobotSphere : RobotBehaviour
    {
        public int ID;

        public enum RobotSphereState { Idle, Walk, Jump, Open };

        public GameObject BulletPrefab;
        public GameObject GunfirePrefab;

        // Script References
        private RobotFreeAnim animator;

        private CharacterController cc;

        // private readonly ControllerOp currOp = ControllerOp.FPS;
        public RobotSphereState CurrentState = RobotSphereState.Idle;

        private Camera fpsCamera;

        private FpsController fpsController;

        // Robot Status
        private Movable movable;

        private AWSLScript script;
        private bool scriptActive;

        private Camera thirdPersonCamera;

        // Robot Components
        public Battery Battery { get; private set; }

        public CPU CPU { get; private set; }

        private float health;

        protected override void Awake()
        {
            ID = RobotManager.NextID;

            movable = GetComponent<Movable>();

            cc = GetComponent<CharacterController>();

            scriptActive = false;

            Battery = GetComponent<Battery>();
            CPU = GetComponent<CPU>();

            base.Awake();
        }

        protected override void Start()
        {
            fpsCamera = CameraManager.AddCamera("Robot POV " + ID.ToString(), parent: transform);
            fpsCamera.transform.localPosition = new Vector3(0f, 0.1f, 0.07f);
            fpsCamera.gameObject.AddComponent<RobotCameraScanning>();
            thirdPersonCamera = CameraManager.AddCamera("Robot " + ID.ToString(),
                parent: transform, enableAudioListener: true,
                position: new Vector3(0, 2, 0),
                rotation: Quaternion.Euler(90, 0, 0));

            GameObject light = transform.GetComponentInChildren<Light>().gameObject;
            light.SetParent(fpsCamera.transform);

            animator = new RobotFreeAnim(gameObject);
            fpsController = new FpsController(GetComponent<CharacterController>(), gameObject);
            EventManager.Subscribe<OnCodeEditorSaved, UpdateScriptEventArgs>(OnCodeSaved);
            OnFixedUpdate += RobotSphereUpdate;

            health = 100.0f;

            base.Start();
        }

        private void OnMouseDown()
        {
            RobotManager.ActiveRobot = gameObject;
            CameraManager.SetCamera(fpsCamera);
        }

        private void OnCodeSaved(object sender, UpdateScriptEventArgs e)
        {
            if (movable.enabled)
            {
                movable.ClearSchedule();
            }
            else
            {
                movable.enabled = true;
            }

            script = new AWSLScript(this, e.Code);
            scriptActive = true;
        }

        private void RobotSphereUpdate()
        {
            //Block user input when editor is up
            if (UIManager.Get<CodeEditorViewModel>()?.IsActive ?? false)
            {
                return;
            }

            // update animation at beginning to ensure consistency
            animator.Update(CurrentState);
            CurrentState = cc.velocity.magnitude < float.Epsilon || GetComponent<Battery>().CurrentEnergy <= 0f ? RobotSphereState.Idle : RobotSphereState.Walk;

            if (scriptActive)
            {
                ExecuteScript();
            }
            else
            {
                fpsController.Update();
            }

            if (CurrentState == RobotSphereState.Walk)
            {
                fpsCamera.transform.localPosition = new Vector3(Mathf.Sin(Time.time * 8f) * 0.02f, 0.1f + Mathf.Sin(Time.time * 16f) * 0.02f, 0.07f);
            }
        }

        public void Move(Movable.Move move, int steps, bool schedule = false)
        {
            if (GetComponent<Battery>().CurrentEnergy > 0f && movable.enabled)
            {
                CurrentState = RobotSphereState.Walk;
                if (schedule)
                {
                    movable.ScheduleMove(move, steps);
                }
                else
                {
                    movable.MoveOnces[(int)move](steps);
                }
            }
        }

        private IEnumerator ShutGunfire(GameObject gunfire)
        {
            yield return new WaitForSeconds(0.1f);
            gunfire.SetActive(false);
            Destroy(gunfire);
        }

        private IEnumerator KillAlien(GameObject alien)
        {
            yield return new WaitForSeconds(0.2f);
            EnemyBehavior enemyBehavior = alien.GetComponent<EnemyBehavior>();
            enemyBehavior.OnHit();
        }

        // Attack on a target coordinate
        public void Attack(Vector3 target)
        {
            if (movable.enabled)
            {
                GameObject gunfire = Instantiate(GunfirePrefab, transform.position + transform.forward * 10f + new Vector3(0, 3, 0), Quaternion.identity);
                StartCoroutine(ShutGunfire(gunfire));
                GameObject bullet = Instantiate(BulletPrefab, transform.position + transform.forward * 10f + new Vector3(0, 1, 0), Quaternion.identity);
                bullet.GetComponent<Bullet>().Target = target;
                fpsCamera.GetComponent<CameraShake>().ShakeCamera(0.5f, 0.5f);
            }
        }

        // Attack on a GameObject (such as AlienBeetle)
        public void Attack(GameObject target)
        {
            if (movable.enabled)
            {
                GameObject gunfire = Instantiate(GunfirePrefab, transform.position + transform.forward * 10f + new Vector3(0, 3, 0), Quaternion.identity);
                StartCoroutine(ShutGunfire(gunfire));
                GameObject bullet = Instantiate(BulletPrefab, transform.position + transform.forward * 10f + new Vector3(0, 1, 0), Quaternion.identity);
                bullet.GetComponent<Bullet>().Target = target.transform.position;
                StartCoroutine(KillAlien(target));
                fpsCamera.GetComponent<CameraShake>().ShakeCamera(0.5f, 0.5f);
            }
        }

        public void OnHit()
        {
            health -= 10.0f;
            if (health <= 0)
            {
                if (RobotManager.ID2Robot.Count == 1)
                {
                    // lose
                }
                else
                {
                    int id = RobotManager.ID2Robot.GetEnumerator().Current.Key;
                    RobotManager.ActiveRobot = RobotManager.ID2Robot[id];
                    CameraManager.SetCamera(RobotManager.ActiveRobot.GetComponent<RobotSphere>().fpsCamera);

                    RobotManager.RemoveRobot(gameObject);
                    CameraManager.Remove(fpsCamera);
                    CameraManager.Remove(thirdPersonCamera);
                    gameObject.SetActive(false);
                    Destroy(gameObject);
                }
            }
        }

        private void ExecuteScript()
        {
            scriptActive = !script.Execute();
            if (!scriptActive)
            {
                movable.enabled = RobotManager.ActiveRobot == gameObject;
            }
        }
    }
}