namespace ControlServer
{
    partial class ControlServer
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.txtSendMsg = new System.Windows.Forms.TextBox();
            this.btnSendMsg = new System.Windows.Forms.Button();
            this.txtMsg = new System.Windows.Forms.TextBox();
            this.btnStartService = new System.Windows.Forms.Button();
            this.lstClients = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblIP = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnClearSelectedState = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMouse1 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtKeyboard1 = new System.Windows.Forms.TextBox();
            this.txtMouse2 = new System.Windows.Forms.TextBox();
            this.txtMouse3 = new System.Windows.Forms.TextBox();
            this.txtMouse4 = new System.Windows.Forms.TextBox();
            this.txtKeyboard2 = new System.Windows.Forms.TextBox();
            this.txtKeyboard3 = new System.Windows.Forms.TextBox();
            this.txtKeyboard4 = new System.Windows.Forms.TextBox();
            this.labelCondition = new System.Windows.Forms.Label();
            this.txtSendMouse4 = new System.Windows.Forms.TextBox();
            this.txtSendMouse3 = new System.Windows.Forms.TextBox();
            this.txtSendMouse2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtSendMouse1 = new System.Windows.Forms.TextBox();
            this.txtSendKeyboard1 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtSendKeyboard2 = new System.Windows.Forms.TextBox();
            this.txtSendKeyboard3 = new System.Windows.Forms.TextBox();
            this.txtSendKeyboard4 = new System.Windows.Forms.TextBox();
            this.btnRecControlStart = new System.Windows.Forms.Button();
            this.btnRecControlStop = new System.Windows.Forms.Button();
            this.btnSendControlStart = new System.Windows.Forms.Button();
            this.btnSendControlStop = new System.Windows.Forms.Button();
            this.labelCondition2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(192, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 23;
            this.label3.Text = "控制信息:";
            // 
            // txtSendMsg
            // 
            this.txtSendMsg.Location = new System.Drawing.Point(228, 401);
            this.txtSendMsg.Name = "txtSendMsg";
            this.txtSendMsg.Size = new System.Drawing.Size(412, 21);
            this.txtSendMsg.TabIndex = 22;
            this.txtSendMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSendMsg_KeyDown);
            // 
            // btnSendMsg
            // 
            this.btnSendMsg.Location = new System.Drawing.Point(646, 399);
            this.btnSendMsg.Name = "btnSendMsg";
            this.btnSendMsg.Size = new System.Drawing.Size(75, 23);
            this.btnSendMsg.TabIndex = 21;
            this.btnSendMsg.Text = "发送信息";
            this.btnSendMsg.UseVisualStyleBackColor = true;
            this.btnSendMsg.Click += new System.EventHandler(this.btnSendMsg_Click);
            // 
            // txtMsg
            // 
            this.txtMsg.Location = new System.Drawing.Point(194, 30);
            this.txtMsg.Multiline = true;
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMsg.Size = new System.Drawing.Size(519, 365);
            this.txtMsg.TabIndex = 19;
            // 
            // btnStartService
            // 
            this.btnStartService.Location = new System.Drawing.Point(37, 234);
            this.btnStartService.Name = "btnStartService";
            this.btnStartService.Size = new System.Drawing.Size(68, 23);
            this.btnStartService.TabIndex = 15;
            this.btnStartService.Text = "启动服务";
            this.btnStartService.UseVisualStyleBackColor = true;
            this.btnStartService.Click += new System.EventHandler(this.btnStartService_Click);
            // 
            // lstClients
            // 
            this.lstClients.FormattingEnabled = true;
            this.lstClients.ItemHeight = 12;
            this.lstClients.Location = new System.Drawing.Point(4, 29);
            this.lstClients.Name = "lstClients";
            this.lstClients.Size = new System.Drawing.Size(164, 112);
            this.lstClients.TabIndex = 24;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(175, 406);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 26;
            this.label4.Text = "控制端:";
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.ForeColor = System.Drawing.Color.Red;
            this.lblIP.Location = new System.Drawing.Point(35, 15);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(0, 12);
            this.lblIP.TabIndex = 27;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(117, 234);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(69, 21);
            this.btnExit.TabIndex = 29;
            this.btnExit.Text = "退出服务";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 16;
            this.label2.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 12);
            this.label1.TabIndex = 17;
            this.label1.Text = "IP:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblPort);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.lblIP);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(174, 60);
            this.groupBox1.TabIndex = 30;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "控制端信息";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.lblPort.Location = new System.Drawing.Point(41, 36);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(0, 12);
            this.lblPort.TabIndex = 28;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnClearSelectedState);
            this.groupBox2.Controls.Add(this.lstClients);
            this.groupBox2.Location = new System.Drawing.Point(12, 78);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(174, 150);
            this.groupBox2.TabIndex = 31;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "子端列表";
            // 
            // btnClearSelectedState
            // 
            this.btnClearSelectedState.Location = new System.Drawing.Point(77, -1);
            this.btnClearSelectedState.Name = "btnClearSelectedState";
            this.btnClearSelectedState.Size = new System.Drawing.Size(69, 24);
            this.btnClearSelectedState.TabIndex = 25;
            this.btnClearSelectedState.Text = "取消选择";
            this.btnClearSelectedState.UseVisualStyleBackColor = true;
            this.btnClearSelectedState.Click += new System.EventHandler(this.btnClearSelectedState_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(717, 138);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 12);
            this.label5.TabIndex = 35;
            this.label5.Text = "接收的坐标信息:";
            // 
            // txtMouse1
            // 
            this.txtMouse1.Location = new System.Drawing.Point(719, 156);
            this.txtMouse1.Name = "txtMouse1";
            this.txtMouse1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMouse1.Size = new System.Drawing.Size(240, 21);
            this.txtMouse1.TabIndex = 34;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(989, 135);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(95, 12);
            this.label6.TabIndex = 37;
            this.label6.Text = "接收的控制信息:";
            // 
            // txtKeyboard1
            // 
            this.txtKeyboard1.Location = new System.Drawing.Point(991, 156);
            this.txtKeyboard1.Name = "txtKeyboard1";
            this.txtKeyboard1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtKeyboard1.Size = new System.Drawing.Size(240, 21);
            this.txtKeyboard1.TabIndex = 36;
            // 
            // txtMouse2
            // 
            this.txtMouse2.Location = new System.Drawing.Point(719, 183);
            this.txtMouse2.Name = "txtMouse2";
            this.txtMouse2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMouse2.Size = new System.Drawing.Size(240, 21);
            this.txtMouse2.TabIndex = 52;
            // 
            // txtMouse3
            // 
            this.txtMouse3.Location = new System.Drawing.Point(719, 210);
            this.txtMouse3.Name = "txtMouse3";
            this.txtMouse3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMouse3.Size = new System.Drawing.Size(240, 21);
            this.txtMouse3.TabIndex = 53;
            // 
            // txtMouse4
            // 
            this.txtMouse4.Location = new System.Drawing.Point(719, 237);
            this.txtMouse4.Name = "txtMouse4";
            this.txtMouse4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMouse4.Size = new System.Drawing.Size(240, 21);
            this.txtMouse4.TabIndex = 54;
            // 
            // txtKeyboard2
            // 
            this.txtKeyboard2.Location = new System.Drawing.Point(991, 183);
            this.txtKeyboard2.Name = "txtKeyboard2";
            this.txtKeyboard2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtKeyboard2.Size = new System.Drawing.Size(240, 21);
            this.txtKeyboard2.TabIndex = 55;
            // 
            // txtKeyboard3
            // 
            this.txtKeyboard3.Location = new System.Drawing.Point(991, 210);
            this.txtKeyboard3.Name = "txtKeyboard3";
            this.txtKeyboard3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtKeyboard3.Size = new System.Drawing.Size(240, 21);
            this.txtKeyboard3.TabIndex = 56;
            // 
            // txtKeyboard4
            // 
            this.txtKeyboard4.Location = new System.Drawing.Point(991, 237);
            this.txtKeyboard4.Name = "txtKeyboard4";
            this.txtKeyboard4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtKeyboard4.Size = new System.Drawing.Size(240, 21);
            this.txtKeyboard4.TabIndex = 57;
            // 
            // labelCondition
            // 
            this.labelCondition.AutoSize = true;
            this.labelCondition.Font = new System.Drawing.Font("SimSun", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelCondition.Location = new System.Drawing.Point(933, 30);
            this.labelCondition.Name = "labelCondition";
            this.labelCondition.Size = new System.Drawing.Size(332, 29);
            this.labelCondition.TabIndex = 64;
            this.labelCondition.Text = "尚未从子端接收控制信息";
            // 
            // txtSendMouse4
            // 
            this.txtSendMouse4.Location = new System.Drawing.Point(719, 365);
            this.txtSendMouse4.Name = "txtSendMouse4";
            this.txtSendMouse4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendMouse4.Size = new System.Drawing.Size(240, 21);
            this.txtSendMouse4.TabIndex = 69;
            // 
            // txtSendMouse3
            // 
            this.txtSendMouse3.Location = new System.Drawing.Point(719, 338);
            this.txtSendMouse3.Name = "txtSendMouse3";
            this.txtSendMouse3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendMouse3.Size = new System.Drawing.Size(240, 21);
            this.txtSendMouse3.TabIndex = 68;
            // 
            // txtSendMouse2
            // 
            this.txtSendMouse2.Location = new System.Drawing.Point(719, 311);
            this.txtSendMouse2.Name = "txtSendMouse2";
            this.txtSendMouse2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendMouse2.Size = new System.Drawing.Size(240, 21);
            this.txtSendMouse2.TabIndex = 67;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(717, 266);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(95, 12);
            this.label7.TabIndex = 66;
            this.label7.Text = "发送的坐标信息:";
            // 
            // txtSendMouse1
            // 
            this.txtSendMouse1.Location = new System.Drawing.Point(719, 284);
            this.txtSendMouse1.Name = "txtSendMouse1";
            this.txtSendMouse1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendMouse1.Size = new System.Drawing.Size(240, 21);
            this.txtSendMouse1.TabIndex = 65;
            // 
            // txtSendKeyboard1
            // 
            this.txtSendKeyboard1.Location = new System.Drawing.Point(991, 284);
            this.txtSendKeyboard1.Name = "txtSendKeyboard1";
            this.txtSendKeyboard1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendKeyboard1.Size = new System.Drawing.Size(240, 21);
            this.txtSendKeyboard1.TabIndex = 65;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(989, 266);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 12);
            this.label8.TabIndex = 66;
            this.label8.Text = "发送的控制信息:";
            // 
            // txtSendKeyboard2
            // 
            this.txtSendKeyboard2.Location = new System.Drawing.Point(991, 311);
            this.txtSendKeyboard2.Name = "txtSendKeyboard2";
            this.txtSendKeyboard2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendKeyboard2.Size = new System.Drawing.Size(240, 21);
            this.txtSendKeyboard2.TabIndex = 67;
            // 
            // txtSendKeyboard3
            // 
            this.txtSendKeyboard3.Location = new System.Drawing.Point(991, 338);
            this.txtSendKeyboard3.Name = "txtSendKeyboard3";
            this.txtSendKeyboard3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendKeyboard3.Size = new System.Drawing.Size(240, 21);
            this.txtSendKeyboard3.TabIndex = 68;
            // 
            // txtSendKeyboard4
            // 
            this.txtSendKeyboard4.Location = new System.Drawing.Point(991, 365);
            this.txtSendKeyboard4.Name = "txtSendKeyboard4";
            this.txtSendKeyboard4.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSendKeyboard4.Size = new System.Drawing.Size(240, 21);
            this.txtSendKeyboard4.TabIndex = 69;
            // 
            // btnRecControlStart
            // 
            this.btnRecControlStart.Location = new System.Drawing.Point(16, 282);
            this.btnRecControlStart.Name = "btnRecControlStart";
            this.btnRecControlStart.Size = new System.Drawing.Size(131, 23);
            this.btnRecControlStart.TabIndex = 70;
            this.btnRecControlStart.Text = "接收控制信息启动";
            this.btnRecControlStart.UseVisualStyleBackColor = true;
            this.btnRecControlStart.Click += new System.EventHandler(this.btnRecControlStart_Click);
            // 
            // btnRecControlStop
            // 
            this.btnRecControlStop.Location = new System.Drawing.Point(16, 311);
            this.btnRecControlStop.Name = "btnRecControlStop";
            this.btnRecControlStop.Size = new System.Drawing.Size(131, 23);
            this.btnRecControlStop.TabIndex = 71;
            this.btnRecControlStop.Text = "接收控制信息停止";
            this.btnRecControlStop.UseVisualStyleBackColor = true;
            this.btnRecControlStop.Click += new System.EventHandler(this.btnRecControlStop_Click);
            // 
            // btnSendControlStart
            // 
            this.btnSendControlStart.Location = new System.Drawing.Point(16, 361);
            this.btnSendControlStart.Name = "btnSendControlStart";
            this.btnSendControlStart.Size = new System.Drawing.Size(131, 23);
            this.btnSendControlStart.TabIndex = 72;
            this.btnSendControlStart.Text = "发送控制信息启动";
            this.btnSendControlStart.UseVisualStyleBackColor = true;
            this.btnSendControlStart.Click += new System.EventHandler(this.btnSendControlStart_Click);
            // 
            // btnSendControlStop
            // 
            this.btnSendControlStop.Location = new System.Drawing.Point(16, 390);
            this.btnSendControlStop.Name = "btnSendControlStop";
            this.btnSendControlStop.Size = new System.Drawing.Size(131, 23);
            this.btnSendControlStop.TabIndex = 73;
            this.btnSendControlStop.Text = "发送控制信息停止";
            this.btnSendControlStop.UseVisualStyleBackColor = true;
            this.btnSendControlStop.Click += new System.EventHandler(this.btnSendControlStop_Click);
            // 
            // labelCondition2
            // 
            this.labelCondition2.AutoSize = true;
            this.labelCondition2.Font = new System.Drawing.Font("SimSun", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelCondition2.Location = new System.Drawing.Point(933, 74);
            this.labelCondition2.Name = "labelCondition2";
            this.labelCondition2.Size = new System.Drawing.Size(332, 29);
            this.labelCondition2.TabIndex = 74;
            this.labelCondition2.Text = "尚未向子端发送控制信息";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtPort);
            this.groupBox3.Controls.Add(this.txtIP);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Location = new System.Drawing.Point(719, 30);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(186, 90);
            this.groupBox3.TabIndex = 75;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "演示端信息:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(48, 55);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(123, 21);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "6666";
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(48, 15);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(123, 21);
            this.txtIP.TabIndex = 2;
            this.txtIP.Text = "10.71.44.16";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 58);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 12);
            this.label9.TabIndex = 1;
            this.label9.Text = "Port: ";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(13, 18);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 12);
            this.label10.TabIndex = 0;
            this.label10.Text = "IP: ";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.pictureBox1);
            this.groupBox4.Location = new System.Drawing.Point(37, 440);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(300, 200);
            this.groupBox4.TabIndex = 32;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "展示端预览图--主视图";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 17);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(294, 180);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.pictureBox2);
            this.groupBox5.Location = new System.Drawing.Point(343, 440);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(300, 200);
            this.groupBox5.TabIndex = 33;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "展示端预览图--俯视图";
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(3, 17);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(294, 180);
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.pictureBox3);
            this.groupBox6.Location = new System.Drawing.Point(649, 440);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(300, 200);
            this.groupBox6.TabIndex = 33;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "展示端预览图--左视图";
            // 
            // pictureBox3
            // 
            this.pictureBox3.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pictureBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox3.Location = new System.Drawing.Point(3, 17);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(294, 180);
            this.pictureBox3.TabIndex = 0;
            this.pictureBox3.TabStop = false;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.pictureBox4);
            this.groupBox7.Location = new System.Drawing.Point(955, 440);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(300, 200);
            this.groupBox7.TabIndex = 76;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "展示端预览图--透视图";
            // 
            // pictureBox4
            // 
            this.pictureBox4.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pictureBox4.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox4.Location = new System.Drawing.Point(3, 17);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(294, 180);
            this.pictureBox4.TabIndex = 0;
            this.pictureBox4.TabStop = false;
            // 
            // ControlServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1280, 658);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.labelCondition2);
            this.Controls.Add(this.btnSendControlStop);
            this.Controls.Add(this.btnSendControlStart);
            this.Controls.Add(this.btnRecControlStop);
            this.Controls.Add(this.btnRecControlStart);
            this.Controls.Add(this.txtSendKeyboard4);
            this.Controls.Add(this.txtSendMouse4);
            this.Controls.Add(this.txtSendKeyboard3);
            this.Controls.Add(this.txtSendMouse3);
            this.Controls.Add(this.txtSendKeyboard2);
            this.Controls.Add(this.txtSendMouse2);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtSendKeyboard1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtSendMouse1);
            this.Controls.Add(this.labelCondition);
            this.Controls.Add(this.txtKeyboard4);
            this.Controls.Add(this.txtKeyboard3);
            this.Controls.Add(this.txtKeyboard2);
            this.Controls.Add(this.txtMouse4);
            this.Controls.Add(this.txtMouse3);
            this.Controls.Add(this.txtMouse2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtKeyboard1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtMouse1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtSendMsg);
            this.Controls.Add(this.btnSendMsg);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.btnStartService);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "ControlServer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "控制中心";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            this.groupBox7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSendMsg;
        private System.Windows.Forms.Button btnSendMsg;
        private System.Windows.Forms.TextBox txtMsg;
        private System.Windows.Forms.Button btnStartService;
        private System.Windows.Forms.ListBox lstClients;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnClearSelectedState;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtMouse1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtKeyboard1;
        private System.Windows.Forms.TextBox txtMouse2;
        private System.Windows.Forms.TextBox txtMouse3;
        private System.Windows.Forms.TextBox txtMouse4;
        private System.Windows.Forms.TextBox txtKeyboard2;
        private System.Windows.Forms.TextBox txtKeyboard3;
        private System.Windows.Forms.TextBox txtKeyboard4;
        private System.Windows.Forms.Label labelCondition;
        private System.Windows.Forms.TextBox txtSendMouse4;
        private System.Windows.Forms.TextBox txtSendMouse3;
        private System.Windows.Forms.TextBox txtSendMouse2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtSendMouse1;
        private System.Windows.Forms.TextBox txtSendKeyboard1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtSendKeyboard2;
        private System.Windows.Forms.TextBox txtSendKeyboard3;
        private System.Windows.Forms.TextBox txtSendKeyboard4;
        private System.Windows.Forms.Button btnRecControlStart;
        private System.Windows.Forms.Button btnRecControlStop;
        private System.Windows.Forms.Button btnSendControlStart;
        private System.Windows.Forms.Button btnSendControlStop;
        private System.Windows.Forms.Label labelCondition2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox4;
    }
}

