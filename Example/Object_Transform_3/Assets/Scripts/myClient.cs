using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MYTCP;
using System.Threading;
using System;

public class myClient : MonoBehaviour
{
    //public static IPinfo myIp = new IPinfo();
    public static string ipAdress = "192.168.31.220";
    public static int ipPort = 5000;
    static string Information = null;

    string editString = "在此输入信息";
    static MyTcpClient client = new MyTcpClient(ipAdress, ipPort, "Client1");

    public GameObject RelativeCoordinateSystem;








    // Start is called before the first frame update
    void Start()
    {

        Thread MsgsThread = new Thread(ProsMsg);
        MsgsThread.Start();
        MsgsThread.IsBackground = true;
        client.BeginConnect();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SendInformation(string Information)
    {
        client.SendData(DataType.String, Information);
    }

   

    static void ProsMsg()
    {
        while (true)
        {
            try
            {
                Msg m = client.ReadMsg();//阻塞
                if (m == null)
                {
                    continue;
                }
                if (m.msgType != MsgType.Recv)
                {
                    Debug.Log((string)m.data);
                    continue;
                }

                var a = m.data;
                Information = a.ToString();


            }
            catch (System.Exception)
            {
                Information = null;
            }




        }
    }


    



    private Vector3 ComputeWorldPosition(Vector3 position, GameObject Father)
    {

        return Father.transform.TransformPoint(position);
    }

    private Quaternion ComputeWorldRotation(Quaternion rotation, GameObject Father)
    {
        return Father.transform.rotation * rotation;
    }
}
