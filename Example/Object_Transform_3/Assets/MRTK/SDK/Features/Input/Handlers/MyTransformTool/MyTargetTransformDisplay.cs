using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyTransformTool
{
    public class MyTargetTransformDisplay : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] MyTransformLabel positionLabel;
        [SerializeField] MyTransformLabel rotationLabel;

        public MyTransformToolUI UIController;

        // Start is called before the first frame update
        void Start()
        {
            if (UIController == null)
            {
                UIController = GameObject.FindObjectOfType<MyTransformToolUI>();
            }
            if (target == null && UIController != null)
            {
                target = UIController.TransformToolList[0].target;
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (target != null)
            {
                positionLabel.SetXYZ(target.transform.position);
                rotationLabel.SetXYZ(target.transform.rotation.eulerAngles);
            }
        }
    }
}


