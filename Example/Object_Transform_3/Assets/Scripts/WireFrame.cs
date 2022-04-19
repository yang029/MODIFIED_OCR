using MyTransformTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WireFrame : MonoBehaviour
{
    
    public Material mat;
    public GameObject target;
    public GameObject Replica;
    public GameObject wireframe;
    public Condition condition;
    public Color targetColor;

    private Vector3 OriginalPosition = new Vector3 (0.0f, 0.50f, 2.5f);
    private Quaternion OriginalRotation = Quaternion.identity;
    private MyTransformToolUI UIController;
    private MyCreatToolForObject CreatTool;
    private bool IsRecord = true;
    private float StartTime;
    private float EndTime;
    private WriteAndRead FileIO;
    public string Information = null;
    private string CurTask;
    private myClient myclient;
    public Transform WorldCoordinateSystem;
    private bool Tolerance = true;
    public Text Task_Text;

    void Start()
    {
        UIController = this.GetComponent<MyTransformToolUI>();
        CreatTool = this.GetComponent<MyCreatToolForObject>();
        FileIO = this.GetComponent<WriteAndRead>();
        if (WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
        myclient = this.GetComponent<myClient>();

        target.GetComponent<MeshRenderer>().material.color = targetColor;
        wireframe = CreatWireFrame(target);
        if (condition == Condition.DWBM)
        {
            CreatTool.CreatToolForObject(target);
        }
        else
        {
            CreatTool.CreatReplicaToolForObject(target);
            Replica = UIController.DirectTarget;
        }
        AddAllGameObjectInformation();
        UIController.OnModeChanged(3);
    }

    void Update()
    {
        if(Tolerance)
        {
            if (IsAligned() && !IsRecord)
            {
                ProcessAligned();
                target.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            if (!IsAligned())
            {
                AddAllGameObjectInformation();
            }
            SendInformation();
        }
        else
        {
            if(!IsRecord)
            {
                AddAllGameObjectInformation();
            }
            SendInformation();
        }        

    }

    public GameObject CreatWireFrame(GameObject target)
    {
        Mesh m = target.GetComponent<MeshFilter>().mesh;

        Vector3[] vertices = m.vertices;

        Mesh r = MeshProcessor.processForOutlineMesh(m);

        GameObject wireframe = new GameObject();
        wireframe.transform.position = target.transform.position;
        wireframe.transform.rotation = target.transform.rotation;
        wireframe.transform.localScale = target.transform.localScale;
        wireframe.name = target.name + " processed outline";

        wireframe.AddComponent<MeshFilter>().mesh = r;
        wireframe.AddComponent<MeshRenderer>().material = mat;
        wireframe.GetComponent<MeshRenderer>().material.SetFloat("_Width", 0.03f / target.transform.localScale.x);

        foreach (Vector3 vertice in vertices)
        {
            Vector3 Globalposition = target.transform.TransformPoint(vertice);
            GameObject sphere_cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere_cube.transform.position = Globalposition;
            sphere_cube.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            sphere_cube.transform.parent = target.transform;

            GameObject sphere_wireframe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere_wireframe.transform.position = Globalposition;
            sphere_wireframe.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            sphere_wireframe.transform.parent = wireframe.transform;
            if (vertice.y < 0 && vertice.z < 0)
            {
                sphere_cube.GetComponent<MeshRenderer>().material.color = Color.red;
                sphere_wireframe.GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else if (vertice.y > 0 && vertice.z < 0)
            {
                sphere_cube.GetComponent<MeshRenderer>().material.color = Color.blue;
                sphere_wireframe.GetComponent<MeshRenderer>().material.color = Color.blue;
            }
            else if (vertice.y < 0 && vertice.z > 0)
            {
                sphere_cube.GetComponent<MeshRenderer>().material.color = Color.green;
                sphere_wireframe.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else if (vertice.y > 0 && vertice.z > 0)
            {
                sphere_cube.GetComponent<MeshRenderer>().material.color = Color.yellow;
                sphere_wireframe.GetComponent<MeshRenderer>().material.color = Color.yellow;
            }
        }
            return wireframe;
    }

    public void RandomPose()
    {
        CurTask = "T+R";
        Task_Text.text = "Task: " + CurTask;
        Tolerance = true;
        target.GetComponent<MeshRenderer>().material.color = targetColor;
        UIController.InitInteractionWithTargetNum();
        StartTime = Time.time;
        target.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        target.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        if (condition == Condition.NFWBM && Replica != null)
        {
            Replica.transform.position = ComputeWorldPosition(UIController.ReplicPosition, WorldCoordinateSystem);
            Replica.transform.rotation = target.transform.rotation;
        }        
        wireframe.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        wireframe.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        Vector3 position = Random.onUnitSphere;
        Vector3 axis = Random.onUnitSphere;
        Quaternion rotation = Quaternion.AngleAxis(150.0f, axis);
        target.transform.position = wireframe.transform.position + position;
        wireframe.transform.rotation = target.transform.rotation * rotation;
        UIController.UpdateGlobal();
        UIController.TranslationAndRotationTask();
        UIController.InitInteractionWithUINum();
        UIController.UpdateUI();
        UIController.InitStore();
        IsRecord = false;
        myclient.enabled = true;
    }

    public void RandomPosition()
    {
        CurTask = "T";
        Task_Text.text = "Task: " + CurTask;
        Tolerance = true;
        target.GetComponent<MeshRenderer>().material.color = targetColor;
        UIController.InitInteractionWithTargetNum();
        StartTime = Time.time;
        target.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        target.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        if (condition == Condition.NFWBM && Replica != null)
        {
            Replica.transform.position = ComputeWorldPosition(UIController.ReplicPosition, WorldCoordinateSystem);
            Replica.transform.rotation = target.transform.rotation;
        }
        wireframe.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        wireframe.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        Vector3 position = Random.onUnitSphere;
        target.transform.position = wireframe.transform.position + position;
        UIController.UpdateGlobal();
        UIController.TranslationTask();
        UIController.InitInteractionWithUINum();
        UIController.UpdateUI();
        UIController.InitStore();
        IsRecord = false;
        myclient.enabled = true;
    }

    public void RandomRotation()
    {
        CurTask = "R";
        Task_Text.text = "Task: " + CurTask;
        Tolerance = true;
        target.GetComponent<MeshRenderer>().material.color = targetColor;
        UIController.InitInteractionWithTargetNum();
        StartTime = Time.time;
        target.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        target.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        if (condition == Condition.NFWBM && Replica != null)
        {
            Replica.transform.position = ComputeWorldPosition(UIController.ReplicPosition, WorldCoordinateSystem);
            Replica.transform.rotation = target.transform.rotation;
        }
        wireframe.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        wireframe.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        Vector3 axis = Random.onUnitSphere;
        Quaternion rotation = Quaternion.AngleAxis(150.0f, axis);
        wireframe.transform.rotation = target.transform.rotation * rotation;
        UIController.UpdateGlobal();
        UIController.RotationTask();
        UIController.InitInteractionWithUINum();
        UIController.UpdateUI();
        UIController.InitStore();
        IsRecord = false;
        myclient.enabled = true;
    }

    public void RandomFreePose()
    {
        CurTask = "Free R+T";
        Task_Text.text = "Task: " + CurTask;
        Tolerance = false;
        target.GetComponent<MeshRenderer>().material.color = targetColor;
        UIController.InitInteractionWithTargetNum();
        StartTime = Time.time;
        target.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        target.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        if (condition == Condition.NFWBM && Replica != null)
        {
            Replica.transform.position = ComputeWorldPosition(UIController.ReplicPosition, WorldCoordinateSystem);
            Replica.transform.rotation = target.transform.rotation;
        }
        wireframe.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        wireframe.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        Vector3 position = Random.onUnitSphere;
        Vector3 axis = Random.onUnitSphere;
        Quaternion rotation = Quaternion.AngleAxis(150.0f, axis);
        target.transform.position = wireframe.transform.position + position;
        wireframe.transform.rotation = target.transform.rotation * rotation;
        UIController.UpdateGlobal();
        UIController.TranslationAndRotationTask();
        UIController.InitInteractionWithUINum();
        UIController.UpdateUI();
        UIController.InitStore();
        IsRecord = false;
        myclient.enabled = true;
    }

    public void RandomFreePosition()
    {
        CurTask = "Free T";
        Task_Text.text = "Task: " + CurTask;
        Tolerance = false;
        target.GetComponent<MeshRenderer>().material.color = targetColor;
        UIController.InitInteractionWithTargetNum();
        StartTime = Time.time;
        target.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        target.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        if (condition == Condition.NFWBM && Replica != null)
        {
            Replica.transform.position = ComputeWorldPosition(UIController.ReplicPosition, WorldCoordinateSystem);
            Replica.transform.rotation = target.transform.rotation;
        }
        wireframe.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        wireframe.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        Vector3 position = Random.onUnitSphere;
        target.transform.position = wireframe.transform.position + position;
        UIController.UpdateGlobal();
        UIController.TranslationTask();
        UIController.InitInteractionWithUINum();
        UIController.UpdateUI();
        UIController.InitStore();
        IsRecord = false;
        myclient.enabled = true;
    }

    public void RandomFreeRotation()
    {
        CurTask = "Free R";
        Task_Text.text = "Task: " + CurTask;
        Tolerance = false;
        target.GetComponent<MeshRenderer>().material.color = targetColor;
        UIController.InitInteractionWithTargetNum();
        StartTime = Time.time;
        target.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        target.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        if (condition == Condition.NFWBM && Replica != null)
        {
            Replica.transform.position = ComputeWorldPosition(UIController.ReplicPosition, WorldCoordinateSystem);
            Replica.transform.rotation = target.transform.rotation;
        }
        wireframe.transform.position = ComputeWorldPosition(OriginalPosition, WorldCoordinateSystem);
        wireframe.transform.rotation = ComputeWorldRotation(OriginalRotation, WorldCoordinateSystem);
        Vector3 axis = Random.onUnitSphere;
        Quaternion rotation = Quaternion.AngleAxis(150.0f, axis);
        wireframe.transform.rotation = target.transform.rotation * rotation;
        UIController.UpdateGlobal();
        UIController.RotationTask();
        UIController.InitInteractionWithUINum();
        UIController.UpdateUI();
        UIController.InitStore();
        IsRecord = false;
        myclient.enabled = true;
    }


    public bool IsAligned()
    {
        bool result = false;
        if((Vector3.Magnitude(target.transform.position - wireframe.transform.position) < 0.01) && (Quaternion.Angle(target.transform.rotation, wireframe.transform.rotation) < 3))
        {
            result = true;
        }
        return result;
    }

    public void ProcessAligned()
    {
        target.GetComponent<MeshRenderer>().material.color = Color.green;
        UIController.OnModeChanged(3);
        Debug.Log("Success!");
        EndTime = Time.time;
        //Debug.Log("The task completion time:" + (EndTime - StartTime).ToString("f3"));
        //Debug.Log("The number of interaction with object:" + UIController.GetInteractionWithTargetNum().ToString());
        //Debug.Log("The number of interaction with UI:" + UIController.GetInteractionWithUINum().ToString());
        //Debug.Log("The error position: " + Vector3.Magnitude(target.transform.position - wireframe.transform.position).ToString("f6"));
        //Debug.Log("The error orientation: " + Quaternion.Angle(target.transform.rotation, wireframe.transform.rotation).ToString("f6"));

        AddAllGameObjectInformation();
        Information += "Record" + "," + "The current technique:" + "0" + ":" + condition.ToString() + "|";
        Information += "Record" + "," + "The current task:" + "1" + ":" + CurTask + "|";
        Information += "Record" + "," + "The task completion time:" + "2" + ":" + (EndTime - StartTime).ToString("f3") + "|";
        Information += "Record" + "," + "The number of interaction with object:" + "3" + ":" + UIController.GetInteractionWithTargetNum().ToString() + "|";
        Information += "Record" + "," + "The number of direct interaction with object:" + "4" + ":" + UIController.GetDirectInteractionWithTargetNum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with UI:" + "5" + ":" + UIController.GetInteractionWithUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with Move Slider UI:" + "6" + ":" + UIController.GetInteractionWithMoveSliderUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with Rotate Slider UI:" + "7" + ":" + UIController.GetInteractionWithRotateSliderUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with UnReDo Button UI:" + "8" + ":" + UIController.GetInteractionWithUnReDoUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with Show Toggle UI:" + "9" + ":" + UIController.GetInteractionWithShowUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with ModeDropdown UI:" + "10" + ":" + UIController.GetInteractionWithModeUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with TypeDropdown UI:" + "11" + ":" + UIController.GetInteractionWithTypeUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with SpaceDropdown UI:" + "12" + ":" + UIController.GetInteractionWithSpaceUINum().ToString() + "|";
        Information += "Record" + "," + "The error position:" + "13" + ":" + Vector3.Magnitude(target.transform.position - wireframe.transform.position).ToString("f6") + "|";
        Information += "Record" + "," + "The error orientation:" + "14" + ":" + Quaternion.Angle(target.transform.rotation, wireframe.transform.rotation).ToString("f6") + "|";
        SendInformation();
        IsRecord = true;
        myclient.enabled = false;
    }

    public void SendRecord()
    {
        target.GetComponent<MeshRenderer>().material.color = Color.green;
        UIController.OnModeChanged(3);
        Debug.Log("Success!");
        EndTime = Time.time;

        AddAllGameObjectInformation();
        Information += "Record" + "," + "The current technique:" + "0" + ":" + condition.ToString() + "|";
        Information += "Record" + "," + "The current task:" + "1" + ":" + CurTask + "|";
        Information += "Record" + "," + "The task completion time:" + "2" + ":" + (EndTime - StartTime).ToString("f3") + "|";
        Information += "Record" + "," + "The number of interaction with object:" + "3" + ":" + UIController.GetInteractionWithTargetNum().ToString() + "|";
        Information += "Record" + "," + "The number of direct interaction with object:" + "4" + ":" + UIController.GetDirectInteractionWithTargetNum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with UI:" + "5" + ":" + UIController.GetInteractionWithUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with Move Slider UI:" + "6" + ":" + UIController.GetInteractionWithMoveSliderUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with Rotate Slider UI:" + "7" + ":" + UIController.GetInteractionWithRotateSliderUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with UnReDo Button UI:" + "8" + ":" + UIController.GetInteractionWithUnReDoUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with Show Toggle UI:" + "9" + ":" + UIController.GetInteractionWithShowUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with ModeDropdown UI:" + "10" + ":" + UIController.GetInteractionWithModeUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with TypeDropdown UI:" + "11" + ":" + UIController.GetInteractionWithTypeUINum().ToString() + "|";
        Information += "Record" + "," + "The number of interaction with SpaceDropdown UI:" + "12" + ":" + UIController.GetInteractionWithSpaceUINum().ToString() + "|";
        Information += "Record" + "," + "The error position:" + "13" + ":" + Vector3.Magnitude(target.transform.position - wireframe.transform.position).ToString("f6") + "|";
        Information += "Record" + "," + "The error orientation:" + "14" + ":" + Quaternion.Angle(target.transform.rotation, wireframe.transform.rotation).ToString("f6") + "|";
        SendInformation();
        IsRecord = true;
        myclient.enabled = false;
    }

    public void AddGameObjectInformation(GameObject g)
    {
        if(g != null)
        {
            Information += "GameObject" + "," + g.name + "," + g.activeSelf.ToString() + ",";
            Vector3 position = g.transform.position;
            Quaternion rotation = g.transform.rotation;
            position = ComputeLocalPosition(position, WorldCoordinateSystem);
            rotation = ComputeLocalRotation(rotation, WorldCoordinateSystem);

            Information += position.x.ToString("f4") + "," + position.y.ToString("f4") + "," + position.z.ToString("f4") + ",";
            Information += rotation.x.ToString("f3") + "," + rotation.y.ToString("f3") + "," + rotation.z.ToString("f3") + "," + rotation.w.ToString("f3") + ",";
            Information += g.transform.localScale.x.ToString("f3") + "," + g.transform.localScale.y.ToString("f3") + "," + g.transform.localScale.z.ToString("f3") + "|";
        }
        else
        {
            Information += "GameObject" + "," + "Replica" + "," + false.ToString() + ",";
            Information += Vector3.zero.x.ToString("f4") + "," + Vector3.zero.y.ToString("f4") + "," + Vector3.zero.z.ToString("f4") + ",";
            Information += Quaternion.identity.x.ToString("f3") + "," + Quaternion.identity.y.ToString("f3") + "," + Quaternion.identity.z.ToString("f3") + "," + Quaternion.identity.w.ToString("f3") + ",";
            Information += Vector3.one.x.ToString("f3") + "," + Vector3.one.y.ToString("f3") + "," + Vector3.one.z.ToString("f3") + "|";
        }
    }

    public void AddAllGameObjectInformation()
    {
        Information = null;
        AddGameObjectInformation(target);
        AddGameObjectInformation(wireframe);
        AddGameObjectInformation(Replica);

    }

    
    public void SendInformation()
    {
        if((Information != null) && (myclient.enabled == true))
        {
            myclient.SendInformation(Information);
            Information = null;
        }        
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
