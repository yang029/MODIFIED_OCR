using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgr : MonoBehaviour
{
    public GameObject ToleranceTask;
    public GameObject FreeTask;
    public GameObject SendRecord;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SceneStart()
    {
        SceneManager.LoadScene(0);
        //SceneManager.LoadScene("SceneManager");
    }

    public void SceneDWBM()
    {
        SceneManager.LoadScene(1);
        //SceneManager.LoadScene("DWBM");
    }

    public void SceneNFWBM()
    {
        SceneManager.LoadScene(2);
        //SceneManager.LoadScene("NFWBM");
    }

    public void SwitchTask()
    {
        if(ToleranceTask.activeInHierarchy)
        {
            ToleranceTask.SetActive(false);
            FreeTask.SetActive(true);
            SendRecord.SetActive(true);
        }
        else
        {
            ToleranceTask.SetActive(true);
            FreeTask.SetActive(false);
            SendRecord.SetActive(false);
        }
    }

}
