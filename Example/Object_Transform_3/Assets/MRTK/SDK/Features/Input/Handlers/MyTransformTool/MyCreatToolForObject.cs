using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MyTransformTool
{
    public class MyCreatToolForObject : MonoBehaviour
    {
        public GameObject TransformTool;
        public GameObject ReplicaTransformTool;

        private GameObject transformtoolObj;
        private MyTransformTool transformtool;

        private GameObject targetReplica;
        private GameObject transformtoolReplica;
        private MyReplicaTransformTool Replicatransformtool;
        public Transform WorldCoordinateSystem;

        private MyTransformToolUI UIController;

        private void Awake()
        {
            UIController = this.GetComponent<MyTransformToolUI>();
        }


        public void CreatToolForObject(GameObject target)
        {
            target.name = "Target";
            GameObject transformtoolObj = GameObject.Instantiate(TransformTool, target.transform.position, target.transform.rotation);
            MyTransformTool transformtool = transformtoolObj.GetComponent<MyTransformTool>();
            UIController.TransformToolList.Add(transformtool);
            UIController.UpdateTransformStore();


            transformtool.target = target.transform;
            UIController.DirectTarget = target;
            UIController.RealTarget = target;
            target.AddComponent<ObjectManipulator>().OnManipulationEnded.AddListener(transformtoolObj.GetComponent<MyTransformStore>().InsertDirectManipulation);
            target.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(UIController.UpdateInteractionWithTargetNum);
            target.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(UIController.UpdateDirectInteractionWithTargetNum);
            target.GetComponent<ObjectManipulator>().OnHoverEntered.AddListener(UIController.SetTargetToActivationColor);
            target.GetComponent<ObjectManipulator>().OnHoverExited.AddListener(UIController.SetTargetToOriginalColor);
            target.AddComponent<MoveAxisConstraint>().ConstraintOnMovement = 0;
            target.AddComponent<RotationAxisConstraint>().ConstraintOnRotation = 0;
            target.AddComponent<NearInteractionGrabbable>();
            foreach (var handle in transformtool.GetComponentsInChildren<MyTransformHandle>())
            {
                handle.GetComponent<MyObjectManipulator>().OnManipulationEnded.AddListener(UIController.UpdateInteractionWithTargetNum);
            }
            transformtool.Init();
            transformtool.IsControlled = false;
            transformtool.SetToolTransform();
            UIController.UpdateUI();

            transformtoolObj = null;
            transformtool = null;
        }

        public void CreatReplicaToolForObject(GameObject target)
        {
            target.name = "Target";
            GameObject transformtoolObj = GameObject.Instantiate(TransformTool, target.transform.position, target.transform.rotation);
            MyTransformTool transformtool = transformtoolObj.GetComponent<MyTransformTool>();
            UIController.TransformToolList.Add(transformtool);
            transformtool.target = target.transform;
            UIController.RealTarget = target;
            UIController.DirectTarget = target;
            transformtool.Init();
            transformtool.SetToolTransform();
            transformtool.IsControlled = true;


            GameObject targetReplica = GameObject.Instantiate(target, ComputeWorldPosition(UIController.ReplicPosition, WorldCoordinateSystem), target.transform.rotation);
            targetReplica.name = "Replica";
            GameObject transformtoolReplica = GameObject.Instantiate(ReplicaTransformTool, targetReplica.transform.position, targetReplica.transform.rotation);
            MyReplicaTransformTool Replicatransformtool = transformtoolReplica.GetComponent<MyReplicaTransformTool>();            
            UIController.TransformToolList.Add(Replicatransformtool);
            UIController.UpdateTransformStore();
            Mesh mesh = target.GetComponent<MeshFilter>().mesh;
            Vector3 size = mesh.bounds.size;

            float sizemax = float.MinValue;
            size.x *= target.transform.lossyScale.x;
            sizemax = size.x;
            size.y *= target.transform.lossyScale.y;
            sizemax = sizemax > size.y ? sizemax : size.y;
            size.z *= target.transform.lossyScale.z;
            sizemax = sizemax > size.z ? sizemax : size.z;

            if (sizemax > 0.15f)
            {
                targetReplica.transform.localScale = targetReplica.transform.localScale / (sizemax / 0.15f);
            }

            Replicatransformtool.target = targetReplica.transform;
            UIController.DirectTarget = targetReplica;
            Replicatransformtool.RealTarget = transformtool;

            targetReplica.AddComponent<ObjectManipulator>().OnManipulationEnded.AddListener(transformtoolObj.GetComponent<MyTransformStore>().InsertDirectManipulation);
            targetReplica.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(transformtoolReplica.GetComponent<MyTransformStore>().InsertDirectManipulation);
            targetReplica.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(UIController.UpdateReplicPosition);
            targetReplica.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(UIController.ReturnReplicPosition);
            targetReplica.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(UIController.UpdateInteractionWithTargetNum);
            targetReplica.GetComponent<ObjectManipulator>().OnManipulationEnded.AddListener(UIController.UpdateDirectInteractionWithTargetNum);
            targetReplica.GetComponent<ObjectManipulator>().OnHoverEntered.AddListener(UIController.SetTargetToActivationColor);
            targetReplica.GetComponent<ObjectManipulator>().OnHoverExited.AddListener(UIController.SetTargetToOriginalColor);
            foreach (var handle in Replicatransformtool.GetComponentsInChildren<MyTransformHandle>())//将所有的变换用坐标轴控制柄添加到_transformHandles中
            {
                handle.GetComponent<MyObjectManipulator>().OnManipulationEnded.AddListener(UIController.ReturnReplicPosition);
                handle.GetComponent<MyObjectManipulator>().OnManipulationEnded.AddListener(UIController.UpdateInteractionWithTargetNum);

            }
            targetReplica.GetComponent<ObjectManipulator>().AllowFarManipulation = false;
            targetReplica.AddComponent<MoveAxisConstraint>().ConstraintOnMovement = 0;
            targetReplica.AddComponent<RotationAxisConstraint>().ConstraintOnRotation = 0;
            targetReplica.AddComponent<MyReplicaManipulation>().target = target.transform;
            targetReplica.AddComponent<NearInteractionGrabbable>();

            Replicatransformtool.Init();
            Replicatransformtool.SetToolTransform();
            Replicatransformtool.IsControlled = false;
            UIController.UpdateUI();
        }

        private Vector3 ComputeWorldPosition(Vector3 position, Transform Father)
        {

            return Father.TransformPoint(position);
        }

        private Quaternion ComputeWorldRotation(Quaternion rotation, Transform Father)
        {
            return Father.rotation * rotation;
        }

        private Vector3 ComputeLocalPosition(Vector3 position, Transform Father)
        {

            return Father.InverseTransformPoint(position);
        }

        private Quaternion ComputeLocalRotation(Quaternion rotation, Transform Father)
        {
            return Quaternion.Inverse(Father.rotation) * rotation;
        }



    }
}


