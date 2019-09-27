﻿using FIVE.Robot;
using System.Collections.Generic;

namespace FIVE.AWSL
{
    internal class RuntimeContext
    {
        public RobotSphere Robot;
        public List<Expr> Exprs;
        public int ExprP = 0;
        public Dictionary<string, object> Vars = new Dictionary<string, object>();

        public bool Execute()
        {
            if (ExprP < Exprs.Count)
            {
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