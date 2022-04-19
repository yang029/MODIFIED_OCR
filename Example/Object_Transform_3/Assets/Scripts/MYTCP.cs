using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MYTCP
{
    public enum MsgType//信息类型
    { Good, Warning, Send, Recv, NewConnect, DisConnect }
    public enum DataType
    { Bool, Byte, Char, Short, Int32, Int64, Float, Double, String, NotData, Name, HeartBeat, Vector3 }
    public class Client//自定义客户端类
    {
        public TcpClient client;//内置client类
        public string name;//如客户端名称
        public Client(TcpClient client, string _name = "default")
        {
            this.client = client;
            this.name = _name;
        }
    }
    public class Msg
    {
        public object data;//object类型
        public DataType dataType;//数据类型
        public MsgType msgType;//消息类型
        public Client remoteClient;//哪个客户端的消息
        public Msg(object _data, MsgType _msgType, DataType _dataType = DataType.NotData, Client client = null)
        {
            this.data = _data;
            this.dataType = _dataType;
            this.msgType = _msgType;
            this.remoteClient = client;
        }
    }
    public class MyTcpClient
    {
        private Thread recvThread;
        private Thread heartBeatThread;
        public TcpClient client;
        public Queue<Msg> Msgs = new Queue<Msg>();
        public string clientIp;
        public int clientPort;
        public string name;
        public int heartBeatTime = 1000;
        private bool connected;
        public MyTcpClient(string ip, int port, string _name = "default")
        {
            clientIp = ip;
            clientPort = port;
            name = _name;
            connected = false;
            recvThread = new Thread(RecvData);
            recvThread.IsBackground = true;
            heartBeatThread = new Thread(HeartBeat);
            heartBeatThread.IsBackground = true;
            recvThread.Start();
        }
        public void HeartBeat()
        {
            while (connected)
            {
                //Console.WriteLine("心跳包监测连接！");
                try
                {
                    SendData(DataType.HeartBeat, null);
                }
                catch (Exception ex)
                {
                    connected = false;
                    Msgs.Enqueue(new Msg("Connection unexpectedly disconnected!", MsgType.DisConnect));
                }
                Thread.Sleep(heartBeatTime);
            }
        }
        /// <summary>
        /// 使用现有ip和port连接服务器，若已连接，则返回
        /// </summary>
        public void Connect()
        {
            Connect(clientIp, clientPort);
        }
        private void Connect(string ip, int port)
        {
            if (connected)
            {
                Msgs.Enqueue(new Msg("This client already connected!", MsgType.Warning));
                return;
            }
            try
            {
                client = new TcpClient(ip, port);
                connected = true;
                Msgs.Enqueue(new Msg("Connected successfully!", MsgType.NewConnect));
                SendData(DataType.Name, name);
                Thread.Sleep(10);
                new Thread(HeartBeat).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                connected = false;
                Msgs.Enqueue(new Msg("Connected failed!", MsgType.DisConnect));
            }
        }
        public void BeginConnect()
        {
            while (!connected)
            {
                Connect();
                Thread.Sleep(1000);
            }
        }
        public void Close()
        {
            client.Close();
            //Msgs.Enqueue(new Msg("Tcp client disconnected!", MsgType.DisConnect));
            connected = false;
        }
        private void RecvData()
        {
            byte[] data = new byte[1024 * 1024];
            while (true)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    if (client.Connected && stream.CanRead)
                    {
                        int length = stream.Read(data, 0, data.Length);
                        if (length == 0)//服务器端关闭
                        {
                            Msgs.Enqueue(new Msg(string.Format("Server active disconnected!"),
                            MsgType.DisConnect, DataType.NotData));
                            client.Close();
                            connected = false;
                        }
                        else
                        {
                            Msg m = Converter.ToDataMsg(data, length);
                            if (m.dataType == DataType.HeartBeat) continue;
                            Msgs.Enqueue(Converter.ToDataMsg(data, length));
                        }
                    }
                }
                catch (Exception) { }
                Thread.Sleep(1);
            }
        }
        public void SendData(DataType type, object data)
        {
            if (!connected) return;
            NetworkStream stream = client.GetStream();
            byte[] all = Converter.ToSendBytes(type, data);
            stream.Write(all, 0, all.Length);
            if (type != DataType.HeartBeat && type != DataType.Name)
                Msgs.Enqueue(new Msg(string.Format(
                    "Send a/an {0} data", type.ToString()), MsgType.Send, DataType.NotData));
            Thread.Sleep(1);
        }
        /// <summary>
        /// 阻塞方法
        /// </summary>
        /// <returns></returns>
        public Msg ReadMsg()//读取消息队列首个元素
        {
            while (Msgs.Count == 0) { Thread.Sleep(1); }
            return Msgs.Dequeue();
        }
    }
    public class MyTcpServer
    {
        public TcpListener server;//tcp server
        public List<Client> clients = new List<Client>();//连接的客户端链表
        public string HostName { get; }//本地计算机名
        public string IPV4Address { get; }//本机IPV4
        public string IPV6Address { get; }//本机IPV6
        public string ServerIP { get; set; }//服务器地址
        public int ServerPort { get; set; }//服务器端口
        public int heartBeatTime = 1000;

        public Queue<Msg> Msgs = new Queue<Msg>();//消息队列

        public MyTcpServer(string ip, int port)
        {
            ServerIP = ip;
            ServerPort = port;
            HostName = Dns.GetHostName();
            foreach (IPAddress address in Dns.GetHostAddresses(HostName))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPV4Address = address.ToString();
                    break;
                }
                else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    IPV6Address = address.ToString();
            }
            Thread HeartBeatThread = new Thread(HeartBeat);
            HeartBeatThread.IsBackground = true;
            HeartBeatThread.Start();
        }
        private void HeartBeat()
        {
            while (true)
            {
                List<Client> removes = new List<Client>();
                foreach (Client client in clients)
                {
                    try
                    {
                        Send(DataType.HeartBeat, null, client);
                    }
                    catch (Exception)
                    {
                        removes.Add(client);
                    }
                }
                foreach (Client client in removes)
                {
                    clients.Remove(client);
                    Msgs.Enqueue(new Msg(string.Format("Client {0}(name={1}) unexpectedly disconnected!", client.client.Client.RemoteEndPoint.ToString(), client.name),
                            MsgType.DisConnect, DataType.NotData, client));
                }
                Thread.Sleep(heartBeatTime);
            }
        }
        public void Start()
        {
            Start(ServerIP, ServerPort);
        }
        public void Start(string ip, int port)
        {
            ServerIP = ip;
            ServerPort = port;
            server = new TcpListener(IPAddress.Parse(ip), port);
            server.Start();
            Msgs.Enqueue(new Msg("Tcp Server Opened!", MsgType.Good));//服务端打开消息
            /*BeginAcceptTcpClient是一个异步方法，不阻塞。
             代码的处理逻辑为：
                (1)调用BeginAccetpTcpClient方法开开始连接新的连接，当连接事件发生时，回调函数被调用以完成连接操作；
                (2)上面DoAcceptTcpCliet方法通过AsyncState属性获得由BeginAcceptTcpClient传入的listner实例；
                (3)在得到listener对象后，用它调用EndAcceptTcpClient方法，该方法返回新的包含客户端信息的TcpClient。*/
            server.BeginAcceptTcpClient(DoAcceptTcpClient, server);
        }
        public void Close()
        {
            if (server != null)
            {
                server.Stop();//这里用stop，server.close不行，不改变IsBound
                Msgs.Enqueue(new Msg("Tcp Server Closed!", MsgType.Warning));//服务端关闭消息
            }
            foreach (Client c in clients)
            {
                c.client.Close();//断开所有客户端
            }
            clients.Clear();//清空客户端列表
        }
        public void Send(DataType type, object data, string ip, int port)
        {
            Client[] desClients = clients.FindAll(n =>
            {
                string[] info = n.client.Client.RemoteEndPoint.ToString().Split(':');
                if (info[0] == ip && port == int.Parse(info[1])) return true;
                return false;
            }).ToArray();
            Send(type, data, desClients);
        }
        /// <summary>
        /// 向某一ip或名称发送信息,判断标准是ip只包含数字和'.',否则就是名称
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="ipOrName"></param>
        public void Send(DataType type, object data, string ipOrName)
        {
            bool IsIP = true;
            ipOrName.ToList().ForEach(c => { if ((c < '0' || c > '9') && c != '.') IsIP = false; });
            if (IsIP)
            {
                Client[] desClients = clients.FindAll(n => n.client.Client.RemoteEndPoint.ToString().Split(':')[0] == ipOrName).ToArray();
                Send(type, data, desClients);
            }
            else
            {
                Client[] desClients = clients.FindAll(n => n.name == ipOrName).ToArray();
                Send(type, data, desClients);
            }

        }
        public void Send(DataType type, object data, params Client[] clients)
        {
            foreach (Client c in clients)
            {
                NetworkStream stream = c.client.GetStream();
                byte[] sendData = Converter.ToSendBytes(type, data);
                stream.Write(sendData, 0, sendData.Length);
                if (type != DataType.HeartBeat)
                    Msgs.Enqueue(new Msg(string.Format(
                    "Send a/an {0} data to {1}(name={2})", type.ToString(), c.client.Client.RemoteEndPoint.ToString(), c.name), MsgType.Send, DataType.NotData, c));
            }
            Thread.Sleep(1);
        }

        /// <summary>
        /// 阻塞方法，没有消息会停在这里,返回值[0]是一个client对象，[1]是从该client传来的消息string
        /// </summary>
        /// <returns></returns>
        public Msg ReadMsg()//读取消息队列首个元素
        {
            while (Msgs.Count == 0) { Thread.Sleep(1); }
            return Msgs.Dequeue();
        }
        private void DoAcceptTcpClient(IAsyncResult ar)
        {
            TcpListener listener;
            try//这个异常处理为了：当释放server后，ar为null，如果不加，会导致未设置引用对象的实例的异常，程序直接崩
            {
                listener = ar.AsyncState as TcpListener;
                if (!listener.Server.IsBound)
                {
                    return;
                }
                TcpClient tcpClient = listener.EndAcceptTcpClient(ar);
                Client client = new Client(tcpClient);

                clients.Add(client);
                Msgs.Enqueue(new Msg(string.Format("Client {0} connected!", client.client.Client.RemoteEndPoint.ToString()),
                    MsgType.NewConnect, DataType.NotData, client));

                Thread recThread = new Thread(RecMsg);//读取消息线程
                recThread.IsBackground = true;//后台线程，程序结束自动退出
                recThread.Start(client);
                listener.BeginAcceptTcpClient(DoAcceptTcpClient, listener);
            }
            catch (Exception) { return; }
        }
        private void RecMsg(object clientObj)
        {
            TcpClient tcpClient = (clientObj as Client).client;
            byte[] data = new byte[1024 * 1024];//一次最多读 1024 byte(hhhhhh)
            while (true)
            {
                try
                {
                    //服务器直接关闭
                    if (!server.Server.IsBound) { return; }
                    NetworkStream stream = tcpClient.GetStream();
                    int length = stream.Read(data, 0, data.Length);//这个函数好像是阻塞的
                    if (length == 0)
                    {
                        //client主动断开
                        clients.Remove(clientObj as Client);
                        Msgs.Enqueue(new Msg(string.Format("Client {0} active disconnected!", tcpClient.Client.RemoteEndPoint.ToString()),
                            MsgType.DisConnect, DataType.NotData, clientObj as Client));
                        return;
                    }
                    else//正常读取
                    {
                        Msg m = Converter.ToDataMsg(data, length);
                        Client tc = clientObj as Client;
                        m.remoteClient = tc;
                        if (m.dataType == DataType.Name)
                        {
                            tc.name = (string)m.data;
                            Msgs.Enqueue(new Msg(string.Format("{0} rename to {1}", tc.client.Client.RemoteEndPoint.ToString(), tc.name),
                                MsgType.Warning));
                        }
                        else if (m.dataType == DataType.HeartBeat) continue;
                        else
                        {
                            Msgs.Enqueue(m);
                        }

                    }
                }
                catch (Exception) { }
                Thread.Sleep(1);
            }
        }
    }
    static class Converter
    {
        static public byte[] ToSendBytes(DataType type, object data)
        {
            byte[] byteData = new byte[] { };
            switch (type)
            {
                case DataType.Bool:
                    byteData = BitConverter.GetBytes((bool)data);
                    break;
                case DataType.Byte:
                    byteData = BitConverter.GetBytes((byte)data);
                    break;
                case DataType.Char:
                    byteData = BitConverter.GetBytes((char)data);
                    break;
                case DataType.Short:
                    byteData = BitConverter.GetBytes((short)data);
                    break;
                case DataType.Int32:
                    byteData = BitConverter.GetBytes((Int32)data);
                    break;
                case DataType.Int64:
                    byteData = BitConverter.GetBytes((Int64)data);
                    break;
                case DataType.Float:
                    byteData = BitConverter.GetBytes((float)data);
                    break;
                case DataType.Double:
                    byteData = BitConverter.GetBytes((double)data);
                    break;
                case DataType.String:
                    byteData = Encoding.UTF8.GetBytes((string)data);
                    break;
                case DataType.NotData:
                    break;
                case DataType.Name:
                    byteData = Encoding.UTF8.GetBytes((string)data);
                    break;
                case DataType.HeartBeat:
                    byteData = new byte[0];
                    break;
                case DataType.Vector3:

                    break;
                default:
                    break;
            }
            byte[] res = new byte[1] { (byte)type }.Concat(byteData).ToArray();
            return res;
        }
        static public Msg ToDataMsg(byte[] data, int length)
        {
            DataType type = (DataType)data[0];
            object objData = null;
            switch (type)
            {
                case DataType.Bool:
                    objData = BitConverter.ToBoolean(data, 1);
                    break;
                case DataType.Byte:
                    objData = data[1];
                    break;
                case DataType.Char:
                    objData = (char)data[1];
                    break;
                case DataType.Short:
                    objData = BitConverter.ToInt16(data, 1);
                    break;
                case DataType.Int32:
                    objData = BitConverter.ToInt32(data, 1);
                    break;
                case DataType.Int64:
                    objData = BitConverter.ToInt64(data, 1);
                    break;
                case DataType.Float:
                    objData = BitConverter.ToSingle(data, 1);
                    break;
                case DataType.Double:
                    objData = BitConverter.ToDouble(data, 1);
                    break;
                case DataType.String:
                    objData = Encoding.UTF8.GetString(data, 1, length - 1);
                    break;
                case DataType.Name:
                    objData = Encoding.UTF8.GetString(data, 1, length - 1);
                    break;
                case DataType.NotData:
                    objData = data;
                    break;
                case DataType.HeartBeat:
                    break;
                default:
                    break;
            }
            return new Msg(objData, MsgType.Recv, type);
        }
    }
}
