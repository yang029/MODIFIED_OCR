using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTransformTool
{
    public class MyTransformRotationHandle : MyTransformHandle
    {
        private Quaternion Check = Quaternion.identity;

        protected override void Start()
        {
            // Populates _intObj with the InteractionBehaviour, and _tool with the TransformTool.
            // 用InteractionBehavior填充_intObj，用TransformTool填充_工具。
            base.Start();

            // Subscribe to OnGraspedMovement; all of the logic will happen when the handle is moved via grasping.
            // 支持OnGraspedMovement；通过抓取移动手柄时，所有逻辑都会发生。
            _intObj.OnGraspedManipulation += onGraspedMovement;
        }

        private void onGraspedMovement(Vector3 presolvePos, Quaternion presolveRot, Vector3 solvedPos, Quaternion solvedRot)
        {
            /* 
             * The RotationHandle works very similarly to the TranslationHandle.
             * 旋转控制柄的工作原理与TranslationHandle非常相似。
             * 
             * We use OnGraspedMovement to get the position and rotation of this object
             * before and after it was moved by its grapsing hand. We calculate how the handle
             * would have rotated and report that to the Transform Tool, and then we move
             * the handle back where it was before it was moved, because the Tool will
             * actually move all of its handles at the end of the frame.
             * 我们使用OnGraspedMovement来获取该对象在被其手移动之前和之后的位置和旋转。
             * 我们计算控制柄的旋转方式并将其报告给变换工具，然后将控制柄移回移动前的位置，
             * 因为该工具实际上会在帧的末尾移动其所有控制柄。
             */

            // Constrain the position of the handle and determine the resulting rotation required to get there.
            // 约束控制柄的位置并确定到达该位置所需的最终旋转。


            Vector3 presolveToolToHandleDirection = (presolvePos - _tool.transform.position).normalized;//旋转前手与坐标原点的矢量
            Vector3 solvedToolToHandleDirection = (solvedPos - _tool.transform.position).normalized;//旋转后手与坐标原点的矢量

            Vector3 presolveToolToHandleOnPlane = LocalorGlobal ?
                Vector3.ProjectOnPlane(presolveToolToHandleDirection, (presolveRot * Vector3.up)).normalized:
                Vector3.ProjectOnPlane(presolveToolToHandleDirection, (GlobalRotation * Vector3.up)).normalized;//将solvedToolToHandleDirection投影在局部的XoZ平面上

            Vector3 solvedToolToHandleOnPlane = LocalorGlobal ?
                Vector3.ProjectOnPlane(solvedToolToHandleDirection, (presolveRot * Vector3.up)).normalized * presolveToolToHandleDirection.magnitude :
                Vector3.ProjectOnPlane(solvedToolToHandleDirection, (GlobalRotation * Vector3.up)).normalized * presolveToolToHandleDirection.magnitude;//将solvedToolToHandleDirection投影在局部的平面上



            Quaternion deltaRotation = Quaternion.FromToRotation(presolveToolToHandleOnPlane, solvedToolToHandleOnPlane);//求两个矢量的夹角



            //Quaternion deltaRotation = Quaternion.identity;//求两个矢量的夹角
            // Notify the tool about the calculated rotation.
            // 将计算量传给工具组件
            _tool.NotifyHandleRotation(deltaRotation);


            // Move the object back to its original position, to be moved correctly later on by the Transform Tool.
            // 将对象移回其原始位置，以便稍后通过变换工具正确移动。
            if (_intObj.rigidbody != null)
            {
                _intObj.rigidbody.position = presolvePos;
                _intObj.rigidbody.rotation = presolveRot;
            }
            _intObj.HostTransform.position = presolvePos;
            _intObj.HostTransform.rotation = presolveRot;
        }
    }
}


