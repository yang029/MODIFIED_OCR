using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MYTCP;
using System.Threading;


public class myServer : MonoBehaviour
{
    public static string ipAdress = "192.168.31.220";
    public static int ipPort = 5000;
    string editString = "在此输入信息";
    static MyTcpServer server = new MyTcpServer(ipAdress, ipPort);








    public Transform WorldCoordinateSystem;


    // Start is called before the first frame update
    void Start()
    {
        if (WorldCoordinateSystem == null) WorldCoordinateSystem = GameObject.Find("WorldCoordinateSystem").transform;
        Thread MsgsThread = new Thread(ProsMsg);
        MsgsThread.Start();
        MsgsThread.IsBackground = true;
        server.Start();
    }



    public void SendInformation(string Information)
    {
        server.Send(DataType.String, Information, "Client1");
    }








    // Update is called once per frame
    void Update()
    {

    }

    static void ProsMsg()
    {
        while (true)
        {
            Msg m = server.ReadMsg();//阻塞

            if (m.msgType != MsgType.Recv)
            {
                Debug.Log((string)m.data);
                continue;
            }
            Debug.Log(m.remoteClient.client.Client.RemoteEndPoint.ToString() + ":");
            var a = m.data;
            if (a != null)
                Debug.Log(a.GetType().ToString() + ":" + a.ToString());
        }
    }

    private Vector3 ComputeLocalPosition(Vector3 position, GameObject Father)
    {

        return Father.transform.InverseTransformPoint(position);
    }

    private Quaternion ComputeLocalRotation(Quaternion rotation, GameObject Father)
    {
        return Quaternion.Inverse(Father.transform.rotation) * rotation;
    }



}
