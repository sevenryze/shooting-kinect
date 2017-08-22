using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace ControlServer
{
    public partial class ControlServer : Form
    {
        public ControlServer()
        {
            InitializeComponent();
        }
        //分别创建一个监听客户端的线程和套接字
        Thread threadWatch = null;
        Socket socketWatch = null;

        private void btnStartService_Click(object sender, EventArgs e)
        {
            //定义一个套接字用于监听客户端发来的信息  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //发送信息 需要1个IP地址和端口号
            //获取服务端IPv4地址
            IPAddress ipAddress = GetLocalIPv4Address();
            lblIP.Text = ipAddress.ToString();
            //给服务端赋予一个端口号
            int port = 8888;
            lblPort.Text = port.ToString();

            //将IP地址和端口号绑定到网络节点endpoint上 
            IPEndPoint endpoint = new IPEndPoint(ipAddress, port);
            //将负责监听的套接字绑定网络端点
            socketWatch.Bind(endpoint);
            //将套接字的监听队列长度设置为20
            socketWatch.Listen(20);
            //创建一个负责监听客户端的线程 
            threadWatch = new Thread(WatchConnecting);
            //将窗体线程设置为与后台同步
            threadWatch.IsBackground = true;
            //启动线程
            threadWatch.Start();
            txtMsg.AppendText("控制端已经启动,开始监听子端传来的信息!" + "\r\n");
            btnStartService.Enabled = false;
        }

        /// <summary>
        /// 获取本地IPv4地址
        /// </summary>
        /// <returns>本地IPv4地址</returns>
        public IPAddress GetLocalIPv4Address()
        {
            IPAddress localIPv4 = null;
            //获取本机所有的IP地址列表
            IPAddress[] ipAddressList = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipAddress in ipAddressList)
            {
                //判断是否是IPv4地址
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork) //AddressFamily.InterNetwork表示IPv4 
                {
                    localIPv4 = ipAddress;
                }
                else
                    continue;
            }
            return localIPv4;
        }

        //用于保存所有通信客户端的Socket
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();

        //创建与客户端建立连接的套接字
        Socket socConnection = null;
        string clientName = null; //创建访问客户端的名字
        IPAddress clientIP; //访问客户端的IP
        int clientPort; //访问客户端的端口号
        /// <summary>
        /// 持续不断监听客户端发来的请求, 用于不断获取客户端发送过来的连续数据信息
        /// </summary>
        private void WatchConnecting()
        {
            while (true)
            {
                try
                {
                    socConnection = socketWatch.Accept();
                }
                catch (Exception ex)
                {
                    txtMsg.AppendText(ex.Message); //提示套接字监听异常
                    break;
                }
                //获取访问客户端的IP
                clientIP = (socConnection.RemoteEndPoint as IPEndPoint).Address;
                //获取访问客户端的Port
                clientPort = (socConnection.RemoteEndPoint as IPEndPoint).Port;
                //创建访问客户端的唯一标识 由IP和端口号组成 
                clientName = "IP: " + clientIP +" Port: "+ clientPort;
                lstClients.Items.Add(clientName); //在客户端列表添加该访问客户端的唯一标识
                dicSocket.Add(clientName, socConnection); //将客户端名字和套接字添加到添加到数据字典中

                txtMsg.AppendText("IP: " + clientIP + " Port: " + clientPort + " 的子端与您连接成功,现在你们可以开始通信了.\r\n");
            }
        }

        /// <summary>
        /// 发送控制信息到子端的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        private void ServerSendControl(string sendMsg)
        {
            string ScreenName = null; //创建访问演示端的名字（唯一标识）
            //获取演示端信息文本框输入的控制端IP和Port
            IPAddress ScreenIPAddress = IPAddress.Parse(txtIP.Text.Trim());
            int ScreenPort = int.Parse(txtPort.Text.Trim());
            //创建访问客户端的唯一标识 由IP和端口号组成 
            ScreenName = "IP: " + ScreenIPAddress + " Port: " + ScreenPort;

            //将输入的字符串转换成 机器可以识别的字节数组
            byte[] arrServerMsg = Encoding.UTF8.GetBytes(sendMsg);
            //实际发送的字节数组比实际输入的长度多1 用于存取标识符
            byte[] arrServerSendMsg = new byte[arrServerMsg.Length + 1];
            arrServerSendMsg[0] = 2;  //在索引为0的位置上添加一个标识符
            Buffer.BlockCopy(arrServerMsg, 0, arrServerSendMsg, 1, arrServerMsg.Length);
            //获得相应的套接字 并将字节数组信息发送出去
            dicSocket[ScreenName.ToString()].Send(arrServerSendMsg);     
        }

        //将信息发送到到客户端
        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            //调用发送信息的方法 并检查发送是否成功
            ServerSendMsg(txtSendMsg.Text);
            txtSendMsg.Clear();
        }

        //关闭服务端
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        Boolean[] is_double_order = new Boolean[6] { false, false, false, false, false, false };
        
        /// <summary>
        /// 处理接收到的控制信息
        /// </summary>
        /// <param name="Msg">控制信息格式的字符串</param>
        private void handleRecControl(string Msg)
        {
            if (is_rec_control_start == false)
            {
                return;
            }
            int[] pXY = new int[2];
            float x = 0;
            float y = 0;
            int[] keyControl = new int[2];

            if (Msg[0] == 'M' && Msg[1] == 'A'){
                pXY = getNumberfromString(Msg);
                txtMouse1.Clear();
                txtSendMouse1.Clear();
                txtMouse1.AppendText("X1 - " + pXY[0].ToString() + "  Y1 - " + pXY[1].ToString());
                if (is_send_control_start == false) return;
                x = pXY[0] * 1366 / 640;
                y = pXY[1] * 768 / 480;
                ServerSendControl("MA" + 'X' + x.ToString() + 'Y' + y.ToString() + "END");
                txtSendMouse1.AppendText("X1 - " + x.ToString() + "  Y1 - " + y.ToString());
                return;
            }
            if (Msg[0] == 'M' && Msg[1] == 'B'){
                pXY = getNumberfromString(Msg);
                txtMouse2.Clear();
                txtSendMouse2.Clear();
                txtMouse2.AppendText("X2 - " + pXY[0].ToString() + "  Y2 - " + pXY[1].ToString());
                if (is_send_control_start == false) return;
                x = pXY[0] *1366 / 640;
                y = pXY[1] *768 / 480;
                ServerSendControl("MB" + 'X' + x.ToString() + 'Y' + y.ToString() + "END");
                txtSendMouse2.AppendText("X2 - " + x.ToString() + "  Y2 - " + y.ToString());
                return;
            }
            if (Msg[0] == 'M' && Msg[1] == 'C')
            {
                pXY = getNumberfromString(Msg);
                txtMouse3.Clear();
                txtSendMouse3.Clear();
                txtMouse3.AppendText("X3 - " + pXY[0].ToString() + "  Y3 - " + pXY[1].ToString());
                if (is_send_control_start == false) return;
                x = pXY[0] *1366 / 640;
                y = pXY[1] *768 / 480;
                ServerSendControl("MC" + 'X' + x.ToString() + 'Y' + y.ToString() + "END");
                txtSendMouse3.AppendText("X3 - " + x.ToString() + "  Y3 - " + y.ToString());
                return;
            }
            if (Msg[0] == 'M' && Msg[1] == 'D')
            {
                pXY = getNumberfromString(Msg);
                txtMouse4.Clear();
                txtSendMouse4.Clear();
                txtMouse4.AppendText("X4 - " + pXY[0].ToString() + "  Y4 - " + pXY[1].ToString());
                if (is_send_control_start == false) return;
                x = pXY[0] *1366 / 640;
                y = pXY[1] *768 / 480;
                ServerSendControl("MD" + 'X' + x.ToString() + 'Y' + y.ToString() + "END");
                txtSendMouse4.AppendText("X4 - " + x.ToString() + "  Y4 - " + y.ToString());
                return;
            }
            if (Msg[0] == 'K' && Msg[1] == 'A'){
                keyControl = getNumberfromString2(Msg);
                txtKeyboard1.Clear();
                txtSendKeyboard1.Clear();
                if (keyControl[0] == 12){
                    txtKeyboard1.AppendText(" 单手 - 左旋 ");
                    if (is_send_control_start == false) return;
                    if (is_double_order[2] == true) 
                    {
                        is_double_order[0] = false;
                        is_double_order[1] = false;
                        is_double_order[3] = false;
                        is_double_order[4] = false;
                        is_double_order[5] = false;
                        return; 
                    }
                    ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                    is_double_order[2] = true;
                    txtSendKeyboard1.AppendText(" 单手 - 左旋 ");
                    return;
                }
                if (keyControl[0] == 15){
                    txtKeyboard1.AppendText(" 单手 - 缩放  半径 = " + keyControl[1].ToString());
                    if (is_send_control_start == false) return;
                    //if (is_double_order[5] == true)
                    //{
                        is_double_order[0] = false;
                        is_double_order[1] = false;
                        is_double_order[2] = false;
                        is_double_order[3] = false;
                        is_double_order[4] = false;
                        //return;
                    //}
                    ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                    //is_double_order[5] = true;
                    txtSendKeyboard1.AppendText(" 单手 - 缩放  半径 = " + keyControl[1].ToString());
                    return;
                }
                if (keyControl[0] == 14)
                {
                   /* 
                    if (is_send_control_start == false) return;
                    if (is_double_order[4] == true)
                    {
                        is_double_order[0] = false;
                        is_double_order[1] = false;
                        is_double_order[2] = false;
                        is_double_order[3] = false;
                        is_double_order[5] = false;
                        return;
                    }
                    if (keyControl[1] == 0)
                    {
                        txtKeyboard1.AppendText(" 单手 - 向左旋转");
                        if (is_send_control_start == false) return;
                        ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                        is_double_order[4] = true;
                        txtSendKeyboard1.AppendText(" 单手 - 向左旋转");
                        return;
                    }
                    if (keyControl[1] == 1)
                    {
                        txtKeyboard1.AppendText(" 单手 - 向右旋转");
                        if (is_send_control_start == false) return;
                        ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                        is_double_order[4] = true;
                        txtSendKeyboard1.AppendText(" 单手 - 向右旋转");
                        return;
                    }
                    if (keyControl[1] == 2)
                    {
                        txtKeyboard1.AppendText(" 单手 - 向上翻转");
                        if (is_send_control_start == false) return;
                        ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                        is_double_order[4] = true;
                        txtSendKeyboard1.AppendText(" 单手 - 向上翻转");
                        return;
                    }
                    if (keyControl[1] == 3)
                    {
                        txtKeyboard1.AppendText(" 单手 - 向下翻转");
                        if (is_send_control_start == false) return;
                        ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                        is_double_order[4] = true;
                        txtSendKeyboard1.AppendText(" 单手 - 向下翻转");
                        return;
                    }*/

                    txtKeyboard1.AppendText(" 单手 - 右旋");
                    if (is_send_control_start == false) return;
                    if (is_double_order[4] == true)
                    {
                        is_double_order[0] = false;
                        is_double_order[1] = false;
                        is_double_order[2] = false;
                        is_double_order[3] = false;
                        is_double_order[5] = false;
                        return;
                    }
                    ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                    is_double_order[4] = true;
                    txtSendKeyboard1.AppendText(" 单手 - 右旋");
                    return;
                }
                if (keyControl[0] == 13)
                {
                    txtKeyboard1.AppendText(" 单手 - 取消所有操作及使模型静止");
                    if (is_send_control_start == false) return;
                    if (is_double_order[3] == true)
                    {
                        is_double_order[0] = false;
                        is_double_order[1] = false;
                        is_double_order[2] = false;
                        is_double_order[4] = false;
                        is_double_order[5] = false;
                        return;
                    }
                    ServerSendControl("KA" + 'X' + keyControl[0].ToString() + 'Y' + keyControl[1].ToString() + "END");
                    is_double_order[3] = true;
                    txtSendKeyboard1.AppendText(" 单手 - 取消所有操作及使模型静止");
                    return;
                }
            }
        }

        /// <summary>
        /// 提取坐标控制信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private int[] getNumberfromString(String str)
        {
            int[] returnXY = new int[2];
            returnXY[0] = 0;
            returnXY[1] = 0;

            string XStr = string.Empty;
            string YStr = string.Empty;

            int i = 0;
            for (; str[i] != 'X'; i++)
            {
            }
            for (; str[i] != 'Y'; i++)
            {
                if (Char.IsNumber(str, i) == true)
                {
                    XStr += str.Substring(i, 1);
                }
            }
            for (; str[i] != 'E'; i++)
            {
                if (Char.IsNumber(str, i) == true)
                {
                    YStr += str.Substring(i, 1);
                }
            }
            //MessageBox.Show(XStr);
            //MessageBox.Show(YStr);
            for (int j = 0; j < XStr.Length; j++)
            {
                returnXY[0] = returnXY[0] * 10 + XStr[j] - '0';
            }
            for (int j = 0; j < YStr.Length; j++)
            {
                returnXY[1] = returnXY[1] * 10 + YStr[j] - '0';
            }
            //MessageBox.Show(returnXY[0].ToString());
            //MessageBox.Show(returnXY[1].ToString());
            return returnXY;
        }

        /// <summary>
        /// 提取键盘控制信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private int[] getNumberfromString2(String str)
        {
            int[] returnXY = new int[2];
            returnXY[0] = 0;
            returnXY[1] = 0;

            string XStr = string.Empty;
            string YStr = string.Empty;

            int i = 0;
            for (; str[i] != 'X'; i++)
            {
            }
            for (; str[i] != 'Y'; i++)
            {
                if (Char.IsNumber(str, i) == true)
                {
                    XStr += str.Substring(i, 1);
                }
            }
            for (; str[i] != 'E'; i++)
            {
                if (Char.IsNumber(str, i) == true)
                {
                    YStr += str.Substring(i, 1);
                }
            }
            //MessageBox.Show(XStr);
            //MessageBox.Show(YStr);
            for (int j = 0; j < XStr.Length; j++)
            {
                returnXY[0] = returnXY[0] * 10 + XStr[j] - '0';
            }
            for (int j = 0; j < YStr.Length; j++)
            {
                returnXY[1] = returnXY[1] * 10 + YStr[j] - '0';
            }
            //MessageBox.Show(returnXY[0].ToString());
            //MessageBox.Show(returnXY[1].ToString());
            return returnXY;
        }          
    }
}
