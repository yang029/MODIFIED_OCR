using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WriteAndRead : MonoBehaviour
{
    private string path;
    StreamWriter writer;
    StreamReader reader;
    bool hasWrite = false;
    List<string> Allmytxt = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        path = Application.dataPath + "/mytxt.txt";
        FileInfo file = new FileInfo(path);
        if (file.Exists)
        {
            file.Delete();
            file.Refresh();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void WriteIntoTxt(string message)
    {
        FileInfo file = new FileInfo(path);
        if (!file.Exists)
        {
            writer = file.CreateText();
        }
        else
        {
            writer = file.AppendText();
        }
        writer.WriteLine(message);
        writer.Flush();
        writer.Dispose();
        writer.Close();
    }
    public void ReadOutTxt(List<string> Allmytxt)
    {
        Allmytxt.Clear();
        reader = new StreamReader(path);
        string text;
        while ((text = reader.ReadLine()) != null)
        {
            Allmytxt.Add(text);
        }
        reader.Dispose();
        reader.Close();
    }
}
