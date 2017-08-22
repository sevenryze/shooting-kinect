using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace GestureClient
{
    public partial class GestureClient : Form
    {
        public GestureClient()
        {
            InitializeComponent();
            //关闭对文本框的非法线程操作检查
            TextBox.CheckForIllegalCrossThreadCalls = false;      
        }
        //创建 1个客户端套接字 和1个负责监听控制端请求的线程  
        Socket socketClient = null;
        Thread threadClient = null;

        private void btnConnectToServer_Click(object sender, EventArgs e)
        {
            //定义一个套字节监听  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
  
            //获取文本框输入的控制端IP和Port
            IPAddress serverIPAddress =IPAddress.Parse(txtIP.Text.Trim());
            int serverPort = int.Parse(txtPort.Text.Trim());
            IPEndPoint endpoint = new IPEndPoint(serverIPAddress, serverPort);
            //向指定的ip和端口号的控制端发送连接请求 用的方法是Connect 不是Bind
            socketClient.Connect(endpoint);
            //创建一个新线程 用于监听控制端发来的信息
            threadClient = new Thread(RecMsg);
            //将窗体线程设置为与后台同步
            threadClient.IsBackground = true;
            //启动线程
            threadClient.Start();
            txtMsg.AppendText("建立与控制端的连接,开始通信.\r\n");
            btnConnectToServer.Enabled = false;
        }

        /// <summary>
        /// 接收控制端信息的方法
        /// </summary>
        private void RecMsg()
        {
            while (true) //持续监听控制端发来的消息
            {
                string strRecMsg = null;
                int length = 0;
                //定义一个10M的内存缓冲区 用于临时性存储接收到的信息
                byte[] arrRecMsg = new byte[10 * 1024 * 1024];
                try
                {
                    //将客户端套接字接收到的字节数组存入内存缓冲区, 并获取其长度
                    length = socketClient.Receive(arrRecMsg);
                }
                catch (SocketException ex)
                {
                    txtMsg.AppendText("套接字异常消息:" + ex.Message + "\r\n");
                    txtMsg.AppendText("控制端已断开连接\r\n");
                    break;
                }
                catch (Exception ex)
                {
                    txtMsg.AppendText("系统异常消息: " + ex.Message + "\r\n");
                    break;
                }
                //将套接字获取到的字节数组转换为人可以看懂的字符串
                strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 0, length);
                //将文本框输入的信息附加到txtMsg中  并显示 谁,什么时间,换行,发送了什么信息 再换行
                txtMsg.AppendText("控制端在 " + GetCurrentTime() + " 给您发送了:\r\n" + strRecMsg + "\r\n");
            }
        }

        /// <summary>
        /// 发送普通信息的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        private void ClientSendMsg(string sendMsg)
        {
            //将输入的字符串信息转换为机器可以识别的字节数组
            byte[] arrClientMsg = Encoding.UTF8.GetBytes(sendMsg);
            //实际发送的字节数组比实际输入的长度多1 用于存取标识符
            byte[] arrClientSendMsg = new byte[arrClientMsg.Length + 1];
            arrClientSendMsg[0] = 0;  //在索引为0的位置上添加一个标识符
            Buffer.BlockCopy(arrClientMsg, 0, arrClientSendMsg, 1, arrClientMsg.Length);
            //调用客户端套接字发送字节数组
            socketClient.Send(arrClientSendMsg);
            txtMsg.AppendText("您在 " + GetCurrentTime() + " 向控制端发送了:\r\n" + sendMsg + "\r\n");
            //将已发送的信息从发送文本框中清除
            txtCMsg.Clear();
        }

        /// <summary>
        /// 发送控制信息的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        private void ClientSendControl(string sendMsg)
        {
            //将输入的字符串信息转换为机器可以识别的字节数组
            byte[] arrClientMsg = Encoding.UTF8.GetBytes(sendMsg);
            //实际发送的字节数组比实际输入的长度多1 用于存取标识符
            byte[] arrClientSendMsg = new byte[arrClientMsg.Length + 1];
            arrClientSendMsg[0] = 2;  //在索引为0的位置上添加一个标识符
            Buffer.BlockCopy(arrClientMsg, 0, arrClientSendMsg, 1, arrClientMsg.Length);
            //调用客户端套接字发送字节数组
            socketClient.Send(arrClientSendMsg);
        }

        //向控制端发送信息
        private void btnCSend_Click(object sender, EventArgs e)
        {
            ClientSendMsg(txtCMsg.Text);
        }
        //快捷键 Enter 发送信息
        private void txtCMsg_KeyDown(object sender, KeyEventArgs e)
        {   //当光标位于输入文本框上的情况下 发送信息的热键为回车键Enter 
            if (e.KeyCode == Keys.Enter)
            {
                //则调用客户端向控制端发送信息的方法
                ClientSendMsg(txtCMsg.Text);
            }
        }

        string filePath = null;   //文件的全路径
        string fileName = null;   //文件名称(不包含路径) 
        //选择要发送的文件
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofDialog = new OpenFileDialog();
            if (ofDialog.ShowDialog(this) == DialogResult.OK)
            {
                fileName = ofDialog.SafeFileName; //获取选取文件的文件名
                txtFileName.Text = fileName;      //将文件名显示在文本框上 
                filePath = ofDialog.FileName;     //获取包含文件名的全路径
            }
        }

        /// <summary>
        /// 发送文件的方法
        /// </summary>
        /// <param name="fileFullPath">文件全路径(包含文件名称)</param>
        private void SendFile(string fileFullPath)
        {
            if (fileFullPath == null)
            {
                txtMsg.AppendText("请选择需要发送的文件!");
                return;
            }
            else if (fileFullPath != null)
            {
                //创建文件流 
                FileStream fs = new FileStream(fileFullPath, FileMode.Open);
                //创建一个内存缓冲区 用于临时存储读取到的文件字节数组
                byte[] arrClientFile = new byte[10 * 1024 * 1024];
                //从文件流中读取文件的字节数组 并将其存入到缓冲区arrClientFile中 
                int realLength = fs.Read(arrClientFile, 0, arrClientFile.Length);  //realLength 为文件的真实长度
                byte[] arrClientSendedFile = new byte[realLength + 1];
                //给新增标识符(实际要发送的)字节数组的索引为0的位置上增加一个标识符1
                arrClientSendedFile[0] = 1;  //告诉机器该发送的字节数组为文件
                //将真实的文件字节数组完全拷贝到需要发送的文件字节数组中,从索引为1的位置开始存放,存放的字节长度为realLength.
                //实际发送的文件字节数组 arrSendedFile包含了2部分 索引为0位置上的标识符1 以及 后面的真实文件字节数组
                Buffer.BlockCopy(arrClientFile, 0, arrClientSendedFile, 1, realLength);
                //调用发送信息的方法 将文件名发送出去
                ClientSendMsg(fileName);
                socketClient.Send(arrClientSendedFile);
                txtMsg.AppendText("您在 " + GetCurrentTime() + " 向控制端发送了文件:\r\n" + fileName + "\r\n");
                //将发送文本框字符清除
                txtFileName.Clear();
            }
        }

        //点击文件发送按钮 发送文件
        private void btnSendFile_Click(object sender, EventArgs e)
        {
            //发送文件 
            SendFile(filePath);
        }

        /// <summary>
        /// 获取当前系统时间
        /// </summary>
        /// <returns>当前系统时间</returns>
        public DateTime GetCurrentTime()
        {
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            return currentTime;
        }

        //关闭客户端
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //自定义的信息编号
        class Message
        {
            public const int USER = 0x0400;
            public const int WM_mouse1 = USER + 100;
            public const int WM_mouse2 = USER + 101;
            public const int WM_mouse3 = USER + 102;
            public const int WM_mouse4 = USER + 103;
            public const int WM_keyboard1 = USER + 104;
            public const int WM_keyboard2 = USER + 105;
            public const int WM_keyboard3 = USER + 106;
            public const int WM_keyboard4 = USER + 107;
        }

        //本窗口事件响应的重载
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case Message.WM_mouse1:
                case Message.WM_mouse2:
                case Message.WM_mouse3:
                case Message.WM_mouse4:
                case Message.WM_keyboard1:
                case Message.WM_keyboard2:
                case Message.WM_keyboard3:
                case Message.WM_keyboard4:
                    message_handle(m);
                    break;
                default:
                    base.DefWndProc(ref m);
                    break;
            }
        }

/*        
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        private static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
*/

        Boolean is_start_gesture = false;

        // 开启姿态检测
        private void btnStartGesture_Click(object sender, EventArgs e)
        {
            is_start_gesture = true;
            btnStartGesture.Enabled = false;
            btnCloseGesture.Enabled = true;
            labelCondition.Text = "正在发送控制信息";
        }     
        // 关闭姿态检测
        private void btnCloseGesture_Click(object sender, EventArgs e)
        {
            is_start_gesture = false;
            btnStartGesture.Enabled = true;
            btnCloseGesture.Enabled = false;
            labelCondition.Text = "尚未发送控制信息";
        }
        
        //创建鼠标监视线程
        Thread threadMouse = null;
        // 开启鼠标模拟调试
        private void btnMouseVirtualStart_Click(object sender, EventArgs e)
        {
            threadMouse = new Thread(WatchMouse);
            threadMouse.IsBackground = true;
            threadMouse.Start();
            labelCondition2.Text = "正在使用鼠标模拟调试";
            btnMouseVirtualStart.Enabled = false;
            btnMouseVirtualStop.Enabled = true;
        }
        // 关闭鼠标模拟调试
        private void btnMouseVirtualStop_Click(object sender, EventArgs e)
        {
            threadMouse.Abort();
            labelCondition2.Text = "尚未使用鼠标模拟调试";
            btnMouseVirtualStart.Enabled = true;
            btnMouseVirtualStop.Enabled = false;
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        private static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        /// <summary>
        /// 鼠标监视线程
        /// </summary>
        private void WatchMouse()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(300);
                Point mouse = MousePosition;
                
                IntPtr hwnd = FindWindow(null, "运动识别端");
                if (hwnd == IntPtr.Zero)
                {
                    MessageBox.Show("没有找到运动识别端窗口");
                    return;
                }
                if (checkBoxHand1.CheckState == CheckState.Checked)
                {
                    PostMessage(hwnd, Message.WM_mouse1, (int)(mouse.X * 640 / 1600), (int)(mouse.Y * 480/ 900));
                   // PostMessage(hwnd, Message.WM_keyboard1, 300, 300);
                    
                }
                if (checkBoxHand2.CheckState == CheckState.Checked)
                {
                    PostMessage(hwnd, Message.WM_mouse2, (int)(mouse.X * 640 / 1600), (int)(mouse.Y * 480/ 900));
                    
                }
                if (checkBoxHand3.CheckState == CheckState.Checked)
                {
                    PostMessage(hwnd, Message.WM_mouse3, (int)(mouse.X * 640 / 1600), (int)(mouse.Y * 480/ 900));
                    
                }
                if (checkBoxHand4.CheckState == CheckState.Checked)
                {
                    PostMessage(hwnd, Message.WM_mouse4, (int)(mouse.X * 640 / 1600), (int)(mouse.Y * 480/ 900));
                   
                }
               
            }
        }
        
        /// <summary>
        /// 键盘监视
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_SYSKEYDOWN = 0x104;
            if (btnMouseVirtualStart.Enabled == false)
            {
                IntPtr hwnd = FindWindow(null, "运动识别端");
                if (hwnd == IntPtr.Zero)
                {
                    MessageBox.Show("没有找到运动识别端窗口");
                    return base.ProcessCmdKey(ref msg, keyData);
                }
                if ((msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN))
                {
                    switch (keyData)
                    {
                        case Keys.D2:
                            PostMessage(hwnd, Message.WM_keyboard1, 12, 0);
                            break;
                        case Keys.D5:
                            PostMessage(hwnd, Message.WM_keyboard1, 15, 1);
                            break;
                        case Keys.D3:
                            PostMessage(hwnd, Message.WM_keyboard1, 13, 0);
                            break;
                        case Keys.D4:
                            PostMessage(hwnd, Message.WM_keyboard1, 14, 0);
                            break;
                        case Keys.D6:
                            PostMessage(hwnd, Message.WM_keyboard1, 14, 1);
                            break;
                        case Keys.D7:
                            PostMessage(hwnd, Message.WM_keyboard1, 14, 2);
                            break;
                        case Keys.D8:
                            PostMessage(hwnd, Message.WM_keyboard1, 14, 3);
                            break;
                        default :
                            //PostMessage(hwnd, Message.WM_keyboard1, 0, 0);
                            break;
                    }
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        //每个数组用于记录相对应的坐标和控制信息
        public  int[] X = new int[6] { 0, 0, 0, 0, 0, 0 };
        public  int[] Y = new int[6] { 0, 0, 0, 0, 0, 0 };
        public  int[] key_control1 = new int[6] { 0, 0, 0, 0, 0, 0 };
        public  int[] key_control2 = new int[6] { 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// 自定义的信息标号处理
        /// </summary>
        /// <param name="m"></param>
        private void message_handle(System.Windows.Forms.Message m)
        {
            if (is_start_gesture == false)
            {
                return;
            }
            if (m.Msg == Message.WM_keyboard1){
                key_control1[0] = (int)m.WParam;
                key_control2[0] = (int)m.LParam;
                txtKeyboard1.Clear();
                txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 发送控制:\r\n" +
                "control1 - " + key_control1[0].ToString() + " control2 - " + key_control2[0].ToString() + "\r\n");
                ClientSendControl("KA" + 'X' + key_control1[0].ToString() + 'Y' + key_control2[0].ToString() + "END");
                return;
            }
            if (m.Msg == Message.WM_keyboard2){
                key_control1[1] = (int)m.WParam;
                key_control2[1] = (int)m.LParam;
                txtKeyboard2.Clear();
                txtKeyboard2.AppendText("您在 " + GetCurrentTime() + " 发送控制:\r\n" +
                "control1 - " + key_control1[1].ToString() + " control2 - " + key_control2[1].ToString() + "\r\n");
                ClientSendControl("KB" + 'X' + key_control1[1].ToString() + 'Y' + key_control2[1].ToString() + "END");
                return;
            }

            if (m.Msg == Message.WM_mouse1){
                X[0] = (int)m.WParam;
                Y[0] = (int)m.LParam;
                txtMouse1.Clear();
                txtMouse1.AppendText("您在 " + GetCurrentTime() + " 发送坐标:\r\n" +
                "X0 - " + (X[0]*1600/640).ToString() + "  Y0 - " + (Y[0]*900/480).ToString() + "\r\n");
                ClientSendControl("MA" + 'X' + X[0].ToString() + 'Y' + Y[0].ToString() + "END");
                return;
            }
            if (m.Msg == Message.WM_mouse2){
                X[1] = (int)m.WParam;
                Y[1] = (int)m.LParam;
                txtMouse2.Clear();
                txtMouse2.AppendText("您在 " + GetCurrentTime() + " 发送坐标:\r\n" +
                "X1 - " + (X[1]*1600/640).ToString() + "  Y1 - " + (Y[1]*900/480).ToString() + "\r\n");
                ClientSendControl("MB" + 'X' + X[1].ToString() + 'Y' + Y[1].ToString() + "END");
                return;
            }
            if (m.Msg == Message.WM_mouse3){
                X[2] = (int)m.WParam;
                Y[2] = (int)m.LParam;
                txtMouse3.Clear();
                txtMouse3.AppendText("您在 " + GetCurrentTime() + " 发送坐标:\r\n" +
                "X2 - " + (X[2]*1600/640).ToString() + "  Y2 - " + (Y[2]*900/480).ToString() + "\r\n");
                ClientSendControl("MC" + 'X' + X[2].ToString() + 'Y' + Y[2].ToString() + "END");
                return;
            }
            if (m.Msg == Message.WM_mouse4){
                X[3] = (int)m.WParam;
                Y[3] = (int)m.LParam;
                txtMouse4.Clear();
                txtMouse4.AppendText("您在 " + GetCurrentTime() + " 发送坐标:\r\n" +
                "X3 - " + (X[3]*1600/640).ToString() + "  Y3 - " + (Y[3]*900/480).ToString() + "\r\n");
                ClientSendControl("MD" + 'X' + X[3].ToString() + 'Y' + Y[3].ToString() + "END");
                return;
            } 
        }

    }
}
