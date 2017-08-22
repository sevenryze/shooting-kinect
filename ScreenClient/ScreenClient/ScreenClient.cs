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

namespace ScreenClient
{
    public partial class ScreenClient : Form
    {
        public ScreenClient()
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
/*
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
                //判断发送过来的数据是控制信息
                if (arrRecMsg[0] == 2) //2为控制信息
                {
                    //将字节数组 转换为人可以读懂的字符串
                    strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 1, length - 1);//真实有用的文本信息要比接收到的少1(标识符)
                    handleRecControl(strRecMsg);
                    continue;
                }

                //将套接字获取到的字节数组转换为人可以看懂的字符串
                strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 0, length);
                //将文本框输入的信息附加到txtMsg中  并显示 谁,什么时间,换行,发送了什么信息 再换行
                txtMsg.AppendText("控制端在 " + GetCurrentTime() + " 给您发送了:\r\n" + strRecMsg + "\r\n");                          
            }
        }
*/
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
                //判断发送过来的数据是控制信息
                if (arrRecMsg[0] == 0x66) //2为控制信息
                {
                    GetKinectMessage(arrRecMsg);
                    continue;
                }

                //将套接字获取到的字节数组转换为人可以看懂的字符串
                strRecMsg = Encoding.UTF8.GetString(arrRecMsg, 0, length);
                //将文本框输入的信息附加到txtMsg中  并显示 谁,什么时间,换行,发送了什么信息 再换行
                txtMsg.AppendText("控制端在 " + GetCurrentTime() + " 给您发送了:\r\n" + strRecMsg + "\r\n");
            }
        }

        public enum ServerControlType
        {
            mouseMoveLeft = 0x01,
            mouseMoveRight,
            mouseMoveUp,
            mouseMoveDown,
            mouseLeftDown,
            mouseLeftUp,
            mouseRightDown,
            mouseRightUp,

            keybdDown,
            keybdUp,
        }

        private void GetKinectMessage(byte[] p_bytes)
        {
            UInt16 para1 = (UInt16)(p_bytes[2] | p_bytes[3] << 8);
            switch (p_bytes[1])
            {
                case (byte)ServerControlType.keybdDown:
                    HGX_KeyDown((HGX_Keybd_ScanCode)para1);
                    break;
                case (byte)ServerControlType.keybdUp:
                    HGX_KeyUp((HGX_Keybd_ScanCode)para1);
                    break;
                case (byte)ServerControlType.mouseLeftDown:
                    HGX_Mouse(HGX_MouseEventFlag.LeftDown, 0, 0);
                    break;
                case (byte)ServerControlType.mouseLeftUp:
                    HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0);
                    break;
                case (byte)ServerControlType.mouseMoveLeft:
                    HGX_Mouse(HGX_MouseEventFlag.Move, -30, 0);
                    break;
                case (byte)ServerControlType.mouseMoveRight:
                    HGX_Mouse(HGX_MouseEventFlag.Move, 30,  0);
                    break;
                case (byte)ServerControlType.mouseMoveUp:
                    HGX_Mouse(HGX_MouseEventFlag.Move, 0, -20);
                    break;
                case (byte)ServerControlType.mouseMoveDown:
                    HGX_Mouse(HGX_MouseEventFlag.Move, 0, 20);
                    break;
                default:
                    break;
            }
        }

        #region Input event funtions
               /// <summary>
        /// Import the SendInput() 
        /// Define the Structs used in the SendInput()
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "SendInput")]
        public static extern Int32 SendInput(Int32 nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public Int32 type;
            public MouseKeybdHardwareInputUnion mkhi;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MouseKeybdHardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInput mi;

            [FieldOffset(0)]
            public KeybdInput ki;

            [FieldOffset(0)]
            public HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public Int32 dx;
            public Int32 dy;
            public Int32 Mousedata;
            public Int32 dwFlags;
            public Int32 time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct KeybdInput
        {
            public UInt16 wVk;
            public UInt16 wScan;
            public UInt32 dwFlags;
            public Int32 time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public Int32 uMsg;
            public Int16 wParamL;
            public Int16 wParamH;
        }

        /// <summary>
        /// The ScanCode used in the wScan of KeybdInput struct
        /// </summary>
        public enum HGX_Keybd_ScanCode
        {
            W = 0x11,
            S = 0x1F,
            A = 0x1E,
            D = 0x20,
            R = 0x13,
            Space = 0x39,
            L_Control = 0x1D,
            Tab = 0x0F,

            U = 0x16,
            I = 0x17,

            J = 0x24,
            K = 0x25,

            Q1 = 0x02,
            Q2 = 0x03,
            Q3 = 0x04,
            Q4 = 0x05
        }
        public void HGX_KeyDown(HGX_Keybd_ScanCode p_ScanCode, int p_sendMessageIndex = 0)
        {
            if (p_sendMessageIndex == 0)
            {
                INPUT[] InputData = new INPUT[1];

                InputData[0].type = 1; // Indicate we have a keyboard event
                InputData[0].mkhi.ki.dwFlags = 0x0008; // Indicate we use the keyboard scancode to simulate event and ignore the wVK
                InputData[0].mkhi.ki.wScan = (UInt16)p_ScanCode;

                // Send key down event
                if (SendInput(1, InputData, Marshal.SizeOf(InputData[0])) == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SendInput failed with code: " + Marshal.GetLastWin32Error().ToString());
                    return;
                }
            }
        }
        public void HGX_KeyUp(HGX_Keybd_ScanCode p_ScanCode, int p_sendMessageIndex = 0)
        {
            if (p_sendMessageIndex == 0)
            {
                INPUT[] InputData = new INPUT[1];

                InputData[0].type = 1; // Indicate we have a keyboard event
                InputData[0].mkhi.ki.dwFlags = 0x0008 | 0x0002; // Indicate we use the keyboard scancode and key up event
                InputData[0].mkhi.ki.wScan = (UInt16)p_ScanCode;

                // Send key up event
                if (SendInput(1, InputData, Marshal.SizeOf(InputData[0])) == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SendInput failed with code: " + Marshal.GetLastWin32Error().ToString());
                    return;
                }
            }
           
        }

        /// <summary>
        /// MouseEventFlag
        /// </summary>
        public enum HGX_MouseEventFlag
        {
            Absolute = 0x8000,
            MouserEvent_Hwheel = 0x01000,
            Move = 0x0001,
            Move_noCoalesce = 0x2000,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            RightDown = 0x0008,
            RightUp = 0x0010,
            Wheel = 0x0800,
            XUp = 0x0100,
            XDown = 0x0080,
        }
        public void HGX_Mouse(HGX_MouseEventFlag p_MouseEventFlag, Int32 p_dx, Int32 p_dy, int p_sendMessageIndex = 0)
        {
            if (p_sendMessageIndex == 0)
            {
                INPUT[] InputData = new INPUT[1];

                InputData[0].type = 0; // Indicate we have a mouse event
                switch (p_MouseEventFlag)
                {
                    case HGX_MouseEventFlag.Absolute:
                        break;
                    case HGX_MouseEventFlag.MouserEvent_Hwheel:
                        break;
                    case HGX_MouseEventFlag.Move:
                        InputData[0].mkhi.mi.dwFlags = (Int32)HGX_MouseEventFlag.Move;
                        InputData[0].mkhi.mi.dx = p_dx;
                        InputData[0].mkhi.mi.dy = p_dy;
                        break;
                    case HGX_MouseEventFlag.Move_noCoalesce:
                        break;
                    case HGX_MouseEventFlag.LeftDown:
                        InputData[0].mkhi.mi.dwFlags = (Int32)HGX_MouseEventFlag.LeftDown;
                        break;
                    case HGX_MouseEventFlag.LeftUp:
                        InputData[0].mkhi.mi.dwFlags = (Int32)HGX_MouseEventFlag.LeftUp;
                        break;
                    case HGX_MouseEventFlag.MiddleDown:
                        break;
                    case HGX_MouseEventFlag.MiddleUp:
                        break;
                    case HGX_MouseEventFlag.RightDown:
                        InputData[0].mkhi.mi.dwFlags = (Int32)HGX_MouseEventFlag.RightDown;
                        break;
                    case HGX_MouseEventFlag.RightUp:
                        InputData[0].mkhi.mi.dwFlags = (Int32)HGX_MouseEventFlag.RightUp;
                        break;
                    case HGX_MouseEventFlag.Wheel:
                        break;
                    case HGX_MouseEventFlag.XUp:
                        break;
                    case HGX_MouseEventFlag.XDown:
                        break;
                    default:
                        break;
                }

                // Send mouse event
                if (SendInput(1, InputData, Marshal.SizeOf(InputData[0])) == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SendInput failed with code: " + Marshal.GetLastWin32Error().ToString());
                    return;
                }
            }          
           
        }
        #endregion

        Boolean is_start_gesture = false;

        private void btnStartGesture_Click(object sender, EventArgs e)
        {
            is_start_gesture = true;
            btnStartGesture.Enabled = false;
            btnCloseGesture.Enabled = true;
            labelCondition1.Text = "正在接收控制信息";
        }

        private void btnCloseGesture_Click(object sender, EventArgs e)
        {
            is_start_gesture = false;
            btnStartGesture.Enabled = true;
            btnCloseGesture.Enabled = false;
            labelCondition1.Text = "尚未接收控制信息";
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

        Boolean is_model_start = false;
        private void btnModelStart_Click(object sender, EventArgs e)
        {
            is_model_start = true;
            btnModelStart.Enabled = false;
            btnModelStop.Enabled = true;
            labelCondition2.Text = "正在控制模型运作";
        }
        private void btnModelStop_Click(object sender, EventArgs e)
        {
            is_model_start = false;
            btnModelStart.Enabled = true;
            btnModelStop.Enabled = false;
            labelCondition2.Text = "尚未控制模型运作";
        }

        Boolean[] is_double_order = new Boolean[6] { false, false, false, false, false, false };
        private void handleRecControl(string Msg)
        {
            if (is_start_gesture == false) 
                return;
            Boolean is_find_window = true;
            IntPtr hwnd = FindWindow(null, "Screen");
            if (hwnd == IntPtr.Zero)
            {
                //MessageBox.Show("没有找到演示端窗口");
                //return;
                is_find_window = false;
            }

            int[] pXY = new int[2];
            int[] keyControl = new int[2];

            if (Msg[0] == 'M' && Msg[1] == 'A')
            {
                pXY = getNumberfromString(Msg);
                txtMouse1.Clear();
                txtMouse1.AppendText("您在 " + GetCurrentTime() + " 接收坐标:\r\n");
                txtMouse1.AppendText("X1 - " + pXY[0].ToString() + "  Y1 - " + pXY[1].ToString());
                if (is_model_start == false) return;
                if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                SendMessage(hwnd, Message.WM_mouse1, (int)pXY[0], (int)pXY[1]);
                return;
            }
            if (Msg[0] == 'M' && Msg[1] == 'B')
            {
                pXY = getNumberfromString(Msg);
                txtMouse2.Clear();
                txtMouse2.AppendText("您在 " + GetCurrentTime() + " 接收坐标:\r\n");
                txtMouse2.AppendText("X2 - " + pXY[0].ToString() + "  Y2 - " + pXY[1].ToString());
                if (is_model_start == false) return;
                if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                PostMessage(hwnd, Message.WM_mouse2, (int)pXY[0], (int)pXY[1]);
                return;
            }
            if (Msg[0] == 'K' && Msg[1] == 'A')
            {
                keyControl = getNumberfromString2(Msg);
                txtKeyboard1.Clear();
                if (keyControl[0] == 12)
                {
                    txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                    txtKeyboard1.AppendText(" 单手 - 左旋 ");
                    if (is_model_start == false) return;
                    if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                    PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                    return;
                }
                if (keyControl[0] == 15)
                {
                    txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                    txtKeyboard1.AppendText(" 单手 - 缩放  半径 = " + keyControl[1].ToString());
                    if (is_model_start == false) return;
                    if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                    PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                    return;
                }
                if (keyControl[0] == 14)
                {
                    /*if (keyControl[1] == 0){
                        txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                        txtKeyboard1.AppendText(" 单手 - 左旋转");
                        if (is_model_start == false) return;
                        if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                        if (is_double_order[0] == true)
                        {
                            is_double_order[1] = false;
                            is_double_order[2] = false;
                            is_double_order[3] = false;
                            is_double_order[4] = false;
                            //is_double_order[5] = false;
                            return ;
                        }
                        PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                        is_double_order[0] = true;
                        return;
                    }
                    if (keyControl[1] == 1){
                        txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                        txtKeyboard1.AppendText(" 单手 - 右旋转");
                        if (is_model_start == false) return;
                        if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                        if (is_double_order[1] == true)
                        {
                            is_double_order[0] = false;
                            is_double_order[2] = false;
                            is_double_order[3] = false;
                            is_double_order[4] = false;
                            //is_double_order[5] = false;
                            return ;
                        }
                        PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                        is_double_order[1] = true;
                        return;
                    }
                    if (keyControl[1] == 2){
                        txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                        txtKeyboard1.AppendText(" 单手 - 上翻转");
                        if (is_model_start == false) return;
                        if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                        if (is_double_order[2] == true)
                        {
                            is_double_order[0] = false;
                            is_double_order[1] = false;
                            is_double_order[3] = false;
                            is_double_order[4] = false;
                            //is_double_order[5] = false;
                            return ;
                        }
                        PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                        is_double_order[2] = true;
                        return;
                    }
                    if (keyControl[1] == 3){
                        txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                        txtKeyboard1.AppendText(" 单手 - 下翻转");
                        if (is_model_start == false) return;
                        if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                        if (is_double_order[3] == true)
                        {
                            is_double_order[0] = false;
                            is_double_order[1] = false;
                            is_double_order[2] = false;
                            is_double_order[4] = false;
                            //is_double_order[5] = false;
                            return ;
                        }
                        PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                        is_double_order[3] = true;
                        return;
                    }*/
                    txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                    txtKeyboard1.AppendText(" 单手 - 右旋 ");
                    if (is_model_start == false) return;
                    if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                    PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                    return;
                }
                if (keyControl[0] == 13)
                {
                    txtKeyboard1.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                    txtKeyboard1.AppendText(" 单手 - 取消及静止");
                    if (is_model_start == false) return;
                    if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                    PostMessage(hwnd, Message.WM_keyboard1, keyControl[0], keyControl[1]);
                    return;
                }
                /*txtKeyboard1.Text = "未选中";
                if (is_model_start == false) return;
                if (is_find_window == false) { MessageBox.Show("没有找到演示端窗口"); return; }
                PostMessage(hwnd, Message.WM_keyboard1, 0, 0);
                return;*/
            }
            if (Msg[0] == 'K' && Msg[1] == 'B')
            {
                keyControl = getNumberfromString2(Msg);
                txtKeyboard2.Clear();
                if (keyControl[0] == 22)
                {
                    txtKeyboard2.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                    txtKeyboard2.AppendText(" 单手 - 选中 ");
                    if (is_model_start == false) return;
                    PostMessage(hwnd, Message.WM_keyboard2, keyControl[0], keyControl[1]);
                    return;
                }
                if (keyControl[0] == 25)
                {
                    txtKeyboard2.AppendText("您在 " + GetCurrentTime() + " 接收控制:\r\n");
                    txtKeyboard2.AppendText(" 单手 - 缩放  半径 = " + keyControl[1].ToString());
                    if (is_model_start == false) return;
                    PostMessage(hwnd, Message.WM_keyboard2, keyControl[0], keyControl[1]);
                    return;
                }

                
                txtKeyboard2.Text = "未选中";
                if (is_model_start == false) return;
                PostMessage(hwnd, Message.WM_keyboard2, 0, 0);
                return;
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

        /// <summary>
        /// 发送字符串信息到控制端的方法
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

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        private static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    }
}

       