using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSameWorld : MonoBehaviour
{
    public Transform WorldCoordinateSystem;
    bool OneUpdate = true;
    
    // Start is called before the first frame update
    void Start()
    {
        if (WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
        UpdateWorld();
    }

    // Update is called once per frame
    void Update()
    {
        if(this.gameObject.activeInHierarchy && OneUpdate)
        {
            UpdateWorld();
            OneUpdate = false;
        }
    }

    public void UpdateWorld()
    {
        WorldCoordinateSystem.position = this.transform.position;
        WorldCoordinateSystem.rotation = this.transform.rotation;
    }
}
