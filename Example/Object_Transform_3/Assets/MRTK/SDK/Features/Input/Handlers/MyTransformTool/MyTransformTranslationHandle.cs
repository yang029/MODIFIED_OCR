using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyTransformTool
{
    public class MyTransformTranslationHandle : MyTransformHandle
    {
        protected override void Start()
        {
            // Populates _intObj with the InteractionBehaviour, and _tool with the
            // TransformTool.
            // 用InteractionBehavior填充_intObj，用TransformTool填充_工具。
            base.Start();

            // Subscribe to OnGraspedMovement; all of the logic will happen when the handle is
            // moved via grasping.
            // 支持OnGraspedMovement；通过抓取移动操作柄时，所有逻辑都会发生。
            /*
             * 在被抓取对象的刚体的位置和旋转由其当前抓取控制器设置后直接调用。
             * 如果要覆盖抓取对象的默认行为（例如，约束对象的位置或旋转），请订阅此回调。
             * 
             * 使用InteractionBehavior。刚体。位置和互动行为。
             * 刚体。旋转以设置对象的位置和旋转。
             * 不建议仅设置对象变换的位置和旋转，除非您了解其差异。
             * 
             * 此方法在任何OnGraspBegin或OngRasppend回调后调用，但在OngRaspTay之前调用。
             * 在OnGraspStay中移动交互对象（通过其刚体）也是有效的，尽管OnGraspStay在其回调签名中不提供预解算和后解算数据。
             * 
             */
            _intObj.OnGraspedManipulation += onGraspedMovement;

        }

        private void onGraspedMovement(Vector3 presolvePos, Quaternion presolveRot,Vector3 solvedPos, Quaternion solvedRot)
        {
            /* 
             * OnGraspedMovement provides the position and rotation of the Interaction object
             * before and after it was moved by its grasping hand. This callback only occurs
             * when one or more hands is grasping the Interaction object. In this case, we
             * don't care about how many or which hands are grasping the object, only where
             * the object is moved.
             * 
             * OnGraspedMovement提供交互对象在其抓取手移动前后的位置和旋转。
             * 此回调仅在一只或多只手抓住交互对象时发生。
             * 在这种情况下，我们不关心有多少手或哪只手抓住了物体，只关心物体移动的位置。
             * 
             * The Translation Handle uses the pre- and post-solve movement information to
             * calculate how the user is trying to move the object along this handle's forward
             * direction. Then the Translation Handle will simply override the movement caused
             * by the grasping hand and reset itself back to its original position.
             * 
             * 平移控制柄使用“求解前”和“求解后”移动信息来计算用户试图如何沿该控制柄的前进方向移动对象。
             * 然后，平移手柄将简单地覆盖抓取手引起的移动，并将自身重置回其原始位置。
             * 
             * The movement calculated by the Handle in this method is reported to the Transform
             * Tool, which accumulates movement caused by all Handles over the course of a frame
             * and then moves the target object and all of its child Handles appropriately at
             * the end of the frame.
             * 
             * 此方法中由控制柄计算的移动将报告给TransformTool，
             * TransformTool将在帧过程中累积所有控制柄引起的移动，
             * 然后在帧结束时适当移动目标对象及其所有子控制柄。
             * 
             */


            // Calculate the constrained movement of the handle along its forward axis only.
            // 仅计算坐标轴控制柄沿其前进轴的受约束移动。
            Vector3 deltaPos = solvedPos - presolvePos;
            Vector3 handleForwardDirection = LocalorGlobal ? presolveRot * Vector3.forward : GlobalRotation * Vector3.forward;
            Vector3 deltaAxisPos = handleForwardDirection * Vector3.Dot(handleForwardDirection, deltaPos);

            // Notify the tool about the calculated movement.
            // 将计算的移动发送给工具组件，工具组件将移动存入缓存区。
            _tool.NotifyHandleMovement(deltaAxisPos);


            // In this case, the tool itself will accumulate delta positions and delta rotations
            // from all handles, and will then synchronize handles to the appropriate positions and
            // rotations at the end of the frame.
            //在这种情况下，工具组件本身将累积所有控制柄的增量位置和增量旋转，然后将控制柄同步到帧末尾的适当位置和旋转。


            // Because the Tool will be the one to actually move this Handle, all we have left to do
            // is to undo all of the motion caused by the grasping hand.
            // 因为工具组件会移动该控制柄，所以我们所要做的就是撤销由抓握的手引起的所有运动。
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