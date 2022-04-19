using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.Physics;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Microsoft.MixedReality.Toolkit.Physics;
using UnityEngine.Events;

/// <summary>
/// This script allows for an object to be movable, scalable, and rotatable with one or two hands.
/// You may also configure the script on only enable certain manipulations. The script works with
/// both HoloLens' gesture input and immersive headset's motion controller input.
/// 
/// 该脚本允许一个对象可以用一只手或两只手移动、缩放和旋转。您还可以仅在上配置脚本以启用某些操作。
/// 该脚本可与HoloLens的手势输入和沉浸式耳机的运动控制器输入一起使用。
/// 
/// </summary>


namespace MyTransformTool
{

    [HelpURL("https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/ux-building-blocks/object-manipulator")]
    [RequireComponent(typeof(ConstraintManager))]

    public class MyObjectManipulator : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealityFocusChangedHandler, IMixedRealitySourcePoseHandler
    {

        #region Public Enums

        /// <summary>
        /// Describes what pivot the manipulated object will rotate about when
        /// you rotate your hand. This is not a description of any limits or
        /// additional rotation logic. If no other factors (such as constraints)
        /// are involved, rotating your hand by an amount should rotate the object
        /// by the same amount.
        /// 
        /// 描述当您旋转手时，操纵对象将围绕哪个轴旋转。
        /// 这不是对任何限制或附加旋转逻辑的描述。
        /// 如果不涉及其他因素（如约束），则将手旋转一定量应使对象旋转相同的量。
        /// 
        /// For example a possible future value here is RotateAboutUserDefinedPoint
        /// where the user could specify a pivot that the object is to rotate
        /// around.
        /// 
        /// 例如，此处可能的未来值是RotateAboutUserDefinedPoint，用户可以在其中指定对象要围绕的轴。
        /// 
        /// An example of a value that should not be found here is MaintainRotationToUser
        /// as this restricts rotation of the object when we rotate the hand.
        /// 
        /// 此处不应找到的一个值示例是MaintaintRotationToUser，因为这会在旋转手时限制对象的旋转。
        /// 
        /// </summary>
        public enum RotateInOneHandType
        {
            RotateAboutObjectCenter,
            RotateAboutGrabPoint
        };
        [System.Flags]
        public enum ReleaseBehaviorType
        {
            KeepVelocity = 1 << 0,
            KeepAngularVelocity = 1 << 1
        }


        


        #endregion Public Enums

        #region Serialized Fields

        [SerializeField]
        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        // 将被拖动的变换。默认为组件的对象
        private Transform hostTransform = null;

        /// <summary>
        /// Transform that will be dragged. Defaults to the object of the component.
        /// 将被拖动的变换。默认为组件的对象。
        /// </summary>
        public Transform HostTransform
        {
            get
            {
                if (hostTransform == null)
                {
                    hostTransform = gameObject.transform;
                }

                return hostTransform;
            }
            set => hostTransform = value;
        }

        [SerializeField]
        [EnumFlags]
        [Tooltip("Can manipulation be done only with one hand, only with two hands, or with both?")]
        // 操纵只能用一只手，只能用两只手，还是同时用两只手？
        private ManipulationHandFlags manipulationType = ManipulationHandFlags.OneHanded | ManipulationHandFlags.TwoHanded;

        /// <summary>
        /// Can manipulation be done only with one hand, only with two hands, or with both?
        /// 操纵只能用一只手，只能用两只手，还是同时用两只手？
        /// </summary>
        public ManipulationHandFlags ManipulationType
        {
            get => manipulationType;
            set => manipulationType = value;
        }

        [SerializeField]
        [EnumFlags]
        [Tooltip("What manipulation will two hands perform?")]
        //双手将执行什么操作？
        private TransformFlags twoHandedManipulationType = TransformFlags.Move | TransformFlags.Rotate | TransformFlags.Scale;

        /// <summary>
        /// What manipulation will two hands perform?
        /// 双手将执行什么操作？
        /// </summary>
        public TransformFlags TwoHandedManipulationType
        {
            get => twoHandedManipulationType;
            set => twoHandedManipulationType = value;
        }

        [SerializeField]
        [Tooltip("Specifies whether manipulation can be done using far interaction with pointers.")]
        private bool allowFarManipulation = true;

        /// <summary>
        /// Specifies whether manipulation can be done using far interaction with pointers.
        /// 指定是否可以使用与指针的远端交互来完成操作。
        /// </summary>
        public bool AllowFarManipulation
        {
            get => allowFarManipulation;
            set => allowFarManipulation = value;
        }

        [SerializeField]
        [Tooltip(
             "Whether physics forces are used to move the object when performing near manipulations. " +
             "Off will make the object feel more directly connected to the hand. On will honor the mass and inertia of the object. " +
             "The default is off.")]
        private bool useForcesForNearManipulation = false;

        /// <summary>
        /// Whether physics forces are used to move the object when performing near manipulations.
        /// 执行近距离操纵时是否使用物理力移动对象。
        /// </summary>
        /// <remarks>
        /// <para>Setting this to <c>false</c> will make the object feel more directly connected to the
        /// users hand. Setting this to <c>true</c> will honor the mass and inertia of the object,
        /// but may feel as though the object is connected through a spring. The default is <c>false</c>.</para>
        /// >将此设置为<c>false</c>将使对象感觉更直接地连接到用户的手。
        /// 将此设置为<c>true</c>将考虑对象的质量和惯性，但可能感觉对象通过弹簧连接。默认值为<c>false</c></段落>
        /// </remarks>
        public bool UseForcesForNearManipulation
        {
            get => useForcesForNearManipulation;
            set => useForcesForNearManipulation = value;
        }

        [SerializeField]
        [Tooltip("Rotation behavior of object when using one hand near")]
        //在附近使用一只手时对象的旋转行为
        private RotateInOneHandType oneHandRotationModeNear = RotateInOneHandType.RotateAboutGrabPoint;

        /// <summary>
        /// Rotation behavior of object when using one hand near
        /// 在附近使用一只手时对象的旋转行为
        /// </summary>
        public RotateInOneHandType OneHandRotationModeNear
        {
            get => oneHandRotationModeNear;
            set => oneHandRotationModeNear = value;
        }

        [SerializeField]
        [Tooltip("Rotation behavior of object when using one hand at distance")]
        //远距离使用一只手时对象的旋转行为
        private RotateInOneHandType oneHandRotationModeFar = RotateInOneHandType.RotateAboutGrabPoint;

        /// <summary>
        /// Rotation behavior of object when using one hand at distance
        /// 远距离使用一只手时对象的旋转行为
        /// </summary>
        public RotateInOneHandType OneHandRotationModeFar
        {
            get => oneHandRotationModeFar;
            set => oneHandRotationModeFar = value;
        }

        [SerializeField]
        [EnumFlags]
        [Tooltip("Rigid body behavior of the dragged object when releasing it.")]
        //释放被拖动对象时的刚体行为。
        private ReleaseBehaviorType releaseBehavior = ReleaseBehaviorType.KeepVelocity | ReleaseBehaviorType.KeepAngularVelocity;

        /// <summary>
        /// Rigid body behavior of the dragged object when releasing it.
        /// 释放被拖动对象时的刚体行为。
        /// </summary>
        public ReleaseBehaviorType ReleaseBehavior
        {
            get => releaseBehavior;
            set => releaseBehavior = value;
        }

        /// <summary>
        /// Obsolete: Whether to enable frame-rate independent smoothing.
        /// 过时：是否启用与帧速率无关的平滑。
        /// </summary>
        [Obsolete("SmoothingActive is obsolete and will be removed in a future version. Applications should use SmoothingFar, SmoothingNear or a combination of the two.")]
        //SmoothingActive已过时，将在将来的版本中删除。应用程序应使用SmoothingFar、SmoothingNear或两者的组合。
        public bool SmoothingActive
        {
            get => smoothingFar;
            set => smoothingFar = value;
        }

        [FormerlySerializedAs("smoothingActive")]
        [SerializeField]
        [Tooltip("Frame-rate independent smoothing for far interactions. Far smoothing is enabled by default.")]
        //与帧速率无关的平滑，用于远距离交互。默认情况下启用远平滑。
        private bool smoothingFar = true;

        /// <summary>
        /// Whether to enable frame-rate independent smoothing for far interactions.
        /// 是否为远端交互启用与帧速率无关的平滑。
        /// </summary>
        /// <remarks>
        /// Far smoothing is enabled by default.
        /// 默认情况下启用远平滑。
        /// </remarks>
        public bool SmoothingFar
        {
            get => smoothingFar;
            set => smoothingFar = value;
        }

        [SerializeField]
        [Tooltip("Frame-rate independent smoothing for near interactions. Note that enabling near smoothing may be perceived as being 'disconnected' from the hand.")]
        //与帧速率无关的近交互平滑。请注意，启用近平滑可能会被视为与手“断开”。
        private bool smoothingNear = true;

        /// <summary>
        /// Whether to enable frame-rate independent smoothing for near interactions.
        /// 是否为近交互启用与帧速率无关的平滑。
        /// </summary>
        /// <remarks>
        /// Note that enabling near smoothing may be perceived as being 'disconnected' from the hand.
        /// 请注意，启用近平滑可能会被视为与手“断开”。
        /// </remarks>
        public bool SmoothingNear
        {
            get => smoothingNear;
            set => smoothingNear = value;
        }

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Enter amount representing amount of smoothing to apply to the movement. Smoothing of 0 means no smoothing. Max value means no change to value.")]
        // 输入表示要应用于移动的平滑量的量。平滑度为0表示没有平滑度。“最大值”表示值没有更改。
        private float moveLerpTime = 0.001f;

        /// <summary>
        /// Enter amount representing amount of smoothing to apply to the movement. Smoothing of 0 means no smoothing. Max value means no change to value.
        /// 输入表示要应用于移动的平滑量的量。平滑度为0表示没有平滑度。“最大值”表示值没有更改。
        /// </summary>
        public float MoveLerpTime
        {
            get => moveLerpTime;
            set => moveLerpTime = value;
        }

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Enter amount representing amount of smoothing to apply to the rotation. Smoothing of 0 means no smoothing. Max value means no change to value.")]
        //输入表示要应用于旋转的平滑量的量。平滑度为0表示没有平滑度。“最大值”表示值没有更改。
        private float rotateLerpTime = 0.001f;

        /// <summary>
        /// Enter amount representing amount of smoothing to apply to the rotation. Smoothing of 0 means no smoothing. Max value means no change to value.
        /// 输入表示要应用于旋转的平滑量的量。平滑度为0表示没有平滑度。“最大值”表示值没有更改。
        /// </summary>
        public float RotateLerpTime
        {
            get => rotateLerpTime;
            set => rotateLerpTime = value;
        }

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Enter amount representing amount of smoothing to apply to the scale. Smoothing of 0 means no smoothing. Max value means no change to value.")]
        //输入表示要应用于比例的平滑量的量。平滑度为0表示没有平滑度。“最大值”表示值没有更改。
        private float scaleLerpTime = 0.001f;

        /// <summary>
        /// Enter amount representing amount of smoothing to apply to the scale. Smoothing of 0 means no smoothing. Max value means no change to value.
        /// 输入表示要应用于比例的平滑量的量。平滑度为0表示没有平滑度。“最大值”表示值没有更改。
        /// </summary>
        public float ScaleLerpTime
        {
            get => scaleLerpTime;
            set => scaleLerpTime = value;
        }

        [SerializeField]
        [Tooltip("Enable or disable constraint support of this component. When enabled transform " +
            "changes will be post processed by the linked constraint manager.")]
        private bool enableConstraints = true;
        /// <summary>
        /// Enable or disable constraint support of this component. When enabled, transform
        /// changes will be post processed by the linked constraint manager.
        /// 启用或禁用此组件的约束支持。启用后，变换更改将由链接约束管理器进行后处理。
        /// </summary>
        public bool EnableConstraints
        {
            get => enableConstraints;
            set => enableConstraints = value;
        }

        [SerializeField]
        [Tooltip("Constraint manager slot to enable constraints when manipulating the object.")]
        private ConstraintManager constraintsManager;
        /// <summary>
        /// Constraint manager slot to enable constraints when manipulating the object.
        /// 约束管理器插槽，用于在操作对象时启用约束。
        /// </summary>
        public ConstraintManager ConstraintsManager
        {
            get => constraintsManager;
            set => constraintsManager = value;
        }

        [SerializeField]
        [Tooltip("Elastics Manager slot to enable elastics simulation when manipulating the object.")]
        //“弹性管理器”插槽，用于在操纵对象时启用弹性模拟
        private ElasticsManager elasticsManager;
        /// <summary>
        /// Elastics Manager slot to enable elastics simulation when manipulating the object.
        /// “弹性管理器”插槽，用于在操纵对象时启用弹性模拟。
        /// </summary>
        public ElasticsManager ElasticsManager
        {
            get => elasticsManager;
            set => elasticsManager = value;
        }

        #endregion Serialized Fields

        #region Event handlers
        [Header("Manipulation Events")]
        [SerializeField]
        [FormerlySerializedAs("OnManipulationStarted")]
        private ManipulationEvent onManipulationStarted = new ManipulationEvent();

        /// <summary>
        /// Unity event raised on manipulation started
        /// 操作启动时引发的Unity事件
        /// </summary>
        public ManipulationEvent OnManipulationStarted
        {
            get => onManipulationStarted;
            set => onManipulationStarted = value;
        }

        [SerializeField]
        [FormerlySerializedAs("OnManipulationEnded")]
        private ManipulationEvent onManipulationEnded = new ManipulationEvent();

        /// <summary>
        /// Unity event raised on manipulation ended
        /// 操作结束时引发的Unity事件
        /// </summary>
        public ManipulationEvent OnManipulationEnded
        {
            get => onManipulationEnded;
            set => onManipulationEnded = value;
        }

        [SerializeField]
        [FormerlySerializedAs("OnHoverEntered")]
        private ManipulationEvent onHoverEntered = new ManipulationEvent();

        /// <summary>
        /// Unity event raised on hover started
        /// 当手/控制器将可操纵的鼠标悬停在附近或远处时触发。
        /// </summary>
        public ManipulationEvent OnHoverEntered
        {
            get => onHoverEntered;
            set => onHoverEntered = value;
        }

        [SerializeField]
        [FormerlySerializedAs("OnHoverExited")]
        private ManipulationEvent onHoverExited = new ManipulationEvent();

        

        /// <summary>
        /// Unity event raised on hover ended
        /// 当手/控制器将可操作的，接近或远离的鼠标悬停时触发。
        /// </summary>
        public ManipulationEvent OnHoverExited
        {
            get => onHoverExited;
            set => onHoverExited = value;
        }

        #endregion Event Handlers

        #region Private Properties

        private ManipulationMoveLogic moveLogic;
        private TwoHandScaleLogic scaleLogic;
        private TwoHandRotateLogic rotateLogic;

        /// <summary>
        /// Holds the pointer and the initial intersection point of the pointer ray
        /// with the object on pointer down in pointer space
        /// 在指针空间中保持指针和指针光线与指针上的对象的初始交点
        /// </summary>
        private struct PointerData
        {
            public IMixedRealityPointer pointer;
            private Vector3 initialGrabPointInPointer;

            public PointerData(IMixedRealityPointer pointer, Vector3 worldGrabPoint) : this()
            {
                this.pointer = pointer;
                this.initialGrabPointInPointer = Quaternion.Inverse(pointer.Rotation) * (worldGrabPoint - pointer.Position);
            }

            public bool IsNearPointer => pointer is IMixedRealityNearPointer;

            /// Returns the grab point on the manipulated object in world space
            /// 返回世界空间中操纵对象上的抓取点
            public Vector3 GrabPoint => (pointer.Rotation * initialGrabPointInPointer) + pointer.Position;
        }

        private Dictionary<uint, PointerData> pointerIdToPointerMap = new Dictionary<uint, PointerData>();
        private Quaternion objectToGripRotation;
        private bool isNearManipulation;
        private bool isManipulationStarted;
        private bool isSmoothing;

        private Rigidbody rigidBody;
        private bool wasGravity = false;
        private bool wasKinematic = false;

        private bool IsOneHandedManipulationEnabled => manipulationType.HasFlag(ManipulationHandFlags.OneHanded) && pointerIdToPointerMap.Count == 1;
        private bool IsTwoHandedManipulationEnabled => manipulationType.HasFlag(ManipulationHandFlags.TwoHanded) && pointerIdToPointerMap.Count > 1;

        private Quaternion leftHandRotation;
        private Quaternion rightHandRotation;

        #endregion Private Properties

        #region MyTransformTool Variable




        public bool IsGrabed { get { return isManipulationStarted; } }
        public bool IsHovered { get; private set; } = false;
        private int HoveredCount = 0;
        private bool _ignoreGrasping = false;
        public bool ignoreGrasping
        {
            get { return _ignoreGrasping; }
            set
            {
                _ignoreGrasping = value;

                if (_ignoreGrasping && IsGrabed)
                {
                    ForceEndManipulation();
                }
            }
        }
        public Rigidbody rigidbody
        {
            get { return rigidBody; }
            protected set { rigidBody = value; }
        }

        public Action OnGraspBegin;

        public Action OnGraspEnd;

        #endregion

        #region MyTransformTool Methods


        public void SetIsHovered()
        {
            HoveredCount++;
            if (HoveredCount > 0)
            {
                IsHovered = true;
            }
            else if (HoveredCount < 0)
            {
                Debug.LogError(HoveredCount);
            }
        }
        public void SetIsNotHovered()
        {
            HoveredCount--;
            if (HoveredCount < 1)
            {
                IsHovered = false;
            }
            else if (HoveredCount < 0)
            {
                Debug.LogError(HoveredCount);
            }
        }


        public delegate void MyManipulationEvent(Vector3 oldPosition, Quaternion oldRotation,
                                            Vector3 newPosition, Quaternion newRotation
                                            );

        public MyManipulationEvent OnGraspedManipulation = (preSolvedPos, preSolvedRot,
                                                     solvedPos, solvedRot
                                                     ) => { };

        private void test()
        {
            Debug.Log("!!!");
        }

        #endregion

        #region MonoBehaviour Functions

        private void Awake()
        {
            moveLogic = new ManipulationMoveLogic();
            rotateLogic = new TwoHandRotateLogic();
            scaleLogic = new TwoHandScaleLogic();            

            if (elasticsManager)//“弹性管理器”插槽，用于在操纵对象时启用弹性模拟
            {
                elasticsManager.InitializeElastics(HostTransform);
            }
        }
        private void Start()
        {
            rigidBody = HostTransform.GetComponent<Rigidbody>();
            if (constraintsManager == null && EnableConstraints)
            {
                constraintsManager = gameObject.EnsureComponent<ConstraintManager>();
            }

            // Get child objects with NearInteractionGrabbable attached
            // 获取附加了NearInteractionGrabbable的子对象
            var children = GetComponentsInChildren<NearInteractionGrabbable>();

            if (children.Length == 0)
            {
                Debug.Log($"Near interactions are not enabled for {gameObject.name}. To enable near interactions, add a " +
                    $"{nameof(NearInteractionGrabbable)} component to {gameObject.name} or to a child object of {gameObject.name} that contains a collider.");
                //{gameObject.name}未启用$近距离交互。若要启用近距离交互，请添加+$“{ nameof（NearInteractionGrabbable）}组件到{ gameObject.name}或到包含碰撞器的{ gameObject.name}的子对象。”
            }
                        
        }

        void Update()
        {

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates the unweighted average, or centroid, of all pointers'
        /// grab points, as defined by the PointerData.GrabPoint property.
        /// 
        /// 计算指针数据定义的所有指针抓取点的未加权平均值或质心。
        /// 
        /// Does not use the rotation of each pointer; represents a pure
        /// geometric centroid  of the grab points in world space.
        /// 
        /// GrabPoint属性。不使用每个指针的旋转；代表一种纯粹的世界空间中抓取点的几何质心。
        /// 
        /// </summary>
        /// <returns>
        /// Worldspace grab point centroid of all pointers
        /// in pointerIdToPointerMap.
        /// 
        /// 世界空间获取pointerIdToPointerMap中所有指针的点形心。
        /// 
        /// </returns>
        private Vector3 GetPointersGrabPoint()
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (var p in pointerIdToPointerMap.Values)
            {
                sum += p.GrabPoint;
                count++;
            }
            return sum / Math.Max(1, count);
        }

        /// <summary>
        /// Calculates the multiple-handed pointer pose, used for
        /// far-interaction hand-ray-based manipulations. Uses the
        /// unweighted vector average of the pointers' forward vectors
        /// to calculate a compound pose that takes into account the
        /// pointing direction of each pointer.
        /// 
        /// 计算多手指针姿势，用于基于手射线的远交互操作。
        /// 使用指针前向向量的未加权向量平均值来计算复合姿势，该姿势考虑了每个指针的指向方向。
        /// 
        /// </summary>
        /// <returns>
        /// Compound pose calculated as the average of the poses
        /// corresponding to all of the pointers in pointerIdToPointerMap.
        /// 
        /// 复合姿势计算为与pointerIdToPointerMap中所有指针对应的姿势的平均值。
        /// 
        /// </returns>
        private MixedRealityPose GetPointersPose()
        {
            Vector3 sumPos = Vector3.zero;
            Vector3 sumDir = Vector3.zero;
            int count = 0;
            foreach (var p in pointerIdToPointerMap.Values)
            {
                sumPos += p.pointer.Position;
                sumDir += p.pointer.Rotation * Vector3.forward;
                count++;
            }

            return new MixedRealityPose
            {
                Position = sumPos / Math.Max(1, count),
                Rotation = Quaternion.LookRotation(sumDir / Math.Max(1, count))
            };
        }

        private Vector3 GetPointersVelocity()
        {
            Vector3 sum = Vector3.zero;
            int numControllers = 0;
            foreach (var p in pointerIdToPointerMap.Values)
            {
                // Check pointer has a valid controller (e.g. gaze pointer doesn't)
                // 检查指针是否有有效的控制器（例如，凝视指针没有）
                if (p.pointer.Controller != null)
                {
                    numControllers++;
                    sum += p.pointer.Controller.Velocity;
                }
            }
            return sum / Math.Max(1, numControllers);
        }

        private Vector3 GetPointersAngularVelocity()
        {
            Vector3 sum = Vector3.zero;
            int numControllers = 0;
            foreach (var p in pointerIdToPointerMap.Values)
            {
                // Check pointer has a valid controller (e.g. gaze pointer doesn't)
                // 检查指针是否有有效的控制器（例如，凝视指针没有）
                if (p.pointer.Controller != null)
                {
                    numControllers++;
                    sum += p.pointer.Controller.AngularVelocity;
                }
            }
            return sum / Math.Max(1, numControllers);
        }

        private bool IsNearManipulation()
        {
            foreach (var item in pointerIdToPointerMap)
            {
                if (item.Value.IsNearPointer)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion Private Methods

        #region Public Methods

        /// <summary>
        /// Releases the object that is currently manipulated
        /// 释放当前操纵的对象
        /// </summary>
        public void ForceEndManipulation()
        {
            // end manipulation
            // 结束操纵
            if (isManipulationStarted)
            {
                HandleManipulationEnded(GetPointersGrabPoint(), GetPointersVelocity(), GetPointersAngularVelocity());
            }
            pointerIdToPointerMap.Clear();
        }

        /// <summary>
        /// Gets the grab point for the given pointer id.
        /// Only use if you know that your given pointer id corresponds to a pointer that has grabbed
        /// this component.
        /// 
        /// 获取给定指针id的抓取点。仅当您知道给定指针id对应于已抓取此组件的指针时才使用。
        /// 
        /// </summary>
        public Vector3 GetPointerGrabPoint(uint pointerId)
        {
            Assert.IsTrue(pointerIdToPointerMap.ContainsKey(pointerId));
            return pointerIdToPointerMap[pointerId].GrabPoint;
        }

        #endregion Public Methods

        #region Hand Event Handlers

        /// <inheritdoc />
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (eventData.used ||
                    (!allowFarManipulation && eventData.Pointer as IMixedRealityNearPointer == null))
            {
                return;
            }

            // If we only allow one handed manipulations, check there is no hand interacting yet.
            // 如果我们只允许单手操作，请检查是否还没有手交互。
            if (manipulationType != ManipulationHandFlags.OneHanded || pointerIdToPointerMap.Count == 0)
            {
                uint id = eventData.Pointer.PointerId;
                // Ignore poke pointer events
                // 忽略戳指针事件
                if (!pointerIdToPointerMap.ContainsKey(id))
                {
                    // cache start ptr grab point
                    // 缓存启动ptr抓取点
                    pointerIdToPointerMap.Add(id, new PointerData(eventData.Pointer, eventData.Pointer.Result.Details.Point));

                    // Re-initialize elastic systems.
                    // 重新初始化弹性系统。
                    if (elasticsManager)
                    {
                        elasticsManager.InitializeElastics(HostTransform);
                    }

                    // Call manipulation started handlers
                    // 调用处理程序已启动
                    if (IsTwoHandedManipulationEnabled)
                    {
                        if (!isManipulationStarted)
                        {
                            HandleManipulationStarted();
                        }
                        HandleTwoHandManipulationStarted();
                    }
                    else if (IsOneHandedManipulationEnabled)
                    {
                        if (!isManipulationStarted)
                        {
                            HandleManipulationStarted();
                        }
                        HandleOneHandMoveStarted();
                    }
                }
            }

            if (pointerIdToPointerMap.Count > 0)
            {
                // Always mark the pointer data as used to prevent any other behavior to handle pointer events
                // as long as the ObjectManipulator is active.
                // 只要ObjectManipulator处于活动状态，始终将指针数据标记为用于防止处理指针事件的任何其他行为。
                // This is due to us reacting to both "Select" and "Grip" events.
                // 这是由于我们对“选择”和“抓取”事件都做出反应。
                eventData.Use();
            }
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            // Call manipulation updated handlers
            // 调用操作更新处理程序
            if (IsOneHandedManipulationEnabled)
            {
                HandleOneHandMoveUpdated();
            }
            else if (IsTwoHandedManipulationEnabled)
            {
                HandleTwoHandManipulationUpdated();
            }
        }

        /// <inheritdoc />
        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            // Get pointer data before they are removed from the map
            // 在从映射中删除指针数据之前获取指针数据
            Vector3 grabPoint = GetPointersGrabPoint();
            Vector3 velocity = GetPointersVelocity();
            Vector3 angularVelocity = GetPointersAngularVelocity();

            uint id = eventData.Pointer.PointerId;
            if (pointerIdToPointerMap.ContainsKey(id))
            {
                pointerIdToPointerMap.Remove(id);
            }

            // Call manipulation ended handlers
            // 调用操作结束处理程序
            var handsPressedCount = pointerIdToPointerMap.Count;
            if (manipulationType.HasFlag(ManipulationHandFlags.TwoHanded) && handsPressedCount == 1)
            {
                if (manipulationType.HasFlag(ManipulationHandFlags.OneHanded))
                {
                    HandleOneHandMoveStarted();
                }
                else
                {
                    HandleManipulationEnded(grabPoint, velocity, angularVelocity);
                }
            }
            else if (isManipulationStarted && handsPressedCount == 0)
            {
                HandleManipulationEnded(grabPoint, velocity, angularVelocity);
            }

            eventData.Use();
        }

        #endregion Hand Event Handlers

        #region Private Event Handlers
        private void HandleTwoHandManipulationStarted()//双手操作开始
        {
            var handPositionArray = GetHandPositionArray();

            if (twoHandedManipulationType.HasFlag(TransformFlags.Rotate))
            {
                rotateLogic.Setup(handPositionArray, HostTransform);
            }
            if (twoHandedManipulationType.HasFlag(TransformFlags.Move))
            {
                // If near manipulation, a pure grabpoint centroid is used for
                // the initial pointer pose; if far manipulation, a more complex
                // look-rotation-based pointer pose is used.
                // 如果接近操纵，则使用纯抓取点质心作为初始指针姿势；
                // 如果使用“远”操纵，则使用更复杂的基于旋转的指针姿势。
                MixedRealityPose pointerPose = IsNearManipulation() ? new MixedRealityPose(GetPointersGrabPoint()) : GetPointersPose();
                MixedRealityPose hostPose = new MixedRealityPose(HostTransform.position, HostTransform.rotation);
                moveLogic.Setup(pointerPose, GetPointersGrabPoint(), hostPose, HostTransform.localScale);
            }
            if (twoHandedManipulationType.HasFlag(TransformFlags.Scale))
            {
                scaleLogic.Setup(handPositionArray, HostTransform);
            }
        }

        private void HandleTwoHandManipulationUpdated()//双手操作更新
        {
            Vector3 preSolvedPos = rigidBody.position;
            Quaternion preSolvedRot = rigidBody.rotation;
            var targetTransform = new MixedRealityTransform(HostTransform.position, HostTransform.rotation, HostTransform.localScale);

            var handPositionArray = GetHandPositionArray();

            if (twoHandedManipulationType.HasFlag(TransformFlags.Scale))
            {
                targetTransform.Scale = scaleLogic.UpdateMap(handPositionArray);
                if (EnableConstraints && constraintsManager != null)
                {
                    constraintsManager.ApplyScaleConstraints(ref targetTransform, false, IsNearManipulation());
                }
            }
            if (twoHandedManipulationType.HasFlag(TransformFlags.Rotate))
            {
                targetTransform.Rotation = rotateLogic.Update(handPositionArray, targetTransform.Rotation);
                if (EnableConstraints && constraintsManager != null)
                {
                    constraintsManager.ApplyRotationConstraints(ref targetTransform, false, IsNearManipulation());
                }
            }
            if (twoHandedManipulationType.HasFlag(TransformFlags.Move))
            {
                // If near manipulation, a pure GrabPoint centroid is used for
                // the pointer pose; if far manipulation, a more complex
                // look-rotation-based pointer pose is used.
                // 如果接近操纵，则指针姿势使用纯抓取点质心；
                // 如果使用“远”操纵，则使用更复杂的基于旋转的指针姿势。
                MixedRealityPose pose = IsNearManipulation() ? new MixedRealityPose(GetPointersGrabPoint()) : GetPointersPose();
                targetTransform.Position = moveLogic.Update(pose, targetTransform.Rotation, targetTransform.Scale, true);
                if (EnableConstraints && constraintsManager != null)
                {
                    constraintsManager.ApplyTranslationConstraints(ref targetTransform, false, IsNearManipulation());
                }
            }

            

            if(!ignoreGrasping)
            {
                ApplyTargetTransform(targetTransform);
            }

            Vector3 solvedPos = rigidBody.position;
            Quaternion solvedRot = rigidBody.rotation;

            
            if (OnGraspedManipulation != null)
            {
                OnGraspedManipulation.Invoke(preSolvedPos, preSolvedRot, solvedPos, solvedRot);                
            }
        }

        private void HandleOneHandMoveStarted()//单手操作开始
        {
            Assert.IsTrue(pointerIdToPointerMap.Count == 1);
            PointerData pointerData = GetFirstPointer();
            IMixedRealityPointer pointer = pointerData.pointer;

            // Calculate relative transform from object to grip.
            // 计算从对象到抓取点的相对变换。
            Quaternion gripRotation;
            TryGetGripRotation(pointer, out gripRotation);
            Quaternion worldToGripRotation = Quaternion.Inverse(gripRotation);
            objectToGripRotation = worldToGripRotation * HostTransform.rotation;

            MixedRealityPose pointerPose = new MixedRealityPose(pointer.Position, pointer.Rotation);
            MixedRealityPose hostPose = new MixedRealityPose(HostTransform.position, HostTransform.rotation);
            moveLogic.Setup(pointerPose, pointerData.GrabPoint, hostPose, HostTransform.localScale);
        }

        private void HandleOneHandMoveUpdated()//单手操作更新
        {
            Debug.Assert(pointerIdToPointerMap.Count == 1);
            PointerData pointerData = GetFirstPointer();
            IMixedRealityPointer pointer = pointerData.pointer;

            Vector3 preSolvedPos;
            Quaternion preSolvedRot;

            if (rigidBody != null)
            {
                preSolvedPos = rigidBody.position;
                preSolvedRot = rigidBody.rotation;
            }
            else
            {
                preSolvedPos = HostTransform.transform.position;
                preSolvedRot = HostTransform.transform.rotation;
            }

            var targetTransform = new MixedRealityTransform(HostTransform.position, HostTransform.rotation, HostTransform.localScale);
            
            if (EnableConstraints && constraintsManager != null)
            {
                constraintsManager.ApplyScaleConstraints(ref targetTransform, true, IsNearManipulation());
            }

            Quaternion gripRotation;
            TryGetGripRotation(pointer, out gripRotation);
            targetTransform.Rotation = gripRotation * objectToGripRotation;

            if (EnableConstraints && constraintsManager != null)
            {
                constraintsManager.ApplyRotationConstraints(ref targetTransform, true, IsNearManipulation());
            }

            RotateInOneHandType rotateInOneHandType = isNearManipulation ? oneHandRotationModeNear : oneHandRotationModeFar;
            MixedRealityPose pointerPose = new MixedRealityPose(pointer.Position, pointer.Rotation);
            targetTransform.Position = moveLogic.Update(pointerPose, targetTransform.Rotation, targetTransform.Scale, rotateInOneHandType != RotateInOneHandType.RotateAboutObjectCenter);

            if (EnableConstraints && constraintsManager != null)
            {
                constraintsManager.ApplyTranslationConstraints(ref targetTransform, true, IsNearManipulation());
            }


            
            if (!ignoreGrasping)
            {
                ApplyTargetTransform(targetTransform);
            }

            Vector3 solvedPos;
            Quaternion solvedRot;

            if (rigidBody != null)
            {
                solvedPos = rigidBody.position;
                solvedRot = rigidBody.rotation;
            }
            else
            {
                solvedPos = HostTransform.transform.position;
                solvedRot = HostTransform.transform.rotation;
            }

            if (OnGraspedManipulation != null)
            {
                OnGraspedManipulation.Invoke(preSolvedPos, preSolvedRot, solvedPos, solvedRot);                
            }            
        }

        private void HandleManipulationStarted()//手柄操作开始
        {
            if (OnGraspBegin != null)
            {
                OnGraspBegin();
            }

            isManipulationStarted = true;
            isNearManipulation = IsNearManipulation();
            isSmoothing = (isNearManipulation ? smoothingNear : smoothingFar);

            
            

            // TODO: If we are on HoloLens 1, push and pop modal input handler so that we can use old
            // gaze/gesture/voice manipulation. For HoloLens 2, we don't want to do this.
            // TODO：如果我们在HoloLens 1上，按下并弹出模式输入处理程序，这样我们就可以使用旧的凝视/手势/语音操作。对于全息透镜2，我们不想这样做。
            if (OnManipulationStarted != null)
            {
                OnManipulationStarted.Invoke(new ManipulationEventData
                {
                    ManipulationSource = gameObject,
                    IsNearInteraction = isNearManipulation,
                    Pointer = GetFirstPointer().pointer,
                    PointerCentroid = GetPointersGrabPoint(),
                    PointerVelocity = GetPointersVelocity(),
                    PointerAngularVelocity = GetPointersAngularVelocity()
                });
            }

            if (rigidBody != null)
            {
                wasGravity = rigidBody.useGravity;
                wasKinematic = rigidBody.isKinematic;
                rigidBody.useGravity = false;
                rigidBody.isKinematic = false;
            }

            if (EnableConstraints && constraintsManager != null)
            {
                constraintsManager.Initialize(new MixedRealityTransform(HostTransform));
            }
            if (elasticsManager != null)
            {
                elasticsManager.EnableElasticsUpdate = false;
            }
        }

        private void HandleManipulationEnded(Vector3 pointerGrabPoint, Vector3 pointerVelocity, Vector3 pointerAnglularVelocity)//手柄操作结束
        {
            isManipulationStarted = false;

            

            // TODO: If we are on HoloLens 1, push and pop modal input handler so that we can use old
            // gaze/gesture/voice manipulation. For HoloLens 2, we don't want to do this.
            // TODO：如果我们在HoloLens 1上，按下并弹出模式输入处理程序，这样我们就可以使用旧的凝视/手势/语音操作。对于全息透镜2，我们不想这样做。
            if (OnManipulationEnded != null)
            {
                OnManipulationEnded.Invoke(new ManipulationEventData
                {
                    ManipulationSource = gameObject,
                    IsNearInteraction = isNearManipulation,
                    PointerCentroid = pointerGrabPoint,
                    PointerVelocity = pointerVelocity,
                    PointerAngularVelocity = pointerAnglularVelocity
                });
            }

            ReleaseRigidBody(pointerVelocity, pointerAnglularVelocity);
            if (elasticsManager != null)
            {
                elasticsManager.EnableElasticsUpdate = true;
            }

            if (OnGraspEnd != null)
            {
                OnGraspEnd();
            }
        }

        #endregion Private Event Handlers

        #region Unused Event Handlers
        /// <inheritdoc />
        public void OnPointerClicked(MixedRealityPointerEventData eventData) { }
        public void OnBeforeFocusChange(FocusEventData eventData) { }

        #endregion Unused Event Handlers

        #region Private methods

        private void ApplyTargetTransform(MixedRealityTransform targetTransform)
        {
            if (rigidBody == null)
            {
                TransformFlags transformUpdated = 0;
                if (elasticsManager != null)
                {
                    transformUpdated = elasticsManager.ApplyTargetTransform(targetTransform);
                }

                if (!transformUpdated.HasFlag(TransformFlags.Move))
                {
                    HostTransform.position = isSmoothing ? Smoothing.SmoothTo(HostTransform.position, targetTransform.Position, moveLerpTime, Time.deltaTime) : targetTransform.Position;
                }
                if (!transformUpdated.HasFlag(TransformFlags.Rotate))
                {
                    HostTransform.rotation = isSmoothing ? Smoothing.SmoothTo(HostTransform.rotation, targetTransform.Rotation, rotateLerpTime, Time.deltaTime) : targetTransform.Rotation;
                }
                if (!transformUpdated.HasFlag(TransformFlags.Scale))
                {
                    HostTransform.localScale = isSmoothing ? Smoothing.SmoothTo(HostTransform.localScale, targetTransform.Scale, scaleLerpTime, Time.deltaTime) : targetTransform.Scale;
                }
            }
            else
            {
                // There is a RigidBody. Potential different paths for near vs far manipulation
                // 有一个刚体。近操纵与远操纵的潜在不同路径
                if (isNearManipulation && !useForcesForNearManipulation)
                {
                    // This is a near manipulation and we're not using forces
                    // 这是一个近乎操纵的动作，我们没有使用力
                    // Apply direct updates but account for smoothing
                    // 应用直接更新，但考虑平滑
                    if (isSmoothing)
                    {
                        rigidBody.MovePosition(Smoothing.SmoothTo(rigidBody.position, targetTransform.Position, moveLerpTime, Time.deltaTime));
                        rigidBody.MoveRotation(Smoothing.SmoothTo(rigidBody.rotation, targetTransform.Rotation, rotateLerpTime, Time.deltaTime));
                    }
                    else
                    {
                        rigidBody.MovePosition(targetTransform.Position);
                        rigidBody.MoveRotation(targetTransform.Rotation);
                    }
                }
                else
                {
                    // We are using forces
                    // 我们使用力

                    rigidBody.velocity = ((1f - Mathf.Pow(moveLerpTime, Time.deltaTime)) / Time.deltaTime) * (targetTransform.Position - HostTransform.position);

                    var relativeRotation = targetTransform.Rotation * Quaternion.Inverse(HostTransform.rotation);
                    relativeRotation.ToAngleAxis(out float angle, out Vector3 axis);

                    if (axis.IsValidVector())
                    {
                        if (angle > 180f)
                        {
                            angle -= 360f;
                        }
                        rigidBody.angularVelocity = ((1f - Mathf.Pow(rotateLerpTime, Time.deltaTime)) / Time.deltaTime) * (axis.normalized * angle * Mathf.Deg2Rad);
                    }
                }

                HostTransform.localScale = isSmoothing ? Smoothing.SmoothTo(HostTransform.localScale, targetTransform.Scale, scaleLerpTime, Time.deltaTime) : targetTransform.Scale;
            }
        }

        private Vector3[] GetHandPositionArray()
        {
            var handPositionMap = new Vector3[pointerIdToPointerMap.Count];
            int index = 0;
            foreach (var item in pointerIdToPointerMap)
            {
                handPositionMap[index++] = item.Value.pointer.Position;
            }
            return handPositionMap;
        }

        public void OnFocusChanged(FocusEventData eventData)
        {
            bool isFar = !(eventData.Pointer is IMixedRealityNearPointer);
            if (isFar && !AllowFarManipulation)
            {
                return;
            }

            if (eventData.OldFocusedObject == null ||
                    !eventData.OldFocusedObject.transform.IsChildOf(transform))
            {
                SetIsHovered();
                if (OnHoverEntered != null)
                {
                    OnHoverEntered.Invoke(new ManipulationEventData
                    {
                        ManipulationSource = gameObject,
                        Pointer = eventData.Pointer,
                        IsNearInteraction = !isFar
                    });
                }
            }
            else if (eventData.NewFocusedObject == null ||
                     !eventData.NewFocusedObject.transform.IsChildOf(transform))
            {
                SetIsNotHovered();
                if (OnHoverExited != null)
                {
                    OnHoverExited.Invoke(new ManipulationEventData
                    {
                        ManipulationSource = gameObject,
                        Pointer = eventData.Pointer,
                        IsNearInteraction = !isFar
                    });
                }
            }
        }

        private void ReleaseRigidBody(Vector3 velocity, Vector3 angularVelocity)
        {
            if (rigidBody != null)
            {
                rigidBody.useGravity = wasGravity;
                rigidBody.isKinematic = wasKinematic;

                if (releaseBehavior.HasFlag(ReleaseBehaviorType.KeepVelocity))
                {
                    rigidBody.velocity = velocity;
                }

                if (releaseBehavior.HasFlag(ReleaseBehaviorType.KeepAngularVelocity))
                {
                    rigidBody.angularVelocity = angularVelocity;
                }
            }
        }

        private PointerData GetFirstPointer()
        {
            // We may be able to do this without allocating memory.
            // Moving to a method for later investigation.
            // 我们可能不需要分配内存就可以做到这一点。
            // 移动到一种方法，以便以后进行调查。

            return pointerIdToPointerMap.Values.First();
        }

        private bool TryGetGripRotation(IMixedRealityPointer pointer, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            switch (pointer.Controller?.ControllerHandedness)
            {
                case Handedness.Left:
                    rotation = leftHandRotation;
                    break;
                case Handedness.Right:
                    rotation = rightHandRotation;
                    break;
                default:
                    return false;
            }
            return true;
        }

        #endregion

        #region Source Pose Handler Implementation
        /// <summary>
        /// Raised when the source pose tracking state is changed.
        /// 源姿势跟踪状态更改时引发。
        /// </summary>
        public void OnSourcePoseChanged(SourcePoseEventData<TrackingState> eventData) { }

        /// <summary>
        /// Raised when the source position is changed.
        /// 源位置更改时引发。
        /// </summary>
        public void OnSourcePoseChanged(SourcePoseEventData<Vector2> eventData) { }

        /// <summary>
        /// Raised when the source position is changed.
        /// 源位置更改时引发。
        /// </summary>
        public void OnSourcePoseChanged(SourcePoseEventData<Vector3> eventData) { }

        /// <summary>
        /// Raised when the source rotation is changed.
        /// 更改源旋转时引发。
        /// </summary>
        public void OnSourcePoseChanged(SourcePoseEventData<Quaternion> eventData) { }

        /// <summary>
        /// Raised when the source pose is changed.
        /// 更改源姿势时引发。
        /// </summary>
        public void OnSourcePoseChanged(SourcePoseEventData<MixedRealityPose> eventData)
        {
            switch (eventData.Controller?.ControllerHandedness)
            {
                case Handedness.Left:
                    leftHandRotation = eventData.SourceData.Rotation;
                    break;
                case Handedness.Right:
                    rightHandRotation = eventData.SourceData.Rotation;
                    break;
                default:
                    break;
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData) { }

        public void OnSourceLost(SourceStateEventData eventData) { }

        #endregion
    }


}