﻿using UnityEngine;

namespace FIVE
{
    public class RobotFreeAnim
    {
        private readonly GameObject gameObject;
        private readonly Animator anim;

        public RobotFreeAnim(GameObject gameObject)
        {
            this.gameObject = gameObject;
            anim = gameObject.GetComponent<Animator>();
        }

        // Update is called once per frame
        public void Update(RobotSphere.RobotState currState)
        {
            UpdateAnim(currState);
        }

        private void UpdateAnim(RobotSphere.RobotState currState)
        {
            if (currState == RobotSphere.RobotState.Idle)
            {
                anim.SetBool("Walk_Anim", false);
            }
            else if (currState == RobotSphere.RobotState.Walk)
            {
                anim.SetBool("Walk_Anim", true);
            }
        }
    }

}
