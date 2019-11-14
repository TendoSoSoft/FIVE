﻿using FIVE.Enemy;
using FIVE.Robot;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIVE.AWSL
{
    internal class AWSLScript
    {
        private static SharedData shared;

        private readonly DynValue coroutine;
        private readonly RobotSphere robot;

        private readonly Script script;

        static AWSLScript()
        {
            shared = new SharedData();
        }

        internal AWSLScript(RobotSphere robot, string code)
        {
            this.robot = robot;

            try
            {
                UserData.RegisterAssembly();
                UserData.RegisterProxyType<RobotProxy, RobotSphere>(o => new RobotProxy(o));
                script = new Script(CoreModules.TableIterators | CoreModules.Table);
                script.DoString(code);
                coroutine = script.CreateCoroutine(script.Globals.Get("main"));

                script.Globals["Shared"] = shared;
                script.Globals["Self"] = robot;

                script.Globals["print"] = (Action<DynValue>)(x => Debug.Log(x.ToDebugPrintString()));
                script.Globals["forward"] = FuncMove(Movable.Move.Front);
                script.Globals["backward"] = FuncMove(Movable.Move.Back);
                script.Globals["left"] = FuncMove(Movable.Move.Left);
                script.Globals["right"] = FuncMove(Movable.Move.Right);
                script.Globals["nearestEnemy"] = FuncNearestEnemy();
                script.Globals["nearestBattery"] = FuncNearestBattery();
                script.Globals["attackEnemy"] = FuncAttackNearestEnemy();

                coroutine.Coroutine.AutoYieldCounter = 10 * robot.CPU.Speed;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        internal bool Execute()
        {
            try
            {
                Physics.SphereCast(robot.transform.position + Vector3.up * 0.005f, 0.05f, robot.transform.forward,
                    out RaycastHit hitinfo);
                script.Globals["DISTANCE"] = hitinfo.collider ? hitinfo.distance : 1e7f;

                DynValue result = coroutine.Coroutine.Resume();
                return result.Type != DataType.YieldRequest;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return true;
        }

        private Action<float> FuncMove(Movable.Move dir)
        {
            return x => robot.GetComponent<RobotSphere>().Move(dir, (int)x, true);
        }

        private Func<GameObject> FuncNearestEnemy()
        {
            return () =>
            {
                HashSet<GameObject>.Enumerator iter = EnemyManager.Enemies.GetEnumerator();
                GameObject nearestEnemy = iter.Current;
                float nearestDistance = Vector3.Distance(nearestEnemy.transform.position, robot.gameObject.transform.position);

                while (iter.MoveNext())
                {
                    GameObject enemy = iter.Current;
                    float distance = Vector3.Distance(enemy.transform.position, robot.gameObject.transform.position);
                    if (Vector3.Distance(enemy.transform.position, robot.gameObject.transform.position) < distance)
                    {
                        nearestEnemy = enemy;
                        nearestDistance = distance;
                    }
                }

                return nearestEnemy;
            };
        }

        private Func<GameObject> FuncNearestBattery()
        {
            return () =>
            {
                GameObject nearestBattery = BatteryManager.Instance().Batteries[0];
                float nearestDistance = Vector3.Distance(nearestBattery.transform.position, robot.gameObject.transform.position);

                foreach (GameObject battery in BatteryManager.Instance().Batteries)
                {
                    float distance = Vector3.Distance(battery.transform.position, robot.gameObject.transform.position);
                    if (Vector3.Distance(battery.transform.position, robot.gameObject.transform.position) < distance)
                    {
                        nearestBattery = battery;
                        nearestDistance = distance;
                    }
                }

                return nearestBattery;
            };
        }

        private Action FuncAttackNearestEnemy()
        {
            return () =>
            {
                Debug.Log("Func");
                HashSet<GameObject>.Enumerator iter = EnemyManager.Enemies.GetEnumerator();
                iter.MoveNext();
                GameObject nearestEnemy = iter.Current;
                Debug.Log("Second");
                float nearestDistance = Vector3.Distance(nearestEnemy.transform.position, robot.gameObject.transform.position);
                Debug.Log("Third");

                while (iter.MoveNext())
                {
                    GameObject enemy = iter.Current;
                    float distance = Vector3.Distance(enemy.transform.position, robot.gameObject.transform.position);
                    if (Vector3.Distance(enemy.transform.position, robot.gameObject.transform.position) < distance)
                    {
                        nearestEnemy = enemy;
                        nearestDistance = distance;
                    }
                }

                if (nearestDistance < 400.0f)
                {
                    robot.Attack(nearestEnemy);
                }
            };
        }
    }
}