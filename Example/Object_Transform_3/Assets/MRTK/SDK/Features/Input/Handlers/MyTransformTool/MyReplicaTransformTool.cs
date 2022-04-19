using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTransformTool
{
    public class MyReplicaTransformTool : MyTransformTool
    {

        public MyTransformTool RealTarget;
        //public new bool LocalOrGlobal { get{ return RealTarget.LocalOrGlobal; } }
        //public new float TranslationMultiplier { get{ return RealTarget.TranslationMultiplier; } }

        //public new float RotationMultiplier { get { return RealTarget.RotationMultiplier; } }
        protected override void onPostPhysics()
        {
            if (RealTarget != null)
            {
                RealTarget.IsControlled = true;
                
                RealTarget.SetTargetTransform(_moveBuffer, _rotateBuffer);

            }
            IsControlled = false;
            base.onPostPhysics();
        }

        public void SetRealTargetHandle()
        {
            LocalOrGlobal = RealTarget.LocalOrGlobal;
            TranslationMultiplier = RealTarget.TranslationMultiplier;
            RotationMultiplier = RealTarget.RotationMultiplier;
            RealTarget.SetToolTransform();
        }


    }

}
