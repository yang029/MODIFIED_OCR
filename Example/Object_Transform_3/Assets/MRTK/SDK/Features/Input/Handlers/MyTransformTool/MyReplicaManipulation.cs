using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTransformTool
{
    public class MyReplicaManipulation : MonoBehaviour
    {
        public Transform target;
        private Vector3 LastPosition;
        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            target.rotation = this.transform.rotation;
            target.position += this.transform.position - LastPosition;
            LastPosition = this.transform.position;
        }

        public void Init()
        {
            this.transform.rotation = target.rotation;
            LastPosition = this.transform.position;
        }


    }
}


