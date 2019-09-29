﻿using FIVE.Robot;
using System.Collections.Generic;
using UnityEngine;

namespace FIVE.AWSL
{
    internal class RuntimeContext
    {
        public RobotSphere Robot;
        public List<Expr> Exprs;
        public int ExprP = 0;
        public Dictionary<string, object> Vars = new Dictionary<string, object>();
        public Dictionary<string, Expr> Funcs = new Dictionary<string, Expr>();

        public bool Execute()
        {
            if (ExprP < Exprs.Count)
            {
                Physics.SphereCast(Robot.transform.position + Vector3.up * 0.005f, 0.05f, Robot.transform.forward, out RaycastHit hitinfo);
                Vars["DISTANCE"] = hitinfo.collider ? hitinfo.distance : 1e7f;

                Exprs[ExprP++].Execute(this);
                Robot.currState = RobotSphere.RobotState.Walk;
            }
            if (ExprP >= Exprs.Count)
            {
                Robot.currState = RobotSphere.RobotState.Idle;
                return true;
            }
            return false;
        }
    }
}