using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTargetCorrection : MonoBehaviour
{
    public Transform WorldCoordinateSystem;

    private Vector3 GlobalPosition;
    private Quaternion GlobalRotation;


    private void Awake()
    {
        if (WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
        if (WorldCoordinateSystem != null)
        {
            GlobalPosition = WorldCoordinateSystem.InverseTransformPoint(this.transform.position);
            GlobalRotation =  Quaternion.Inverse(WorldCoordinateSystem.rotation) * this.transform.rotation;
        }
            
    }

    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    // Update is called once per frame
    void Update()
    {
        TargetCorrection();
    }

    public void TargetCorrection()
    {
        this.transform.position = WorldCoordinateSystem.TransformPoint(GlobalPosition);
        this.transform.rotation = WorldCoordinateSystem.rotation * GlobalRotation;
    }
}
