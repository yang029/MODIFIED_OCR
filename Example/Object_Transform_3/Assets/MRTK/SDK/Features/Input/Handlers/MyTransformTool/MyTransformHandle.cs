using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace MyTransformTool
{

    public enum TranslationAxis { X, Y, Z }//变换的三个轴，X、Y、Z
    public enum HandleSerialNumber { No1, No2, No3, No4 }
    public enum HandleType { Translate, Rotate }
   

    public class MyTransformHandle : MonoBehaviour
    {
        

        public TranslationAxis axis;//该控制柄控制的轴
        public HandleSerialNumber SerialNumber;
        public HandleType type
        {
            get
            {
                if (this is MyTransformTranslationHandle) return HandleType.Translate;
                else return HandleType.Rotate;
            }
        }

        protected MyObjectManipulator _intObj;
        protected MyTransformTool _tool;
        protected bool LocalorGlobal { get { return _tool.LocalOrGlobal; } }
        protected Vector3 LocalPosition;
        protected Vector3 GlobalPosition;
        protected Vector3 RelativePosition;
        protected Quaternion LocalRotation;
        protected Quaternion GlobalRotation;
        protected Quaternion RelativeRotation;
        public Transform WorldCoordinateSystem;
        


        //Unity事件
        public UnityEvent OnShouldShowHandle = new UnityEvent();
        public UnityEvent OnShouldHideHandle = new UnityEvent();
        public UnityEvent OnHandleActivated = new UnityEvent();
        public UnityEvent OnHandleDeactivated = new UnityEvent();

        private void Awake()
        {
            _intObj = this.GetComponent<MyObjectManipulator>();
        }

        protected virtual void Start()//使用protected访问修饰符标记的方法或字段，只能在当前类及其子类中使用，有效的防止了外部无意的更改，使得整个框架可靠性更高；使用virtual标记的方法称为虚方法，虚方法可以由设计人员自行决定是否包含方法的实现，配合protected保护特定的方法并且在其子类中可根据业务需求决定是否重写。
        {

            _intObj = this.GetComponent<MyObjectManipulator>();
            _intObj.OnGraspBegin += onGraspBegin;
            _intObj.OnGraspEnd += onGraspEnd;
            _tool = GetComponentInParent<MyTransformTool>();//从父级物体上找对应的TransformTool组件
            if (WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
            if (_tool == null || WorldCoordinateSystem == null) Debug.LogError("No TransformTool found in a parent GameObject.");//如果没有找到，报错
            if(_tool.target != null)
            {
                Init();
            }
            
        }



       

        public void SetHandleTransform()
        {
            this.transform.position = _tool.transform.TransformPoint(RelativePosition);
            this.transform.rotation = _tool.transform.rotation * RelativeRotation;
        }


        // Update is called once per frame
        void Update()
        {

        }



        public void syncRigidbodyWithTransform()//_intObj.rigidbody是与此交互对象关联的刚体，将刚体与变换同步
        {
            if(_intObj.rigidbody != null)
            {
                _intObj.rigidbody.position = this.transform.position;//将其位置与该脚本坐标轴控制柄位置对齐
                _intObj.rigidbody.rotation = this.transform.rotation;//将其旋转与该脚本坐标轴控制柄位置对齐
            }
        }


        private void onGraspBegin()//被抓到时执行
        {
            _tool.NotifyHandleActivated(this);//通知坐标轴控制柄进行激活
            _tool.SetToolTransform();
            if (_tool is MyReplicaTransformTool)
            {
                _tool.GetComponent<MyReplicaTransformTool>().SetRealTargetHandle();
                foreach (var handle in _tool.GetComponent<MyReplicaTransformTool>().RealTarget.GetActiveHandles())
                {
                    handle.OnHandleActivated.Invoke();//显示所有坐标轴控制柄
                }
            }
            OnHandleActivated.Invoke();//调用所有已注册的回调（运行时和持久）

        }

        private void onGraspEnd()//被抓到时执行
        {
            _tool.NotifyHandleDeactivated(this);//通知坐标轴控制柄进行隐藏
            _tool.SetToolTransform();
            if (_tool is MyReplicaTransformTool)
            {
                _tool.GetComponent<MyReplicaTransformTool>().SetRealTargetHandle();
                _tool.GetComponent<MyReplicaTransformTool>().RealTarget.targetTransformStore.Insert(_tool.GetComponent<MyReplicaTransformTool>().RealTarget.target);
                foreach (var handle in _tool.GetActiveHandles())
                {
                    handle.OnHandleDeactivated.Invoke();//显示所有坐标轴控制柄
                }
            }
            if (_tool.targetTransformStore != null)
            {
                _tool.targetTransformStore.Insert(_tool.target);
            }

            OnHandleDeactivated.Invoke(); //调用所有已注册的回调（运行时和持久）。
        }

        #region Handle Visibility

        /// <summary>
        /// Called by the Transform Tool when this handle should be visible.
        /// 当此坐标轴控制柄可见时，由变换工具调用。
        /// </summary>
        public void EnsureVisible()
        {
            OnShouldShowHandle.Invoke();//调用所有已注册的回调（运行时和持久）。
            _intObj.ignoreGrasping = false;

        }

        /// <summary>
        /// Called by the Transform Tool when this handle should not be visible.
        /// 当此坐标轴控制柄不可见时，由转换工具调用。
        /// </summary>
        public void EnsureHidden()
        {
            OnShouldHideHandle.Invoke();
            _intObj.ignoreGrasping = true;
        }

        #endregion

        public void Init()
        {
            if(_tool == null)
            {
                _tool = GetComponentInParent<MyTransformTool>();//从父级物体上找对应的TransformTool组件
            }
            if (WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
            LocalPosition = _tool.transform.InverseTransformPoint(this.transform.position);//物体坐标系中控制柄相对于物体的位置，在物体坐标系中该值一定            
            LocalRotation = Quaternion.Inverse(_tool.transform.rotation) * this.transform.rotation;//物体坐标系中控制柄的四元数

            GlobalPosition = WorldCoordinateSystem.InverseTransformPoint(this.transform.position) - WorldCoordinateSystem.InverseTransformPoint(_tool.transform.position);//全局坐标系中，控制柄相对于物体的位置
            GlobalRotation = WorldCoordinateSystem.rotation * LocalRotation;//全局坐标系中，控制柄的四元数


            RelativePosition = _tool.transform.InverseTransformPoint(this.transform.position);
            RelativeRotation = Quaternion.Inverse(_tool.transform.rotation) * this.transform.rotation;
        }

        public void UpdateGlobal()
        {
            GlobalRotation = WorldCoordinateSystem.rotation * LocalRotation;//全局坐标系中，控制柄的四元数
        }

    }
}

