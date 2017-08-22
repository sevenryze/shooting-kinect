
#define Use_CS
#define Debug

namespace HGX_Body_CS_Basics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using System.Runtime.InteropServices;
    using Microsoft.Kinect.Face;
    using System.Threading;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Main logic
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;

        private KinectSensor kinectSensor = null;

        /// <summary>
        /// The CS game handler 
        /// </summary>
        private IntPtr CS_WindowHandler = IntPtr.Zero;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
#if Use_CS
            CS_WindowHandler = FindWindow(null, "Counter-Strike");

            if (CS_WindowHandler == IntPtr.Zero)
            {
                System.Windows.MessageBox.Show("没有找到窗口");
                return;
            }

            SetForegroundWindow(CS_WindowHandler);

            Thread.Sleep(2000);
#endif
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);
            this.DataContext = this;

            // open the reader for the body and audio frames
            this.BodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.AudioFrameReader = this.kinectSensor.AudioSource.OpenReader();

            // Allocate 1024 bytes to hold a single audio sub frame. Duration sub frame 
            // is 16 msec, the sample rate is 16khz, which means 256 samples per sub frame. 
            // With 4 bytes per sample, that gives us 1024 bytes.
            this.AudioBuffer = new byte[this.kinectSensor.AudioSource.SubFrameLengthInBytes];

            // open the sensor
            this.kinectSensor.Open();

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

        /// <summary>
        /// Execute start up and shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.BodyFrameReader != null)
            {
                this.BodyFrameReader.FrameArrived += this.BodyReader_FrameArrivedEvent;
            }

            if (this.AudioFrameReader != null)
            {
                this.AudioFrameReader.FrameArrived += this.AudioReader_FrameArrivedEvent;
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.BodyFrameReader != null)
            {
                this.BodyFrameReader.Dispose();
                this.BodyFrameReader = null;
            }

            if (this.AudioFrameReader != null)
            {
                this.AudioFrameReader.Dispose();
                this.AudioFrameReader = null;
            }

            if (this.FaceFrameReader != null)
            {
                this.FaceFrameReader.Dispose();
                this.FaceFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
        #endregion

        #region Body tracking
        /// <summary>
        /// Flag that indicates if we have already got all reference
        /// </summary>
        private bool GetReference = false;

        /// <summary>
        /// We can only track ONE person once game so we should have his ID
        /// </summary>
        private ulong CurrentTrackingBodyID = 0;

        /// <summary>
        /// OrderStart is part of the GameStart
        /// </summary>
        private bool GameStart = false;
        private bool OrderStart = false;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader BodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// The reference value and logical flag
        /// </summary>
        private float RefCrouchLine = 0;
        private float RefForwardLine = 0;
        private float RefJumpLine = 0;

        private bool ChangedKnife = false;
        private bool ChangedBullet = false;
        private bool ChangedCrouch = false;
        private bool ChangedJump = false;
        private bool ChangedForward = false;
        private bool ChangedBomb = false;
        private bool ChangedGun = false;
        private bool FireTheBomb = false;

        private Int32 GunType = 2; // 1 means Long gun, 2 means handgun, 3 means knife, 4 means bomb;
        private Int32 PreBombGunType = 0;
        private Int32 PreKnifeGunType = 0;

        private float preAimPoint_X = 0;
        private float preAimPoint_Y = 0;

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void BodyReader_FrameArrivedEvent(object sender, BodyFrameArrivedEventArgs e)
        {
            if (CS_WindowHandler != GetForegroundWindow())
            {
                RefreshFlags();
                GameStart = false;
                OrderStart = false;
                GetReference = false;
                return;
            }

            bool t_BodyFrameProcessed = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    /* The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                        As long as those body objects are not disposed and not set to null in the array,
                        those body objects will be re-used. */
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    t_BodyFrameProcessed = true;
                }
            }

            if (t_BodyFrameProcessed)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    //Draw a transparent background to set the render size                   
                    dc.DrawRectangle(Brushes.Black, null,
                                        new Rect(0.0, 0.0, this.kinectSensor.DepthFrameSource.FrameDescription.Width,
                                                           this.kinectSensor.DepthFrameSource.FrameDescription.Height));

                    foreach (Body body in this.bodies)
                    {
                        if (body.IsTracked)
                        {
                            if (GetReference == true)
                            {
                                if (CurrentTrackingBodyID != body.TrackingId)
                                {
                                    return;
                                }
                            }

                            #region Map the CameraJointPoints to DepthJointPoints and find the min-depth joint
                            IReadOnlyDictionary<JointType, Joint> CameraJointPoints = body.Joints;

                            // convert the joint points to depth (display) space
                            Dictionary<JointType, DepthSpacePoint> DepthJointPoints = new Dictionary<JointType, DepthSpacePoint>();

                            // This is our gun AIM point
                            JointType MinDepthJointType = JointType.Head;

                            foreach (JointType jointType in CameraJointPoints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = CameraJointPoints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = 0.1f;
                                }

                                if (position.Z < CameraJointPoints[MinDepthJointType].Position.Z)
                                {
                                    MinDepthJointType = jointType;
                                }

                                DepthJointPoints[jointType] = this.kinectSensor.CoordinateMapper.MapCameraPointToDepthSpace(position);
                            }
                            #endregion

                            #region See if we will begin game
                            if (GameStart == false)
                            {
                                // Wait to start game angin
                                if (CameraJointPoints[JointType.HandLeft].Position.Z > CameraJointPoints[JointType.SpineMid].Position.Z
                                    && CameraJointPoints[JointType.HandRight].Position.Z > CameraJointPoints[JointType.SpineMid].Position.Z
                                    && body.HandLeftConfidence == TrackingConfidence.High
                                    && body.HandLeftState == HandState.Open
                                    && body.HandRightConfidence == TrackingConfidence.High
                                    && body.HandRightState == HandState.Open)
                                {
                                    GameStart = true;
                                    Console.WriteLine("Game Start.");
                                }
                                return;
                            }

                            if (GameStart == true)
                            {
                                // Wait to end the game
                                if (CameraJointPoints[JointType.HandLeft].Position.Z > CameraJointPoints[JointType.SpineMid].Position.Z
                                    && CameraJointPoints[JointType.HandRight].Position.Z > CameraJointPoints[JointType.SpineMid].Position.Z
                                    && body.HandLeftConfidence == TrackingConfidence.High
                                    && body.HandLeftState == HandState.Closed
                                    && body.HandRightConfidence == TrackingConfidence.High
                                    && body.HandRightState == HandState.Closed)
                                {
                                    // Begin to clear the lecagy flag
                                    GameStart = false;
                                    OrderStart = false;
                                    GetReference = false;

                                    RefreshFlags();
                                    Console.WriteLine("Game Over.");
                                    return;
                                }
                            }
                            #endregion

                            #region Get the reference value
                            if (GetReference == false)
                            {
                                if (body.HandLeftState == HandState.Lasso
                                    && body.HandLeftConfidence == TrackingConfidence.High
                                    && body.HandRightState == HandState.Lasso
                                    && body.HandRightConfidence == TrackingConfidence.High)
                                {
                                    RefForwardLine = (float)0.2 * (DepthJointPoints[JointType.KneeLeft].Y - DepthJointPoints[JointType.AnkleLeft].Y)
                                                 + DepthJointPoints[JointType.AnkleLeft].Y;
                                    RefCrouchLine = DepthJointPoints[JointType.SpineBase].Y
                                                - (float)0.3 * (DepthJointPoints[JointType.SpineBase].Y - DepthJointPoints[JointType.AnkleLeft].Y);
                                    RefJumpLine = DepthJointPoints[JointType.SpineBase].Y
                                                + (float)0.2 * (DepthJointPoints[JointType.SpineShoulder].Y - DepthJointPoints[JointType.SpineBase].Y);

                                    CurrentTrackingBodyID = body.TrackingId;

                                    // Create new face sources with body tracking ID
                                    FaceFrameSource = new FaceFrameSource(kinectSensor, body.TrackingId, faceFrameFeatures);

                                    // Create face new reader
                                    FaceFrameReader = FaceFrameSource.OpenReader();

                                    // Wire faceframe events
                                    FaceFrameReader.FrameArrived += FaceFrameReader_FrameArrivedEvent;

                                    Console.WriteLine("Get Reference, Enjoy the game.");

                                    GetReference = true;
                                }
                                return;
                            }
                            #endregion

                            #region Draw the joints and reference values
#if Debug
                            // Draw the joints in Depth space
                            foreach (JointType jointType in CameraJointPoints.Keys)
                            {
                                Brush drawBrush = null;

                                if (CameraJointPoints[jointType].TrackingState == TrackingState.Tracked)
                                {
                                    drawBrush = Brushes.White;
                                }
                                else if (CameraJointPoints[jointType].TrackingState == TrackingState.Inferred)
                                {
                                    drawBrush = Brushes.Yellow;
                                }

                                if (drawBrush != null)
                                {
                                    dc.DrawEllipse(drawBrush, null, new Point(DepthJointPoints[jointType].X, DepthJointPoints[jointType].Y), 3, 3);
                                }
                            }

                            // Draw the Real-MinDepth point
                           /* dc.DrawRectangle(null, new Pen(Brushes.White,3),
                                                new Rect(new Point(DepthJointPoints[MinDepthJointType].X - 10, DepthJointPoints[MinDepthJointType].Y - 10),
                                                         new Point(DepthJointPoints[MinDepthJointType].X + 10, DepthJointPoints[MinDepthJointType].Y + 10)));
                            */
                            // Draw the reference values
                            dc.DrawLine(new Pen(Brushes.Red, 2), new Point(0, RefForwardLine), new Point(this.kinectSensor.DepthFrameSource.FrameDescription.Width, RefForwardLine));
                            dc.DrawLine(new Pen(Brushes.Red, 2), new Point(0, RefCrouchLine), new Point(this.kinectSensor.DepthFrameSource.FrameDescription.Width, RefCrouchLine));
                            dc.DrawLine(new Pen(Brushes.Red, 2), new Point(0, RefJumpLine), new Point(this.kinectSensor.DepthFrameSource.FrameDescription.Width, RefJumpLine));
#endif
                            #endregion

                            float GameJudgmentDelta = CameraJointPoints[JointType.SpineBase].Position.Z - CameraJointPoints[MinDepthJointType].Position.Z;

                            //Console.WriteLine(GameJudgmentDelta);
                            if (GameJudgmentDelta > 0.4)
                            {
                                // Indicate we can start face track and audio track
                                OrderStart = true;

                                #region Use the Kinfe
                                if (ChangedKnife == true)
                                {
                                    if (CameraJointPoints[JointType.HandRight].Position.Y > CameraJointPoints[JointType.SpineMid].Position.Y
                                        && CameraJointPoints[JointType.HandRight].Position.X > CameraJointPoints[JointType.SpineMid].Position.X
                                        && CameraJointPoints[JointType.HandRight].Position.Z < CameraJointPoints[JointType.SpineBase].Position.Z - 0.4)
                                    {
                                        ChangedKnife = false;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (CameraJointPoints[JointType.HandTipRight].Position.Z > CameraJointPoints[JointType.HipLeft].Position.Z - 0.04
                                    && CameraJointPoints[JointType.HandTipRight].Position.Y < CameraJointPoints[JointType.SpineMid].Position.Y
                                    && CameraJointPoints[JointType.HandTipRight].Position.X < CameraJointPoints[JointType.SpineMid].Position.X)
                                {
                                    switch (GunType)
                                    {
                                        case 1:
                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q3);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q3);

                                            ChangedKnife = true;
                                            PreKnifeGunType = GunType;
                                            GunType = 3;
                                            return;
                                        case 2:
                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q3);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q3);

                                            ChangedKnife = true;
                                            PreKnifeGunType = GunType;
                                            GunType = 3;
                                            return;
                                        case 3:
                                            switch (PreKnifeGunType)
                                            {
                                                case 1:
                                                    HGX_KeyDown(HGX_Keybd_ScanCode.Q1);
                                                    HGX_KeyUp(HGX_Keybd_ScanCode.Q1);
                                                    break;
                                                case 2:
                                                    HGX_KeyDown(HGX_Keybd_ScanCode.Q2);
                                                    HGX_KeyUp(HGX_Keybd_ScanCode.Q2);
                                                    break;
                                                case 4:
                                                    HGX_KeyDown(HGX_Keybd_ScanCode.Q4);
                                                    HGX_KeyUp(HGX_Keybd_ScanCode.Q4);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            ChangedKnife = true;
                                            GunType = PreKnifeGunType;
                                            return;
                                        case 4:
                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q4);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q4);

                                            ChangedKnife = true;
                                            PreKnifeGunType = GunType;
                                            GunType = 3;
                                            return;
                                        default:
                                            return;
                                    }
                                }
                                #endregion

                                #region Use the only one bomb
                                if (FireTheBomb == true)
                                {
                                    if (CameraJointPoints[JointType.HandRight].Position.Y < CameraJointPoints[JointType.Neck].Position.Y)
                                    {
                                        FireTheBomb = false;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (CameraJointPoints[JointType.HandRight].Position.Y - CameraJointPoints[JointType.Head].Position.Y > 0.08)
                                {
                                    HGX_Mouse(HGX_MouseEventFlag.LeftDown, 0, 0);
                                    HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0);

                                    FireTheBomb = true;
                                    return;
                                }

                                if (ChangedBomb == true)
                                {
                                    if (CameraJointPoints[JointType.HandLeft].Position.Y > CameraJointPoints[JointType.HipRight].Position.Y
                                        && CameraJointPoints[JointType.HandLeft].Position.X < CameraJointPoints[JointType.HipRight].Position.X)
                                    {
                                        ChangedBomb = false;
                                        GunType = 1;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (CameraJointPoints[JointType.HandTipLeft].Position.Z > (CameraJointPoints[JointType.HipRight].Position.Z - 0.1)
                                    && CameraJointPoints[JointType.HandTipLeft].Position.Y < CameraJointPoints[JointType.SpineMid].Position.Y
                                    && CameraJointPoints[JointType.HandTipLeft].Position.X > CameraJointPoints[JointType.SpineMid].Position.X)
                                {
                                    switch (GunType)
                                    {
                                        case 1:
                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q4);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q4);

                                            ChangedBomb = true;
                                            PreBombGunType = GunType;
                                            GunType = 4;
                                            return;
                                        case 2:
                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q4);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q4);

                                            ChangedBomb = true;
                                            PreBombGunType = GunType;
                                            GunType = 4;
                                            return;
                                        case 3:
                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q4);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q4);

                                            ChangedBomb = true;
                                            PreBombGunType = GunType;
                                            GunType = 4;
                                            return;
                                        case 4:
                                            switch (PreBombGunType)
                                            {
                                                case 1:
                                                    HGX_KeyDown(HGX_Keybd_ScanCode.Q1);
                                                    HGX_KeyUp(HGX_Keybd_ScanCode.Q1);
                                                    break;
                                                case 2:
                                                    HGX_KeyDown(HGX_Keybd_ScanCode.Q2);
                                                    HGX_KeyUp(HGX_Keybd_ScanCode.Q2);
                                                    break;
                                                case 3:
                                                    HGX_KeyDown(HGX_Keybd_ScanCode.Q3);
                                                    HGX_KeyUp(HGX_Keybd_ScanCode.Q3);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            ChangedBomb = true;
                                            GunType = PreBombGunType;
                                            return;
                                        default:
                                            return;
                                    }
                                }
                                #endregion

                                #region Aim the target
                                if (GunType == 3)
                                {
                                    //Console.WriteLine(((Int32)DepthJointPoints[MinDepthJointType].X - preAimPoint_X) + "   " + ((Int32)DepthJointPoints[MinDepthJointType].X - preAimPoint_X));
                                    if (Math.Abs(((Int32)DepthJointPoints[MinDepthJointType].X - preAimPoint_X)) > 70
                                        || Math.Abs(((Int32)DepthJointPoints[MinDepthJointType].Y - preAimPoint_Y)) > 70)
                                    {
                                        HGX_Mouse(HGX_MouseEventFlag.LeftDown, 0, 0);
                                        HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0);
                                    }
                                }
                                if (((Int32)DepthJointPoints[MinDepthJointType].X - preAimPoint_X) < -2)
                                {
                                    HGX_Mouse(HGX_MouseEventFlag.Move, -30, 0);
                                }
                                else if (((Int32)DepthJointPoints[MinDepthJointType].X - preAimPoint_X) > 2)
                                {
                                    HGX_Mouse(HGX_MouseEventFlag.Move, 30, 0);
                                }

                                if (((Int32)DepthJointPoints[MinDepthJointType].Y - preAimPoint_Y) < -2)
                                {
                                    HGX_Mouse(HGX_MouseEventFlag.Move, 0, -10);
                                }
                                else if (((Int32)DepthJointPoints[MinDepthJointType].Y - preAimPoint_Y) > 2)
                                {
                                    HGX_Mouse(HGX_MouseEventFlag.Move, 0, 10);
                                }
                                preAimPoint_X = (Int32)DepthJointPoints[MinDepthJointType].X;
                                preAimPoint_Y = (Int32)DepthJointPoints[MinDepthJointType].Y;
                                #endregion

                                #region Change gun
                                if (ChangedGun == true)
                                {
                                    if (CameraJointPoints[JointType.HandLeft].Position.Y < CameraJointPoints[JointType.ShoulderRight].Position.Y
                                        && CameraJointPoints[JointType.HandLeft].Position.Z < CameraJointPoints[JointType.SpineBase].Position.Z - 0.4)
                                    {
                                        ChangedGun = false;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (CameraJointPoints[JointType.HandTipLeft].Position.Z > CameraJointPoints[JointType.ShoulderRight].Position.Z - 0.08
                                    && CameraJointPoints[JointType.HandTipLeft].Position.X > CameraJointPoints[JointType.Head].Position.X
                                    && CameraJointPoints[JointType.HandTipLeft].Position.Y > CameraJointPoints[JointType.ShoulderRight].Position.Y)
                                {
                                    switch (GunType)
                                    {
                                        case 1:
                                            GunType = 2;
                                            ChangedGun = true;

                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q2);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q2);
                                            return;
                                        case 2:
                                            GunType = 1;
                                            ChangedGun = true;

                                            HGX_KeyDown(HGX_Keybd_ScanCode.Q1);
                                            HGX_KeyUp(HGX_Keybd_ScanCode.Q1);
                                            return;
                                        default:
                                            return;
                                    }
                                }
                                #endregion

                                #region Change the bandolier
                                if (ChangedBullet == true)
                                {
                                    if (CameraJointPoints[JointType.HandLeft].Position.Z < CameraJointPoints[JointType.ShoulderRight].Position.Z - 0.1)
                                    {
                                        ChangedBullet = false;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (CameraJointPoints[JointType.HandLeft].Position.Z >= CameraJointPoints[JointType.HipLeft].Position.Z)
                                {
                                    if (ChangedBullet == false)
                                    {
                                        ChangedBullet = true;

                                        HGX_KeyDown(HGX_Keybd_ScanCode.R);
                                        HGX_KeyUp(HGX_Keybd_ScanCode.R);
                                        return;
                                    }
                                }
                                #endregion

                                #region Give the Crouch order
                                if (ChangedCrouch == true)
                                {
                                    if (DepthJointPoints[JointType.SpineBase].Y < 0.9 * RefCrouchLine)
                                    {
                                        ChangedCrouch = false;
                                        HGX_KeyUp(HGX_Keybd_ScanCode.L_Control);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (DepthJointPoints[JointType.SpineBase].Y > RefCrouchLine)
                                {
                                    if (ChangedCrouch == false)
                                    {
                                        ChangedCrouch = true;
                                        HGX_KeyDown(HGX_Keybd_ScanCode.L_Control);
                                        return;
                                    }
                                }
                                #endregion

                                #region Give the Jump order
                                if (ChangedJump == true)
                                {
                                    if (DepthJointPoints[JointType.SpineBase].Y > 1.1 * RefJumpLine)
                                    {
                                        ChangedJump = false;
                                        HGX_KeyUp(HGX_Keybd_ScanCode.Space);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (DepthJointPoints[JointType.SpineBase].Y < RefJumpLine)
                                {
                                    if (ChangedJump == false)
                                    {
                                        ChangedJump = true;
                                        HGX_KeyDown(HGX_Keybd_ScanCode.Space);
                                        return;
                                    }
                                }
                                #endregion

                                #region Give the Forward order
                                // Forward
                                if ((DepthJointPoints[JointType.AnkleLeft].Y < RefForwardLine
                                    || DepthJointPoints[JointType.AnkleRight].Y < RefForwardLine))
                                {
                                    if (ChangedForward == false)
                                    {
                                        ChangedForward = true;
                                        HGX_KeyDown(HGX_Keybd_ScanCode.W);
                                    }
                                }
                                else
                                {
                                    if (ChangedForward == true)
                                    {
                                        ChangedForward = false;
                                        HGX_KeyUp(HGX_Keybd_ScanCode.W);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                OrderStart = false;
                                RefreshFlags();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Refresh the flags we used
        /// </summary>
        private void RefreshFlags()
        {
            if (ChangedJump == true)
            {
                ChangedJump = false;

                HGX_KeyUp(HGX_Keybd_ScanCode.Space);
            }

            if (ChangedCrouch == true)
            {
                ChangedCrouch = false;

                HGX_KeyUp(HGX_Keybd_ScanCode.L_Control);
            }

            if (ChangedBullet == true)
            {
                ChangedBullet = false;

                HGX_KeyUp(HGX_Keybd_ScanCode.R);
            }

            if (ChangedForward == true)
            {
                ChangedForward = false;

                HGX_KeyUp(HGX_Keybd_ScanCode.W);
            }

            if (AudioShoot == true)
            {
                HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0);
                AudioShoot = false;
            }

            if (SeeLeft == true)
            {
                HGX_KeyUp(HGX_Keybd_ScanCode.J);
                SeeLeft = false;
            }
            if (SeeRight == true)
            {
                HGX_KeyUp(HGX_Keybd_ScanCode.K);
                SeeRight = false;
            }
            if (SeeUp == true)
            {
                HGX_KeyUp(HGX_Keybd_ScanCode.U);
                SeeUp = false;
            }
            if (SeeDown == true)
            {
                HGX_KeyUp(HGX_Keybd_ScanCode.I);
                SeeDown = false;
            }
        }
        #endregion

        #region Face tracking
        /// <summary>
        /// Requested face features
        /// </summary>
        private const FaceFrameFeatures faceFrameFeatures = FaceFrameFeatures.LeftEyeClosed
                                                            | FaceFrameFeatures.RightEyeClosed
                                                            | FaceFrameFeatures.RotationOrientation;
        /// <summary>
        /// Face Source
        /// </summary>
        private FaceFrameSource FaceFrameSource;

        /// <summary>
        /// Face Reader
        /// </summary>
        private FaceFrameReader FaceFrameReader;

        /// <summary>
        /// Face rotation display angle increment in degrees
        /// </summary>
        private const double FaceRotationIncrementInDegrees = 5.0;

        private bool SeeLeft = false;
        private bool SeeRight = false;
        private bool SeeUp = false;
        private bool SeeDown = false;

        private Int32 SeeLeftRef_Pitch = 25;
        private Int32 SeeRightRef_Pitch = -25;
        private Int32 SeeLeftRef_Yaw = 25;
        private Int32 SeeRightRef_Yaw = -25;

        private Int32 SeeUpRef = 30000;
        private Int32 SeeDownRef = -20000;

        private void FaceFrameReader_FrameArrivedEvent(object sender, FaceFrameArrivedEventArgs e)
        {
            if (OrderStart == true)
            {
                try
                { // Acquire the face frame
                    using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
                    {
                        if (faceFrame == null)
                            return;


                        // Retrieve the face frame result
                        FaceFrameResult frameResult = faceFrame.FaceFrameResult;

                        // extract face rotation in degrees as Euler angles
                        if (frameResult.FaceRotationQuaternion != null)
                        {
                            int pitch, yaw, roll;
                            ExtractFaceRotationInDegrees(frameResult.FaceRotationQuaternion, out pitch, out yaw, out roll);
                            //Console.WriteLine(pitch + "   " + yaw + "   " + roll);

#if false
                            #region See left and right -- Pitch
                        if (SeeLeft == true)
                        {
                            if (roll < SeeLeftRef - 5)
                            {
                                SeeLeft = false;
                                HGX_KeyUp(HGX_Keybd_ScanCode.J);
                            }
                            else
                            {
                                return;
                            }
                        }
                        if (roll > SeeLeftRef)
                        {
                            if (SeeLeft == false)
                            {
                                SeeLeft = true;

                                HGX_KeyDown(HGX_Keybd_ScanCode.J);

                                return;
                            }
                        }
                       
                        if (SeeRight == true)
                        {
                            if (roll > SeeRightRef + 5)
                            {
                                SeeRight = false;
                                HGX_KeyUp(HGX_Keybd_ScanCode.K);
                            }
                            else
                            {
                                return;
                            }
                        }
                        if (roll < SeeRightRef)
                        {
                            if (SeeRight == false)
                            {
                                SeeRight = true;

                                HGX_KeyDown(HGX_Keybd_ScanCode.K);

                                return;
                            }
                        }
                            #endregion
#else
                            #region See left and right -- Yaw
                            if (SeeLeft == true)
                            {
                                if (yaw < SeeLeftRef_Yaw - 5)
                                {
                                    SeeLeft = false;
                                    HGX_KeyUp(HGX_Keybd_ScanCode.J);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            if (yaw > SeeLeftRef_Yaw)
                            {
                                if (SeeLeft == false)
                                {
                                    SeeLeft = true;
                                    HGX_KeyDown(HGX_Keybd_ScanCode.J);
                                    return;
                                }
                            }

                            if (SeeRight == true)
                            {
                                if (yaw > SeeRightRef_Yaw + 5)
                                {
                                    SeeRight = false;
                                    HGX_KeyUp(HGX_Keybd_ScanCode.K);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            if (yaw < SeeRightRef_Yaw)
                            {
                                if (SeeRight == false)
                                {
                                    SeeRight = true;
                                    HGX_KeyDown(HGX_Keybd_ScanCode.K);
                                    return;
                                }
                            }
                            #endregion

#endif
                            #region See Up and Down
                            if (SeeUp == true)
                            {
                                if (pitch < SeeUpRef - 5)
                                {
                                    SeeUp = false;
                                    HGX_KeyUp(HGX_Keybd_ScanCode.U);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            if (pitch > SeeUpRef)
                            {
                                if (SeeUp == false)
                                {
                                    SeeUp = true;
                                    HGX_KeyDown(HGX_Keybd_ScanCode.U);
                                    return;
                                }
                            }

                            if (SeeDown == true)
                            {
                                if (pitch > SeeDownRef + 5)
                                {
                                    SeeDown = false;
                                    HGX_KeyUp(HGX_Keybd_ScanCode.I);
                                }
                                else
                                {
                                    return;
                                }
                            }
                            if (pitch < SeeDownRef)
                            {
                                if (SeeDown == false)
                                {
                                    SeeDown = true;
                                    HGX_KeyDown(HGX_Keybd_ScanCode.I);
                                    return;
                                }
                            }
                            #endregion
                        }

                    }
                }
                catch (Exception)
                {
                    //Ingnore
                }
            }
        }

        /// <summary>
        /// Converts rotation quaternion to Euler angles 
        /// And then maps them to a specified range of values to control the refresh rate
        /// </summary>
        /// <param name="rotQuaternion">face rotation quaternion</param>
        /// <param name="pitch">rotation about the X-axis</param>
        /// <param name="yaw">rotation about the Y-axis</param>
        /// <param name="roll">rotation about the Z-axis</param>
        private static void ExtractFaceRotationInDegrees(Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
        {
            double x = rotQuaternion.X;
            double y = rotQuaternion.Y;
            double z = rotQuaternion.Z;
            double w = rotQuaternion.W;

            // convert face rotation quaternion to Euler angles in degrees
            double yawD, pitchD, rollD;
            pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
            yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
            rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

            // clamp the values to a multiple of the specified increment to control the refresh rate
            double increment = FaceRotationIncrementInDegrees;
            pitch = (int)((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * (int)increment;
            yaw = (int)((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * (int)increment;
            roll = (int)((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * (int)increment;
        }
        #endregion

        #region Audio Event
        /// <summary>
        /// Sum of squares of audio samples being accumulated to compute the next energy value.
        /// </summary>
        private float accumulatedSquareSum;

        /// <summary>
        /// Number of audio samples accumulated so far to compute the next energy value.
        /// </summary>
        private int accumulatedSampleCount;

        /// <summary>
        /// Will be allocated a buffer to hold a single sub frame of audio data read from audio stream.
        /// </summary>
        private readonly byte[] AudioBuffer = null;

        /// <summary>
        /// Number of bytes in each Kinect audio stream sample (32-bit IEEE float).
        /// </summary>
        private const int BytesPerSample = sizeof(float);

        /// <summary>
        /// Number of audio samples represented by each column of pixels in wave bitmap.
        /// </summary>
        private const int SamplesPerColumn = 40;

        /// <summary>
        /// Minimum energy of audio to display (a negative number in dB value, where 0 dB is full scale)
        /// </summary>
        private const int MinEnergy = -90;

        /// <summary>
        /// AudioFrameReader for audio frames
        /// </summary>
        private AudioBeamFrameReader AudioFrameReader = null;

        private bool AudioShoot = false;

        /// <summary>
        /// Handles the audio frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void AudioReader_FrameArrivedEvent(object sender, AudioBeamFrameArrivedEventArgs e)
        {
            if (OrderStart == true)
            {
                AudioBeamFrameReference frameReference = e.FrameReference;

                try
                {
                    AudioBeamFrameList frameList = frameReference.AcquireBeamFrames();

                    if (frameList != null)
                    {
                        // AudioBeamFrameList is IDisposable
                        using (frameList)
                        {
                            // Only one audio beam is supported. Get the sub frame list for this beam
                            IReadOnlyList<AudioBeamSubFrame> subFrameList = frameList[0].SubFrames;

                            // Loop over all sub frames, extract audio buffer and beam information
                            foreach (AudioBeamSubFrame subFrame in subFrameList)
                            {
                                // Process audio buffer
                                subFrame.CopyFrameDataToArray(this.AudioBuffer);

                                for (int i = 0; i < this.AudioBuffer.Length; i += BytesPerSample)
                                {
                                    // Extract the 32-bit IEEE float sample from the byte array
                                    float audioSample = BitConverter.ToSingle(this.AudioBuffer, i);

                                    this.accumulatedSquareSum += audioSample * audioSample;
                                    ++this.accumulatedSampleCount;

                                    if (this.accumulatedSampleCount < SamplesPerColumn)
                                    {
                                        continue;
                                    }

                                    float meanSquare = this.accumulatedSquareSum / SamplesPerColumn;

                                    if (meanSquare > 1.0f)
                                    {
                                        // A loud audio source right next to the sensor may result in mean square values
                                        // greater than 1.0. Cap it at 1.0f for display purposes.
                                        meanSquare = 1.0f;
                                    }

                                    // Calculate energy in dB, in the range [MinEnergy, 0], where MinEnergy < 0
                                    float energy = MinEnergy;

                                    if (meanSquare > 0)
                                    {
                                        energy = (float)(10.0 * Math.Log10(meanSquare));
                                    }

                                    this.accumulatedSquareSum = 0;
                                    this.accumulatedSampleCount = 0;

                                    // Give the shoot order
                                    if (subFrame.BeamAngleConfidence == 1
                                        && subFrame.BeamAngle < 0.1
                                        && subFrame.BeamAngle > -0.1
                                        && (energy > -25 && energy < 10))
                                    {
                                        if (AudioShoot == false)
                                        {
                                            HGX_Mouse(HGX_MouseEventFlag.LeftDown, 0, 0);
                                            AudioShoot = true;
                                        }
                                    }
                                    else
                                    {
                                        if (AudioShoot == true)
                                        {
                                            HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0);
                                            AudioShoot = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore if the frame is no longer available
                }
            }
        }
        #endregion

        #region Input event funtions
        /// <summary>
        /// Import the legacy functions in user32.dll
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetWindow")]
        public static extern IntPtr GetWindow(IntPtr WindowHandler, uint wCmd);

        [DllImport("USER32.DLL", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandler);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetFocus")]
        public static extern IntPtr GetFocus();

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
        public static void HGX_KeyDown(HGX_Keybd_ScanCode p_ScanCode)
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
        public static void HGX_KeyUp(HGX_Keybd_ScanCode p_ScanCode)
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
        public static void HGX_Mouse(HGX_MouseEventFlag p_MouseEventFlag, Int32 p_dx, Int32 p_dy)
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
        #endregion
    }
}
