using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MyTransformTool
{
    public enum Condition { DWBM, NFWBM }//工具的三种状态，闲置、平移、旋转
    public enum ManipulationMode {  Direct, Widget, Set }//工具的三种状态，闲置、平移、旋转
    public class MyTransformToolUI : MonoBehaviour
    {
        public List<MyTransformTool> TransformToolList = new List<MyTransformTool>();
        public HashSet<MyTransformStore> TransformStoreSet = new HashSet<MyTransformStore>();
        public GameObject DirectTarget;
        private HashSet<GameObject> TranslateHandles = new HashSet<GameObject>();
        private HashSet<GameObject> RotateHandles = new HashSet<GameObject>();
        public Dropdown ModeDropdown;
        public Dropdown TypeDropdown;        
        public Dropdown SpaceDropdown;
        public Slider MoveSlider;
        public Slider RotateSlider;
        public Toggle ReplicToggle;
        public Vector3 ReplicPosition = new Vector3(0.0f, 0.3f, 0.0f);
        private int InteractionWithTargetNum = 0;
        private int DirectInteractionWithTargetNum = 0;
        private int InteractionWithUINum = 0;
        private int InteractionWithMoveSliderUINum = 0;
        private int InteractionWithRotateSliderUINum = 0;
        private int InteractionWithUnReDoUINum = 0;
        private int InteractionWithShowUINum = 0;
        private int InteractionWithModeUINum = 0;
        private int InteractionWithTypeUINum = 0;
        private int InteractionWithSpaceUINum = 0;
        public Transform WorldCoordinateSystem;
        private bool OnMoveSlider = false;
        private bool OnRotateSlider = false;
        public GameObject RealTarget;


        public ManipulationMode Mode = ManipulationMode.Direct;
        //Start is called before the first frame update
        [SerializeField]
        public Text TranslationMultiplierLabel;
        [SerializeField]
        public Text RotationMultiplierLabel;
        public void Awake()
        {

            

        }

        public void Start()
        {
            if(WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
        }

        public void OnModeChanged(int index)
        {
            if (DirectTarget.GetComponent<MyReplicaManipulation>() != null)
            {
                DirectTarget.GetComponent<MyReplicaManipulation>().Init();
            }
            switch (index)
            {
                case 0:
                    Mode = ManipulationMode.Direct;
                    EnableDirectManipulation();
                    EnableReplicaManipulation();
                    HideTransformTool();
                    if (ReplicToggle != null)
                    {
                        ReplicToggle.isOn = true;
                        ShowReplic(ReplicToggle.isOn);
                    }
                    UpdateMode();
                    break;
                case 1:
                    Mode = ManipulationMode.Widget;
                    DisableDirectManipulation();
                    DisableReplicaManipulation();
                    ShowTransformTool();                    
                    UpdateMode();
                    break;
                case 2:
                    Mode = ManipulationMode.Set;
                    EnableDirectManipulation();
                    DisableReplicaManipulation();
                    HideTransformTool();
                    if (ReplicToggle != null)
                    {
                        ReplicToggle.isOn = true;
                        ShowReplic(ReplicToggle.isOn);
                    }
                    UpdateMode();
                    break;
                case 3:
                    StopManipulation();
                    DisableDirectManipulation();
                    DisableReplicaManipulation();
                    HideTransformTool();
                    break;
            }
        }

        public void OnTypeChanged(int index)
        {
            switch (index)
            {
                case 0:
                    if(Mode == ManipulationMode.Direct)
                    {
                        EnableDirectMove();
                    }
                    else if(Mode == ManipulationMode.Widget)
                    {
                        ShowHandles(TranslateHandles);
                        HideHandles(RotateHandles);
                    }
                    else if(Mode == ManipulationMode.Set)
                    {
                        EnableDirectMove();
                    }
                    
                    break;
                case 1:
                    if (Mode == ManipulationMode.Direct)
                    {
                        EnableDirectRotate();                        
                    }
                    else if (Mode == ManipulationMode.Widget)
                    {
                        HideHandles(TranslateHandles);
                        ShowHandles(RotateHandles);
                    }
                    else if (Mode == ManipulationMode.Set)
                    {
                        EnableDirectMove();
                    }
                    
                    break;
                case 2:
                    if (Mode == ManipulationMode.Direct)
                    {
                        EnableDirectMoveandRotate();
                    }
                    else if (Mode == ManipulationMode.Widget)
                    {
                        ShowHandles(TranslateHandles);
                        ShowHandles(RotateHandles);
                    }
                    else if (Mode == ManipulationMode.Set)
                    {
                        EnableDirectMove();
                    }
                    
                    break;
            }
        }

        public void OnSpaceChanged(int index)
        {
            switch (index)
            {
                case 0:
                    LocalOrGlobal(false);
                    break;
                case 1:
                    LocalOrGlobal(true);
                    break;
            }
        }

        public void UnDo()
        {
            foreach (MyTransformStore Store in TransformStoreSet)
            {
                Store.UnDo();
            }
            UpdateReplicPosition();
        }

        public void ReDo()
        {
            foreach (MyTransformStore Store in TransformStoreSet)
            {
                Store.ReDo();
            }
            UpdateReplicPosition();
        }

        public void InitStore()
        {
            foreach (MyTransformStore Store in TransformStoreSet)
            {
                Store.InitStore();
            }
        }

        private void HideHandles(HashSet<GameObject> HandlesSet)
        {
            foreach (GameObject Handle in HandlesSet)
            {
                Handle.SetActive(false);
            }
        }

        private void ShowHandles(HashSet<GameObject> HandlesSet)
        {
            foreach (GameObject Handle in HandlesSet)
            {
                Handle.SetActive(true);
            }
        }

        public void HideTransformTool()
        {
            foreach (MyTransformTool TransformTool in TransformToolList)
            {
                TransformTool.gameObject.SetActive(false);
            }
        }

        public void EnableDirectManipulation()
        {
            if(DirectTarget.GetComponent<ObjectManipulator>() != null)
            {
                DirectTarget.GetComponent<ObjectManipulator>().enabled = true;
            }
            if (DirectTarget.GetComponent<MoveAxisConstraint>() != null)
            {
                DirectTarget.GetComponent<MoveAxisConstraint>().enabled = true;
            }
            if (DirectTarget.GetComponent<RotationAxisConstraint>() != null)
            {
                DirectTarget.GetComponent<RotationAxisConstraint>().enabled = true;
            }
            
        }

        public void DisableDirectManipulation()
        {
            if (DirectTarget.GetComponent<ObjectManipulator>() != null)
            {
                DirectTarget.GetComponent<ObjectManipulator>().enabled = false;
            }
            if (DirectTarget.GetComponent<MoveAxisConstraint>() != null)
            {
                DirectTarget.GetComponent<MoveAxisConstraint>().enabled = false;
            }
            if (DirectTarget.GetComponent<RotationAxisConstraint>() != null)
            {
                DirectTarget.GetComponent<RotationAxisConstraint>().enabled = false;
            }            
        }

        public void EnableDirectMove()
        {
            DirectTarget.GetComponent<MoveAxisConstraint>().ConstraintOnMovement = (Microsoft.MixedReality.Toolkit.Utilities.AxisFlags)(0);
            DirectTarget.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = (Microsoft.MixedReality.Toolkit.Utilities.AxisFlags)(-1);
        }
        public void EnableDirectRotate()
        {
            DirectTarget.GetComponent<MoveAxisConstraint>().ConstraintOnMovement = (Microsoft.MixedReality.Toolkit.Utilities.AxisFlags)(-1);
            DirectTarget.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = (Microsoft.MixedReality.Toolkit.Utilities.AxisFlags)(0);
        }

        public void EnableDirectMoveandRotate()
        {
            DirectTarget.GetComponent<MoveAxisConstraint>().ConstraintOnMovement = (Microsoft.MixedReality.Toolkit.Utilities.AxisFlags)(0);
            DirectTarget.GetComponent<RotationAxisConstraint>().ConstraintOnRotation = (Microsoft.MixedReality.Toolkit.Utilities.AxisFlags)(0);
        }

        public void EnableReplicaManipulation()
        {
            if(DirectTarget.GetComponent<MyReplicaManipulation>() != null)
            {
                DirectTarget.GetComponent<MyReplicaManipulation>().enabled = true;
            }
        }
        public void DisableReplicaManipulation()
        {
            if (DirectTarget.GetComponent<MyReplicaManipulation>() != null)
            {
                DirectTarget.GetComponent<MyReplicaManipulation>().enabled = false;
            }
        }


        public void ShowTransformTool()
        {
            foreach (MyTransformTool TransformTool in TransformToolList)
            {
                TransformTool.gameObject.SetActive(true);
            }
        }

        private void LocalOrGlobal(bool value)
        {
            for (int i = 0; i < TransformToolList.Count; i++)
            {
                TransformToolList[i].LocalOrGlobal = value;
                TransformToolList[i].SetToolTransform();
            }
        }

        public void UpdateTransformStore()
        {
            for (int i = 0; i < TransformToolList.Count; i++)
            {
                if (TransformToolList[i].GetComponent<MyTransformStore>() != null)
                {
                    TransformStoreSet.Add(TransformToolList[i].GetComponent<MyTransformStore>());
                }

                for (int j = 0; j < TransformToolList[i].transform.childCount; j++)
                {
                    GameObject Handle = TransformToolList[i].transform.GetChild(j).gameObject;
                    if (Handle.name == "Translate Handles")
                    {
                        TranslateHandles.Add(Handle);
                    }
                    else if (Handle.name == "Rotate Handles")
                    {
                        RotateHandles.Add(Handle);
                    }
                }
            }
            UpdateMultiplierLabel();
            SetTranslationMultiplier(TransformToolList[0].TranslationMultiplier);
            SetRotationMultiplier(TransformToolList[0].RotationMultiplier);
        }

        public void SetTranslationMultiplier(float value)
        {
            
            
            for (int i = 0; i < TransformToolList.Count; i++)
            {
                TransformToolList[i].TranslationMultiplier = ValueToMultiplier(value);
            }
            UpdateMultiplierLabel();
        }

        public void SetRotationMultiplier(float value)
        {
            
            
            for (int i = 0; i < TransformToolList.Count; i++)
            {
                TransformToolList[i].RotationMultiplier = ValueToMultiplier(value);
            }
            UpdateMultiplierLabel();
        }

        public void ChangeToggle()
        {
            ReplicToggle.isOn = !(ReplicToggle.isOn);
            ShowReplic(ReplicToggle.isOn);
        }

        private float ValueToMultiplier(float value)
        {
            float Multiplier;
            if(value < 0.5f)
            {
                Multiplier = 1 * value / 0.5f;
            }
            else if(value > 0.5f)
            {
                Multiplier = 1.0f + 4.0f * (value-0.5f) / 0.5f;
            }
            else
            {
                Multiplier = 1.0f;
            }
            return Multiplier;
        }

        public void ShowReplic(bool value)
        {
            DirectTarget.GetComponent<Renderer>().enabled = value;
            for (int i = 0; i < DirectTarget.transform.childCount; i++)
            {
                DirectTarget.transform.GetChild(i).GetComponent<Renderer>().enabled = value;
            }
        }

        private void UpdateMultiplierLabel()
        {
            TranslationMultiplierLabel.text = TransformToolList[0].TranslationMultiplier.ToString("f3");
            RotationMultiplierLabel.text = TransformToolList[0].RotationMultiplier.ToString("f3");
        }

        public void UpdateUI()
        {
            OnModeChanged(ModeDropdown.value);
            OnTypeChanged(TypeDropdown.value);
            OnSpaceChanged(SpaceDropdown.value);
            SetTranslationMultiplier(MoveSlider.value);
            SetRotationMultiplier(RotateSlider.value);
            if (ReplicToggle != null)
            {
                ShowReplic(ReplicToggle.isOn); 
            }
            
        }
        public void UpdateMode()
        {
            OnTypeChanged(TypeDropdown.value);
            OnSpaceChanged(SpaceDropdown.value);
            SetTranslationMultiplier(MoveSlider.value);
            SetRotationMultiplier(RotateSlider.value);
            if (ReplicToggle != null)
            {
                ShowReplic(ReplicToggle.isOn);
            }
        }

        public void StopManipulation()
        {
            foreach (GameObject Handles in TranslateHandles)
            {
                Transform[] allHandles = Handles.gameObject.GetComponentsInChildren<Transform>(true);

                foreach (Transform Handle in allHandles)
                {
                    if (Handle.GetComponent<MyObjectManipulator>() != null)
                    {
                        Handle.GetComponent<MyObjectManipulator>().ForceEndManipulation();
                    }
                }   
            }
            foreach (GameObject Handles in RotateHandles)
            {
                Transform[] allHandles = Handles.gameObject.GetComponentsInChildren<Transform>(true);

                foreach (Transform Handle in allHandles)
                {
                    if (Handle.GetComponent<MyObjectManipulator>() != null)
                    {
                        Handle.GetComponent<MyObjectManipulator>().ForceEndManipulation();
                    }
                }
            }           
            if (DirectTarget.GetComponent<ObjectManipulator>() != null)
            {
                SetTargetToOriginalColor();
                DirectTarget.GetComponent<ObjectManipulator>().ForceEndManipulation();                
            }
        }

        public void TranslationTask()
        {
            ModeDropdown.value = 0;
            TypeDropdown.value = 0;
            SpaceDropdown.value = 0;
            MoveSlider.value = 0.5f;
            RotateSlider.value = 0.5f;
            

        }
        public void RotationTask()
        {
            ModeDropdown.value = 0;
            TypeDropdown.value = 1;
            SpaceDropdown.value = 1;
            MoveSlider.value = 0.5f;
            RotateSlider.value = 0.5f;
            

        }
        public void TranslationAndRotationTask()
        {
            ModeDropdown.value = 0;
            TypeDropdown.value = 2;
            SpaceDropdown.value = 1;
            MoveSlider.value = 0.5f;
            RotateSlider.value = 0.5f;
           
        }

        public void UpdateReplicPosition(ManipulationEventData eventData)
        {
            UpdateReplicPosition();
        }

        public void UpdateReplicPosition()
        {
            if (Mode == ManipulationMode.Set)
            {
                ReplicPosition = ComputeLocalPosition(DirectTarget.transform.position, WorldCoordinateSystem);
            }            
        }

        public void ReturnReplicPosition(ManipulationEventData eventData)
        {
            ReturnReplicPosition();
        }

        public void ReturnReplicPosition()
        {
            if (Mode != ManipulationMode.Set)
            {
                DirectTarget.transform.position = ComputeWorldPosition(ReplicPosition, WorldCoordinateSystem);
                if(DirectTarget.GetComponent<MyReplicaManipulation>() != null)
                {
                    DirectTarget.GetComponent<MyReplicaManipulation>().Init();
                }
            }
        }

        public void UpdateInteractionWithTargetNum(ManipulationEventData eventData)
        {
            UpdateInteractionWithTargetNum();
        }

        public void UpdateInteractionWithTargetNum()
        {
            InteractionWithTargetNum++;
        }

        public void UpdateDirectInteractionWithTargetNum(ManipulationEventData eventData)
        {
            UpdateDirectInteractionWithTargetNum();
        }

        public void UpdateDirectInteractionWithTargetNum()
        {
            DirectInteractionWithTargetNum++;
        }

        public void InitInteractionWithTargetNum()
        {
            InteractionWithTargetNum = 0;
            DirectInteractionWithTargetNum = 0;
        }

        public int GetInteractionWithTargetNum()
        {
            return InteractionWithTargetNum;
        }
        public int GetDirectInteractionWithTargetNum()
        {
            return DirectInteractionWithTargetNum;
        }

        

        public void UpdateInteractionWithMoveSliderUINum()
        {
            if(!OnMoveSlider)
            {
                InteractionWithUINum++;
                InteractionWithMoveSliderUINum++;
                OnMoveSlider = true;
            }
            OnRotateSlider = false;
        }

        public void UpdateInteractionWithRotateSliderUINum()
        {
            if (!OnRotateSlider)
            {
                InteractionWithUINum++;
                InteractionWithRotateSliderUINum++;
                OnRotateSlider = true;                
            }
            OnMoveSlider = false;
        }

        public void UpdateInteractionWithUnReDoUINum()
        {
            InteractionWithUINum++;
            InteractionWithUnReDoUINum++;
            OnMoveSlider = false;
            OnRotateSlider = false;
        }

        

        public void UpdateInteractionWithShowUINum()
        {
            InteractionWithUINum++;
            InteractionWithShowUINum++;
            OnMoveSlider = false;
            OnRotateSlider = false;
        }

        public void UpdateInteractionWithModeUINum()
        {
            InteractionWithUINum++;
            InteractionWithModeUINum++;
            OnMoveSlider = false;
            OnRotateSlider = false;
        }
        public void UpdateInteractionWithTypeUINum()
        {
            InteractionWithUINum++;
            InteractionWithTypeUINum++;
            OnMoveSlider = false;
            OnRotateSlider = false;
        }
        public void UpdateInteractionWithSpaceUINum()
        {
            InteractionWithUINum++;
            InteractionWithSpaceUINum++;
            OnMoveSlider = false;
            OnRotateSlider = false;
        }

        public void InitInteractionWithUINum()
        {
            InteractionWithUINum = 0;
            InteractionWithMoveSliderUINum = 0;
            InteractionWithRotateSliderUINum = 0;
            InteractionWithUnReDoUINum = 0;
            InteractionWithShowUINum = 0;
            InteractionWithModeUINum = 0;
            InteractionWithTypeUINum = 0;
            InteractionWithSpaceUINum = 0;
        }

        public int GetInteractionWithUINum()
        {
            return InteractionWithUINum;
        }
        public int GetInteractionWithMoveSliderUINum()
        {
            return InteractionWithMoveSliderUINum;
        }

        public int GetInteractionWithRotateSliderUINum()
        {
            return InteractionWithRotateSliderUINum;
        }
        public int GetInteractionWithUnReDoUINum()
        {
            return InteractionWithUnReDoUINum;
        }
        public int GetInteractionWithShowUINum()
        {
            return InteractionWithShowUINum;
        }
        public int GetInteractionWithModeUINum()
        {
            return InteractionWithModeUINum;
        }
        public int GetInteractionWithTypeUINum()
        {
            return InteractionWithTypeUINum;
        }
        public int GetInteractionWithSpaceUINum()
        {
            return InteractionWithSpaceUINum;
        }

        public void UpdateGlobal()
        {
            foreach (MyTransformTool TransformTool in TransformToolList)
            {
                TransformTool.UpdateGlobal();
            }
        }

        public void SetTargetToActivationColor(ManipulationEventData eventData)
        {
            SetTargetToActivationColor();
        }

        public void SetTargetToActivationColor()
        {
            MeshOutline targetoutline = RealTarget.GetComponent<MeshOutline>();
            targetoutline.enabled = true;
           
        }
        public void SetTargetToOriginalColor(ManipulationEventData eventData)
        {
            SetTargetToOriginalColor();
        }

        public void SetTargetToOriginalColor()
        {
            MeshOutline targetoutline = RealTarget.GetComponent<MeshOutline>();
            targetoutline.enabled = false;
        }

        public void SetModeDropdown(int value)
        {
            ModeDropdown.value = value;
            UpdateUI();
        }

        public void SetTypeDropdown(int value)
        {
            TypeDropdown.value = value;
            UpdateUI();
        }

        public void SetSpaceDropdown(int value)
        {
            SpaceDropdown.value = value;
            UpdateUI();
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

        public void DirectVoiceCommand()
        {
            ModeDropdown.value = 0;
        }
        public void WidgetVoiceCommand()
        {
            ModeDropdown.value = 1;
        }
        public void SetVoiceCommand()
        {

            ModeDropdown.value = 2;

        }
        public void TranslateVoiceCommand()
        {

            TypeDropdown.value = 0;

        }
        public void RotateVoiceCommand()
        {

            TypeDropdown.value = 1;

        }
        public void AllVoiceCommand()
        {

            TypeDropdown.value = 2;

        }
        public void GlobalVoiceCommand()
        {
            SpaceDropdown.value = 0;

        }
        public void LocalVoiceCommand()
        {

            SpaceDropdown.value = 1;

        }
        public void UndoVoiceCommand()
        {

            UnDo();
            UpdateInteractionWithUnReDoUINum();

        }
        public void RedoVoiceCommand()
        {

            ReDo();
            UpdateInteractionWithUnReDoUINum();

        }
        public void ShowReplicVoiceCommand()
        {

            ReplicToggle.isOn = true;

        }

        public void HideReplicVoiceCommand()
        {
            ReplicToggle.isOn = false;
        }


        public void SetTranslateScaleMin()
        {
            MoveSlider.value = 0.1f;
        }

        public void SetTranslateScaleMax()
        {
            MoveSlider.value = 0.625f;
        }

        public void SetTranslateScaleOne()
        {
            MoveSlider.value = 0.5f;
        }

        public void SetRotateScaleMin()
        {
            RotateSlider.value = 0.1f;
        }

        public void SetRotateScaleMax()
        {
            RotateSlider.value = 0.625f;
        }

        public void SetRotateScaleOne()
        {
            RotateSlider.value = 0.5f;
        }

    }
}
