using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyTransformTool

{
    public class MyTransformStore : MonoBehaviour
    {
        public struct TransformInformation
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        public TransformInformation GetTransformInformation(Transform target)
        {
            return new TransformInformation() { position = target.transform.position, rotation = target.transform.rotation };
        }

        public void SetTransformInformation(Transform target, TransformInformation Information)
        {
            target.position = Information.position;
            target.rotation = Information.rotation;
        }

        public List<TransformInformation> TransformInformationStored = new List<TransformInformation>();
        public MyTransformTool _tool;
        private int curstep = -1;

        private void Awake()
        {
            if (_tool == null)
            {
                _tool = this.GetComponent<MyTransformTool>();
            }
            if(_tool.target != null)
            {
                Insert(_tool.target);
            }  
        }

        public void InitStore()
        {
            TransformInformationStored.Clear();
            curstep = -1;
            if (_tool.target != null)
            {
                Insert(_tool.target);
            }
        }


        public void Insert(Transform target)
        {
            if (curstep == TransformInformationStored.Count - 1)
            {
                TransformInformationStored.Add(GetTransformInformation(target));
                curstep++;
            }
            else
            {
                TransformInformationStored.RemoveRange(curstep + 1, TransformInformationStored.Count - curstep - 1);
                TransformInformationStored.Add(GetTransformInformation(target));
                curstep++;
            }
        }

        public void UnDo()
        {
            if (curstep - 1 >= 0)
            {
                SetTransformInformation(_tool.target, TransformInformationStored[curstep - 1]);
                _tool.SetToolTransform();
                curstep--;
            }
            else
            {
                Debug.Log("The current Transform is already in the initial state!");
            }

        }

        public void ReDo()
        {
            if (curstep < TransformInformationStored.Count - 1)
            {
                SetTransformInformation(_tool.target, TransformInformationStored[curstep + 1]);
                _tool.SetToolTransform();
                curstep++;
            }
            else
            {
                Debug.Log("The current Transform is already in the final state!");
            }
        }

        public void InsertDirectManipulation(Microsoft.MixedReality.Toolkit.UI.ManipulationEventData eventData)
        {
            Insert(_tool.target);
        }
    }
}

