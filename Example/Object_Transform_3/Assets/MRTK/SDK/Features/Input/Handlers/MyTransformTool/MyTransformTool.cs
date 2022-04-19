using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MyTransformTool
{
    public enum ToolState { Idle, Translating, Rotating }//工具的三种状态，闲置、平移、旋转

    public class MyTransformTool : MonoBehaviour
    {

        
        [Tooltip("The target object to be moved by this tool.")]//此工具要移动的目标对象
        public Transform target;

        protected Vector3 _moveBuffer = Vector3.zero;//平移量
        protected Quaternion _rotateBuffer = Quaternion.identity;//旋转量

        protected HashSet<MyTransformHandle> _transformHandles = new HashSet<MyTransformHandle>();//所有变换用的坐标轴控制柄哈希集


        public ToolState _toolState = ToolState.Idle;//默认工具的状态为闲置
        protected HashSet<MyTransformHandle> _activeHandles = new HashSet<MyTransformHandle>();//被激活的变换用坐标轴控制柄哈希集

        private HashSet<TranslationAxis> _activeTranslationAxes = new HashSet<TranslationAxis>();//被激活的变换坐标轴控制柄

        public bool LocalOrGlobal = true;

        public float TranslationMultiplier = 1.0f;

        public float RotationMultiplier = 1.0f;

        protected Vector3 LocalPosition;
        protected Quaternion LocalRotation;

        protected Vector3 GlobalPosition;
        protected Quaternion GlobalRotation;

        protected Vector3 RelativePosition;
        protected Quaternion RelativeRotation;

        public Transform WorldCoordinateSystem;

        public bool IsControlled = false;

        public MyTransformStore targetTransformStore;

        private void Awake()
        {
            if (GameObject.Find("Diagnostics") != null)
            {
                GameObject.Find("Diagnostics").SetActive(false);
            }

            if (WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
            
            if (targetTransformStore == null)
            {
                targetTransformStore = this.GetComponent<MyTransformStore>();
            }
            // PhysicsCallbacks is useful for creating explicit pre-physics and post-physics
            // behaviour.
            //PhysicCallbacks可用于创建明确的物理前和物理后行为。
            MyPhysicsCallbacks.OnPostPhysics += onPostPhysics;

            foreach (var handle in GetComponentsInChildren<MyTransformHandle>())//将所有的变换用坐标轴控制柄添加到_transformHandles中
            {
                _transformHandles.Add(handle);
            }

            if(target != null)
            {
                Init();
            }            

        }

        // Start is called before the first frame update
        void Start()
        {
            

        }

        // Update is called once per frame
        void Update()
        {
            // Enable or disable handles based on hand proximity and tool state.
            // 根据手接近和刀具状态启用或禁用控制柄。
            updateHandles();
        }

        #region Handle Movement / Rotation

        /// <summary>
        /// Transform handles call this method to notify the tool that they were used
        /// to move the target object.
        /// 变换坐标轴控制柄调用此方法以通知工具移动目标对象。
        /// </summary>
        public void NotifyHandleMovement(Vector3 deltaPosition)
        {
            _moveBuffer += deltaPosition;
        }

        /// <summary>
        /// Transform handles call this method to notify the tool that they were used
        /// to rotate the target object.
        /// 变换坐标轴控制柄调用此方法以通知工具旋转目标对象。
        /// </summary>
        public void NotifyHandleRotation(Quaternion deltaRotation)
        {
            _rotateBuffer = deltaRotation * _rotateBuffer;
        }

        public Quaternion MultiplyQuaternion(Quaternion Rotation, float RotationMultiplier)
        {
            Vector3 axis;
            float angle;
            Rotation.ToAngleAxis(out angle, out axis);
            
            angle *= RotationMultiplier;
            Rotation = Quaternion.AngleAxis(angle, axis);
            return Rotation;
        }

        public void SetToolTransform()
        {
            if (LocalOrGlobal)
            {
                SetToolLocalTransform();
            }
            else
            {
                SetToolGlobalTransform();
            }
            foreach (var handle in _transformHandles)
            {
                handle.SetHandleTransform();
            }
        }

        public HashSet<MyTransformHandle> GetActiveHandles()
        {
            return _activeHandles;
        }


        protected void SetToolLocalTransform()
        {
            this.transform.position = target.transform.TransformPoint(LocalPosition);
            this.transform.rotation = LocalRotation * target.transform.rotation;

        }

        protected void SetToolGlobalTransform()
        {

            this.transform.position = WorldCoordinateSystem.TransformPoint(WorldCoordinateSystem.InverseTransformPoint(target.transform.position) + GlobalPosition);
            this.transform.rotation = GlobalRotation * WorldCoordinateSystem.rotation;
        }

        public void SetToolRelativeTransform()
        {
            this.transform.position = target.transform.TransformPoint(RelativePosition);
            this.transform.rotation = target.transform.rotation * RelativeRotation;
        }

        protected virtual void onPostPhysics()
        {
            // Hooked up via PhysicsCallbacks in Start(), this method will run after
            // FixedUpdate and after PhysX has run. We take the opportunity to immediately
            // manipulate the target object's and this object's transforms using the
            // accumulated information about movement and rotation from the Transform Handles.
            // 通过Start（）中的PhysicCallbacks连接，此方法将在FixedUpdate和PhysX运行后运行。
            // 我们利用变换控制柄中积累的有关移动和旋转的信息，立即操纵目标对象和该对象的变换。
            // Apply accumulated movement and rotation to target object.
            // 将累计移动和旋转应用于目标对象。

            if (!IsControlled)
            {
                this.transform.rotation = _rotateBuffer * this.transform.rotation;
                target.transform.rotation = MultiplyQuaternion(_rotateBuffer, RotationMultiplier) * target.transform.rotation;


                // Match this transform with the target object's (this will move child
                // MyTransformHandles' transforms).
                // 将此变换与目标对象的匹配（这将移动子MyTransformHandles的变换）。
                this.transform.position = this.transform.position + _moveBuffer;

               
                target.transform.position += _moveBuffer * TranslationMultiplier;
            }

            // Explicitly sync MyTransformHandles' rigidbodies with their transforms,
            // which moved along with this object's transform because they are children of it.
            // 显式地将MyTransformHandles的刚体与其变换同步，这些刚体随此对象的变换一起移动，因为它们是该对象的子对象。
            foreach (var handle in _transformHandles)
            {
                handle.SetHandleTransform();
                handle.syncRigidbodyWithTransform();
            }

            // Reset movement and rotation buffers.
            // 重置移动和旋转缓冲区。
            _moveBuffer = Vector3.zero;
            _rotateBuffer = Quaternion.identity;

        }




        #endregion

        #region Handle Visibility

        private void updateHandles()
        {
            switch (_toolState)//根据工具目前的状态对需要执行的操作进行选择
            {
                case ToolState.Idle:
                    if (!IsControlled)
                    {
                       
                        foreach (var handle in _transformHandles)
                        {
                            handle.EnsureVisible();//显示所有坐标轴控制柄
                        }
                    }
                    else
                    {
                        foreach (var handle in _transformHandles)
                        {
                            handle.EnsureHidden();//隐藏其他坐标轴控制柄
                        }
                    }

                    break;

                case ToolState.Translating://当处于平移状态时
                                           // While translating, show all translation handles except the other handle
                                           // on the same axis, and hide rotation handles.
                                           // 平移时，显示除同一轴上另一控制柄以外的所有平移控制柄，并隐藏旋转控制柄。
                    if (!IsControlled)
                    {
                        foreach (var handle in _transformHandles)//遍历所有变换用的坐标轴控制柄
                        {
                            if (handle is MyTransformTranslationHandle)//如果该坐标轴控制柄为平移坐标轴控制柄
                            {
                                var translateHandle = handle as MyTransformTranslationHandle;//获取MyTransformTranslationHandle组件

                                if (!_activeHandles.Contains(translateHandle)
                                    && _activeTranslationAxes.Contains(translateHandle.axis))//如果激活的坐标轴控制柄中不包括该坐标轴控制柄且被激活的轴中包括该坐标轴控制柄代表的轴，即同一轴上的另一控制柄
                                {
                                    handle.EnsureHidden();
                                }
                                else//显示除同一轴上另一控制柄以外的所有平移控制柄
                                {
                                    handle.EnsureVisible();
                                }
                            }
                            else//隐藏旋转控制柄。
                            {
                                handle.EnsureHidden();
                            }
                        }
                    }
                    else
                    {
                        foreach (var handle in _activeHandles)//遍历所有变换用的坐标轴控制柄
                        {
                            if (handle is MyTransformTranslationHandle)//如果该坐标轴控制柄为平移坐标轴控制柄
                            {
                                handle.EnsureVisible();
                            }
                        }
                    }

                    break;

                case ToolState.Rotating:
                    // While rotating, only show the active rotating handle.
                    // 旋转时，仅显示活动的旋转控制柄。
                    foreach (var handle in _transformHandles)//遍历所有的控制柄
                    {
                        if (_activeHandles.Contains(handle))//如果被激活的控制并中包括该控制柄，显示该控制柄
                        {
                            handle.EnsureVisible();
                        }
                        else//隐藏该控制柄
                        {
                            handle.EnsureHidden();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Called by handles when they are grasped.
        /// 当控制柄被抓住时，由控制柄调用。
        /// </summary>
        /// <param name="handle"></param>
        public void NotifyHandleActivated(MyTransformHandle handle)
        {
            switch (_toolState)
            {
                case ToolState.Idle://空闲时执行
                    _activeHandles.Add(handle);//添加该控制柄

                    if (this is MyReplicaTransformTool)
                    {
                        MyReplicaTransformTool _myReplicaTransformTool = this.GetComponent<MyReplicaTransformTool>();
                        MyTransformTool _myOriginTransformTool = _myReplicaTransformTool.RealTarget;
                        foreach (var originhandle in _myOriginTransformTool._transformHandles)
                        {
                            if (handle.axis == originhandle.axis && handle.SerialNumber == originhandle.SerialNumber && handle.type == originhandle.type)
                            {
                                _myOriginTransformTool.NotifyHandleActivated(originhandle);
                            }
                        }
                    }

                    if (handle is MyTransformTranslationHandle)//根据该控制柄的脚本判断是平移控制并还是旋转控制
                    {
                        _toolState = ToolState.Translating;
                        _activeTranslationAxes.Add(((MyTransformTranslationHandle)handle).axis);
                    }
                    else
                    {
                        _toolState = ToolState.Rotating;
                    }



                    break;

                case ToolState.Translating:
                    if (handle is MyTransformRotationHandle)
                    {
                        Debug.LogError("Error: Can't rotate a transform while it is already being "
                                     + "translated.");
                    }
                    else
                    {
                        _activeHandles.Add(handle);
                        _activeTranslationAxes.Add(((MyTransformTranslationHandle)handle).axis);
                        if (this is MyReplicaTransformTool)
                        {
                            MyReplicaTransformTool _myReplicaTransformTool = this.GetComponent<MyReplicaTransformTool>();
                            MyTransformTool _myOriginTransformTool = _myReplicaTransformTool.RealTarget;
                            foreach (var originhandle in _myOriginTransformTool._transformHandles)
                            {
                                if (handle.axis == originhandle.axis && handle.SerialNumber == originhandle.SerialNumber && handle.type == originhandle.type)
                                {
                                    _myOriginTransformTool.NotifyHandleActivated(originhandle);
                                }
                            }
                        }
                    }
                    break;

                case ToolState.Rotating:
                    Debug.LogError("Error: Only one handle can be active while a transform is being "
                                 + "rotated.");
                    break;
            }
        }

        /// <summary>
        /// Called by Handles when they are released.
        /// 释放操作柄时由操作柄调用。
        /// </summary>
        public void NotifyHandleDeactivated(MyTransformHandle handle)
        {
            if (handle is MyTransformTranslationHandle)
            {
                _activeTranslationAxes.Remove(((MyTransformTranslationHandle)handle).axis);
            }

            _activeHandles.Remove(handle);



            switch (_toolState)
            {
                case ToolState.Idle:
                    Debug.LogWarning("Warning: Handle was deactived while Tool was already idle.");
                    break;

                default:
                    if (_activeHandles.Count == 0)
                    {
                        _toolState = ToolState.Idle;
                    }
                    break;
            }

            if (this is MyReplicaTransformTool)
            {
                MyReplicaTransformTool _myReplicaTransformTool = this.GetComponent<MyReplicaTransformTool>();
                MyTransformTool _myOriginTransformTool = _myReplicaTransformTool.RealTarget;
                foreach (var originhandle in _myOriginTransformTool._transformHandles)
                {
                    if (handle.axis == originhandle.axis && handle.SerialNumber == originhandle.SerialNumber && handle.type == originhandle.type)
                    {
                        _myOriginTransformTool.NotifyHandleDeactivated(originhandle);
                    }
                }
                _myOriginTransformTool._toolState = _toolState;
            }

        }

        public void SetTargetTransform(Vector3 Translation, Quaternion Rotation)
        {
            if (IsControlled)
            {
                _moveBuffer = Translation;
                _rotateBuffer = Rotation;
                // Apply accumulated movement and rotation to target object.
                // 将累计移动和旋转应用于目标对象。

                this.transform.rotation = MultiplyQuaternion(_rotateBuffer, RotationMultiplier) * this.transform.rotation;
                target.transform.rotation = MultiplyQuaternion(_rotateBuffer, RotationMultiplier) * target.transform.rotation;


                // Match this transform with the target object's (this will move child
                // MyTransformHandles' transforms).
                // 将此变换与目标对象的匹配（这将移动子MyTransformHandles的变换）。
                this.transform.position += _moveBuffer * TranslationMultiplier;
                target.transform.position += _moveBuffer * TranslationMultiplier;


                // Explicitly sync MyTransformHandles' rigidbodies with their transforms,
                // which moved along with this object's transform because they are children of it.
                // 显式地将MyTransformHandles的刚体与其变换同步，这些刚体随此对象的变换一起移动，因为它们是该对象的子对象。
                foreach (var handle in _transformHandles)
                {
                    handle.syncRigidbodyWithTransform();
                }

                // Reset movement and rotation buffers.
                // 重置移动和旋转缓冲区。
                _moveBuffer = Vector3.zero;
                _rotateBuffer = Quaternion.identity;
            }

        }

        public void Init()
        {
            

            RelativePosition = Vector3.zero;
            RelativeRotation = Quaternion.identity;

            SetToolRelativeTransform();

            AdaptSize();

            LocalPosition = Vector3.zero;//物体坐标系中控制柄相对于物体的位置，在物体坐标系中该值一定
            LocalRotation = Quaternion.identity;//物体坐标系中控制柄的四元数



            GlobalPosition = WorldCoordinateSystem.InverseTransformPoint(this.transform.position) - WorldCoordinateSystem.InverseTransformPoint(target.transform.position);//全局坐标系中，控制柄相对于物体的位置
            GlobalRotation = Quaternion.identity;//全局坐标系中，控制柄的四元数
            targetTransformStore.Insert(target);

            foreach (var handle in _transformHandles)
            {
                handle.Init(); 
            }
        }

        void AdaptSize()
        {
            Mesh mesh = target.GetComponent<MeshFilter>().mesh;
            Vector3 center = mesh.bounds.center;
            
            if(center != Vector3.zero)
            {
                Vector3[] vertices = mesh.vertices;
                for(int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] -= center; 
                }
            }

            Vector3 size = mesh.bounds.size;
            float sizemin = float.MaxValue;
            size.x *= target.lossyScale.x;
            sizemin = size.x;
            size.y *= target.lossyScale.y;
            sizemin = sizemin < size.y ? sizemin : size.y;
            size.z *= target.lossyScale.z;
            sizemin = sizemin < size.z ? sizemin : size.z;

            if(sizemin < 0.15f)
            {
                size = size * (0.15f / sizemin);
            }

            foreach (var handle in _transformHandles)
            {
                Vector3 localposition = this.transform.InverseTransformPoint(handle.transform.position);

                localposition.x *= size.x;
                localposition.y *= size.y;
                localposition.z *= size.z;

                handle.transform.position = this.transform.TransformPoint(localposition);
            }           

        }

        #endregion

        public void UpdateGlobal()
        {
            foreach (var handle in _transformHandles)
            {
                handle.UpdateGlobal();
            }
            SetToolTransform();            
        }

    }
}


