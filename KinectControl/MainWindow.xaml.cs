
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
    using System.Net;
    using System.Text;
    using System.Net.Sockets;

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
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            SocketInitialize();

            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // The depth display screen Width and Height
            depthSpaceDisplayWidth = this.kinectSensor.DepthFrameSource.FrameDescription.Width;
            depthSpaceDisplayHeight = this.kinectSensor.DepthFrameSource.FrameDescription.Height;

            // open the reader for the body and audio frames
            this.BodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            // initialize the components (controls) of the window
            this.drawingGroup = new DrawingGroup();
            this.imageSource = new DrawingImage(this.drawingGroup);
            this.DataContext = this;

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
            // open the sensor
            this.kinectSensor.Open();

            if (this.BodyFrameReader != null)
            {
                this.BodyFrameReader.FrameArrived += this.BodyReader_FrameArrivedEvent;
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.BodyFrameReader != null)
            {
                this.BodyFrameReader.Dispose();
                this.BodyFrameReader = null;
            }
            /*
                        for (int i = 0; i < FaceFrameSourceArray.Length; i++)
                        {
                            if (this.FaceFrameReaderArray[i] != null)
                            {
                                // FaceFrameReader is IDisposable
                                this.FaceFrameReaderArray[i].Dispose();
                                this.FaceFrameReaderArray[i] = null;
                            }

                            if (this.FaceFrameSourceArray[i] != null)
                            {
                                // FaceFrameSource is IDisposable
                                this.FaceFrameSourceArray[i].Dispose();
                                this.FaceFrameSourceArray[i] = null;
                            }
                        }
            */
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }
        #endregion

        #region Body tracking
        /// <summary>
        /// p_bodyOrderInfo.OrderStart is part of the GameStart
        /// </summary>
        private bool GameStart = false;
        private bool SendMessage = false;
        /// <summary>
        /// How many player we allow
        /// </summary>
        private const int PlayerNum = 2;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader BodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        private int depthSpaceDisplayWidth;
        private int depthSpaceDisplayHeight;

        private struct BodyOrderInfo
        {
            public int sendMessageIndex;

            public Brush jointsBrush;

            public bool orderStart;

            public bool getReference;
            public ulong currentTrackingBodyID;

            public float refCrouchLine;
            public float refForwardLine;
            public float refJumpLine;

            public bool changingKnife;
            public bool changingBullet;
            public bool changingCrouch;
            public bool changingJump;
            public bool changingForward;
            public bool changingGun;

            public bool changingBomb;
            public bool firingBomb;

            public Int32 gunType; // 1 means Long gun, 2 means handgun, 3 means knife, 4 means bomb;
            public Int32 preBombGunType;
            public Int32 preKnifeGunType;

            public DepthSpacePoint preAimPoint;

            public bool orderingShoot;

            public bool seeLeft;
            public bool seeRight;
        }
        private BodyOrderInfo[] bodyOrderInfoArray = new BodyOrderInfo[PlayerNum];

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void BodyReader_FrameArrivedEvent(object sender, BodyFrameArrivedEventArgs e)
        {
            if (GameStart == false)
            {
                for (int i = 0; i < bodyOrderInfoArray.Length; i++)
                {
                    if (bodyOrderInfoArray[i].orderStart == true)
                        RefreshFlags(ref bodyOrderInfoArray[i]);
                    bodyOrderInfoArray[i].getReference = false;
                }
                return;
            }

            #region Initialize or Refresh the bodies data
            bool t_bodyFrameProcessed = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];

                        bodyOrderInfoArray[0].jointsBrush = Brushes.White;
                        bodyOrderInfoArray[1].jointsBrush = Brushes.Red;

                        for (int i = 0; i < bodyOrderInfoArray.Length; i++)
                        {
                            bodyOrderInfoArray[i].gunType = 2;
                        }
                    }

                    /* The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                     * As long as those body objects are not disposed and not set to null in the array,
                     * those body objects will be re-used. */
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    t_bodyFrameProcessed = true;
                }
            }
            #endregion

            #region Process the bodies data
            if (t_bodyFrameProcessed == false)
            {
                return;
            }
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                if (SendMessage == false)
                {
                    //Draw a transparent background to set the render size                   
                    dc.DrawRectangle(Brushes.Black, null,
                                        new Rect(0.0, 0.0, depthSpaceDisplayWidth,
                                                           depthSpaceDisplayHeight));
                }
                foreach (Body body in this.bodies)
                {
                    if (body.IsTracked)
                    {
                        for (int i = 0; i < bodyOrderInfoArray.Length; i++)
                        {
                            if (bodyOrderInfoArray[i].getReference == true
                                && bodyOrderInfoArray[i].currentTrackingBodyID != body.TrackingId)
                            {
                                continue;
                            }

                            #region Map the cameraJointPoints to depthJointPoints and find the min-depth joint
                            IReadOnlyDictionary<JointType, Joint> cameraJointPoints = body.Joints;
                            Dictionary<JointType, DepthSpacePoint> depthJointPoints = new Dictionary<JointType, DepthSpacePoint>();

                            // This is our gun AIM point
                            JointType minDepthJointType = JointType.HandTipRight;

                            foreach (JointType jointType in cameraJointPoints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = cameraJointPoints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = 0.1f;
                                }

                                if (position.Z < cameraJointPoints[minDepthJointType].Position.Z)
                                {
                                    minDepthJointType = jointType;
                                }

                                depthJointPoints[jointType] = this.kinectSensor.CoordinateMapper.MapCameraPointToDepthSpace(position);
                            }
                            #endregion

                            #region Get the reference value
                            if (bodyOrderInfoArray[i].getReference == false)
                            {
                                if (body.HandLeftState == HandState.Lasso
                                    && body.HandLeftConfidence == TrackingConfidence.High
                                    && body.HandRightState == HandState.Lasso
                                    && body.HandRightConfidence == TrackingConfidence.High)
                                {
                                    bodyOrderInfoArray[i].refForwardLine = (float)0.2 * (depthJointPoints[JointType.KneeLeft].Y - depthJointPoints[JointType.AnkleLeft].Y)
                                                                            + depthJointPoints[JointType.AnkleLeft].Y;
                                    bodyOrderInfoArray[i].refCrouchLine = depthJointPoints[JointType.SpineBase].Y
                                                                            - (float)0.3 * (depthJointPoints[JointType.SpineBase].Y - depthJointPoints[JointType.AnkleLeft].Y);
                                    bodyOrderInfoArray[i].refJumpLine = depthJointPoints[JointType.SpineBase].Y
                                                                            + (float)0.2 * (depthJointPoints[JointType.SpineShoulder].Y - depthJointPoints[JointType.SpineBase].Y);

                                    bodyOrderInfoArray[i].currentTrackingBodyID = body.TrackingId;

                                    /*// Create new face sources with body tracking ID
                                    FaceFrameSourceArray[i] = new FaceFrameSource(kinectSensor, body.TrackingId, faceFrameFeatures);
                                    FaceFrameReaderArray[i] = FaceFrameSourceArray[i].OpenReader();

                                    // Wire faceframe events
                                    FaceFrameReaderArray[i].FrameArrived += FaceFrameReader_FrameArrivedEvent;*/

                                    bodyOrderInfoArray[i].getReference = true;
                                }

                                // We know that one body only belong to the ONE bodyOrderInfo
                                break;
                            }
                            #endregion

                            #region Draw the joints and reference values
                            // Draw the joints in Depth space
                            if (SendMessage == false)
                            {
                                foreach (JointType jointType in cameraJointPoints.Keys)
                                {
                                    if (cameraJointPoints[jointType].TrackingState == TrackingState.Tracked
                                        || cameraJointPoints[jointType].TrackingState == TrackingState.Inferred)
                                    {
                                        dc.DrawEllipse(bodyOrderInfoArray[i].jointsBrush, null,
                                                        new Point(depthJointPoints[jointType].X, depthJointPoints[jointType].Y), 3, 3);
                                    }
                                }
                                // Draw the reference values
                                if (depthJointPoints[JointType.Head].X < depthSpaceDisplayWidth / 2)
                                {
                                    dc.DrawLine(new Pen(bodyOrderInfoArray[i].jointsBrush, 2),
                                                new Point(0, bodyOrderInfoArray[i].refForwardLine),
                                                new Point(depthSpaceDisplayWidth / 2, bodyOrderInfoArray[i].refForwardLine));
                                    dc.DrawLine(new Pen(bodyOrderInfoArray[i].jointsBrush, 2),
                                                new Point(0, bodyOrderInfoArray[i].refCrouchLine),
                                                new Point(depthSpaceDisplayWidth / 2, bodyOrderInfoArray[i].refCrouchLine));
                                    dc.DrawLine(new Pen(bodyOrderInfoArray[i].jointsBrush, 2),
                                                new Point(0, bodyOrderInfoArray[i].refJumpLine),
                                                new Point(depthSpaceDisplayWidth / 2, bodyOrderInfoArray[i].refJumpLine));
                                    bodyOrderInfoArray[i].sendMessageIndex = 2;
                                }
                                else
                                {
                                    dc.DrawLine(new Pen(bodyOrderInfoArray[i].jointsBrush, 2),
                                                    new Point(depthSpaceDisplayWidth / 2, bodyOrderInfoArray[i].refForwardLine),
                                                    new Point(depthSpaceDisplayWidth, bodyOrderInfoArray[i].refForwardLine));
                                    dc.DrawLine(new Pen(bodyOrderInfoArray[i].jointsBrush, 2),
                                                    new Point(depthSpaceDisplayWidth / 2, bodyOrderInfoArray[i].refCrouchLine),
                                                    new Point(depthSpaceDisplayWidth, bodyOrderInfoArray[i].refCrouchLine));
                                    dc.DrawLine(new Pen(bodyOrderInfoArray[i].jointsBrush, 2),
                                                    new Point(depthSpaceDisplayWidth / 2, bodyOrderInfoArray[i].refJumpLine),
                                                    new Point(depthSpaceDisplayWidth, bodyOrderInfoArray[i].refJumpLine));
                                    bodyOrderInfoArray[i].sendMessageIndex = 3;
                                }
                            }
                            #endregion

                            float t_gameJudgmentDelta = cameraJointPoints[JointType.SpineBase].Position.Z - cameraJointPoints[minDepthJointType].Position.Z;

                            //Console.WriteLine(t_gameJudgmentDelta);
                            if (t_gameJudgmentDelta > 0.4)
                            {
                                GetBodyOrder(ref bodyOrderInfoArray[i], cameraJointPoints, depthJointPoints, minDepthJointType);
                            }
                            else
                            {
                                if (bodyOrderInfoArray[i].orderStart == true)
                                {
                                    RefreshFlags(ref bodyOrderInfoArray[i]);
                                }
                            }

                            // We know that one body only belong to the ONE bodyOrderInfo
                            break;
                        }
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Get our body order
        /// </summary>
        /// <param name="p_bodyOrderInfo"></param>
        /// <param name="p_cameraJointPoints"></param>
        /// <param name="p_depthJointPoints"></param>
        /// <param name="p_minDepthJointType"></param>
        private void GetBodyOrder(ref BodyOrderInfo p_bodyOrderInfo, IReadOnlyDictionary<JointType, Joint> p_cameraJointPoints, Dictionary<JointType, DepthSpacePoint> p_depthJointPoints, JointType p_minDepthJointType)
        {
            // Indicate we can start face track and audio track
            p_bodyOrderInfo.orderStart = true;

            #region Knife Order
            if (p_bodyOrderInfo.changingKnife == true)
            {
                if (p_cameraJointPoints[JointType.HandRight].Position.Z < p_cameraJointPoints[JointType.SpineBase].Position.Z - 0.4
                    && p_cameraJointPoints[JointType.HandRight].Position.Y > p_cameraJointPoints[JointType.SpineBase].Position.Y)
                {
                    p_bodyOrderInfo.changingKnife = false;
                }
                else
                {
                    return;
                }
            }
            if (p_cameraJointPoints[JointType.HandTipRight].Position.Z > p_cameraJointPoints[JointType.HipLeft].Position.Z - 0.04
                && p_cameraJointPoints[JointType.HandTipRight].Position.Y < p_cameraJointPoints[JointType.SpineMid].Position.Y
                && p_cameraJointPoints[JointType.HandTipRight].Position.X < p_cameraJointPoints[JointType.SpineMid].Position.X)
            {
                switch (p_bodyOrderInfo.gunType)
                {
                    case 1:
                        HGX_KeyDown(HGX_Keybd_ScanCode.Q3,p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q3, p_bodyOrderInfo.sendMessageIndex);

                        p_bodyOrderInfo.preKnifeGunType = p_bodyOrderInfo.gunType;
                        p_bodyOrderInfo.gunType = 3;
                        break;
                    case 2:
                        HGX_KeyDown(HGX_Keybd_ScanCode.Q3, p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q3, p_bodyOrderInfo.sendMessageIndex);

                        p_bodyOrderInfo.preKnifeGunType = p_bodyOrderInfo.gunType;
                        p_bodyOrderInfo.gunType = 3;
                        break;
                    case 3:
                        switch (p_bodyOrderInfo.preKnifeGunType)
                        {
                            case 1:
                                HGX_KeyDown(HGX_Keybd_ScanCode.Q1, p_bodyOrderInfo.sendMessageIndex);
                                HGX_KeyUp(HGX_Keybd_ScanCode.Q1, p_bodyOrderInfo.sendMessageIndex);
                                break;
                            case 2:
                                HGX_KeyDown(HGX_Keybd_ScanCode.Q2, p_bodyOrderInfo.sendMessageIndex);
                                HGX_KeyUp(HGX_Keybd_ScanCode.Q2, p_bodyOrderInfo.sendMessageIndex);
                                break;
                            case 4:
                                HGX_KeyDown(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);
                                HGX_KeyUp(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);
                                break;
                            default:
                                break;
                        }
                        p_bodyOrderInfo.gunType = p_bodyOrderInfo.preKnifeGunType;
                        break;
                    case 4:
                        HGX_KeyDown(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);

                        p_bodyOrderInfo.preKnifeGunType = p_bodyOrderInfo.gunType;
                        p_bodyOrderInfo.gunType = 3;
                        break;
                    default:
                        break;
                }
                p_bodyOrderInfo.changingKnife = true;
                return;
            }
            #endregion

            #region The only one bomb order
            if (p_bodyOrderInfo.firingBomb == true)
            {
                if (p_cameraJointPoints[JointType.HandRight].Position.Y < p_cameraJointPoints[JointType.Neck].Position.Y)
                {
                    p_bodyOrderInfo.firingBomb = false;
                }
                else
                {
                    return;
                }
            }
            if (p_bodyOrderInfo.gunType == 4
                && p_cameraJointPoints[JointType.HandRight].Position.Y - p_cameraJointPoints[JointType.Head].Position.Y > 0.05)
            {
                HGX_Mouse(HGX_MouseEventFlag.LeftDown, 0, 0, p_bodyOrderInfo.sendMessageIndex);
                HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0, p_bodyOrderInfo.sendMessageIndex);

                p_bodyOrderInfo.firingBomb = true;
                return;
            }

            if (p_bodyOrderInfo.changingBomb == true)
            {
                if (p_cameraJointPoints[JointType.HandLeft].Position.Z < p_cameraJointPoints[JointType.SpineBase].Position.Z - 0.4)
                {
                    p_bodyOrderInfo.changingBomb = false;
                }
                else
                {
                    return;
                }
            }
            if (p_cameraJointPoints[JointType.HandTipLeft].Position.Z > p_cameraJointPoints[JointType.HipRight].Position.Z - 0.04
                && p_cameraJointPoints[JointType.HandTipLeft].Position.Y < p_cameraJointPoints[JointType.SpineMid].Position.Y
                && p_cameraJointPoints[JointType.HandTipLeft].Position.X > p_cameraJointPoints[JointType.SpineMid].Position.X)
            {
                switch (p_bodyOrderInfo.gunType)
                {
                    case 1:
                        HGX_KeyDown(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);

                        p_bodyOrderInfo.preBombGunType = p_bodyOrderInfo.gunType;
                        p_bodyOrderInfo.gunType = 4;
                        break;
                    case 2:
                        HGX_KeyDown(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);

                        p_bodyOrderInfo.preBombGunType = p_bodyOrderInfo.gunType;
                        p_bodyOrderInfo.gunType = 4;
                        break;
                    case 3:
                        HGX_KeyDown(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q4, p_bodyOrderInfo.sendMessageIndex);

                        p_bodyOrderInfo.preBombGunType = p_bodyOrderInfo.gunType;
                        p_bodyOrderInfo.gunType = 4;
                        break;
                    case 4:
                        switch (p_bodyOrderInfo.preBombGunType)
                        {
                            case 1:
                                HGX_KeyDown(HGX_Keybd_ScanCode.Q1, p_bodyOrderInfo.sendMessageIndex);
                                HGX_KeyUp(HGX_Keybd_ScanCode.Q1, p_bodyOrderInfo.sendMessageIndex);
                                break;
                            case 2:
                                HGX_KeyDown(HGX_Keybd_ScanCode.Q2, p_bodyOrderInfo.sendMessageIndex);
                                HGX_KeyUp(HGX_Keybd_ScanCode.Q2, p_bodyOrderInfo.sendMessageIndex);
                                break;
                            case 3:
                                HGX_KeyDown(HGX_Keybd_ScanCode.Q3, p_bodyOrderInfo.sendMessageIndex);
                                HGX_KeyUp(HGX_Keybd_ScanCode.Q3, p_bodyOrderInfo.sendMessageIndex);
                                break;
                            default:
                                break;
                        }
                        p_bodyOrderInfo.gunType = p_bodyOrderInfo.preBombGunType;
                        break;
                    default:
                        break;
                }
                p_bodyOrderInfo.changingBomb = true;
                return;
            }
            #endregion

            #region Aim the target
            /*if (p_bodyOrderInfo.gunType == 3)
            {
                //Console.WriteLine(((Int32)p_depthJointPoints[p_minDepthJointType].X - p_bodyOrderInfo.preAimPoint_X) + "   " + ((Int32)p_depthJointPoints[p_minDepthJointType].X - p_bodyOrderInfo.preAimPoint_X));
                if (Math.Abs(((Int32)p_depthJointPoints[p_minDepthJointType].X - p_bodyOrderInfo.preAimPoint_X)) > 70
                    || Math.Abs(((Int32)p_depthJointPoints[p_minDepthJointType].Y - p_bodyOrderInfo.preAimPoint_Y)) > 70)
                {
                    HGX_Mouse(HGX_MouseEventFlag.LeftDown, 0, 0);
                    HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0);
                }
            }*/
            if (p_depthJointPoints[p_minDepthJointType].X - p_bodyOrderInfo.preAimPoint.X < -2)
            {
                HGX_Mouse(HGX_MouseEventFlag.Move, -30, 0, p_bodyOrderInfo.sendMessageIndex);
            }
            else if (p_depthJointPoints[p_minDepthJointType].X - p_bodyOrderInfo.preAimPoint.X > 2)
            {
                HGX_Mouse(HGX_MouseEventFlag.Move, 30, 0, p_bodyOrderInfo.sendMessageIndex);
            }

            if (p_depthJointPoints[p_minDepthJointType].Y - p_bodyOrderInfo.preAimPoint.Y < -2)
            {
                HGX_Mouse(HGX_MouseEventFlag.Move, 0, -20, p_bodyOrderInfo.sendMessageIndex);
            }
            else if (p_depthJointPoints[p_minDepthJointType].Y - p_bodyOrderInfo.preAimPoint.Y > 2)
            {
                HGX_Mouse(HGX_MouseEventFlag.Move, 0, 20, p_bodyOrderInfo.sendMessageIndex);
            }
            p_bodyOrderInfo.preAimPoint = p_depthJointPoints[p_minDepthJointType];
            #endregion

            #region Change gun
            if (p_bodyOrderInfo.changingGun == true)
            {
                if (p_cameraJointPoints[JointType.HandLeft].Position.Y < p_cameraJointPoints[JointType.ShoulderRight].Position.Y
                    && p_cameraJointPoints[JointType.HandLeft].Position.Z < p_cameraJointPoints[JointType.SpineBase].Position.Z - 0.4)
                {
                    p_bodyOrderInfo.changingGun = false;
                }
                else
                {
                    return;
                }
            }
            if (p_cameraJointPoints[JointType.HandTipLeft].Position.Z > p_cameraJointPoints[JointType.ShoulderRight].Position.Z - 0.08
                && p_cameraJointPoints[JointType.HandTipLeft].Position.X > p_cameraJointPoints[JointType.Head].Position.X
                && p_cameraJointPoints[JointType.HandTipLeft].Position.Y > p_cameraJointPoints[JointType.ShoulderRight].Position.Y)
            {
                switch (p_bodyOrderInfo.gunType)
                {
                    case 1:
                        p_bodyOrderInfo.gunType = 2;

                        HGX_KeyDown(HGX_Keybd_ScanCode.Q2, p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q2, p_bodyOrderInfo.sendMessageIndex);
                        break;
                    case 2:
                        p_bodyOrderInfo.gunType = 1;

                        HGX_KeyDown(HGX_Keybd_ScanCode.Q1, p_bodyOrderInfo.sendMessageIndex);
                        HGX_KeyUp(HGX_Keybd_ScanCode.Q1, p_bodyOrderInfo.sendMessageIndex);
                        break;
                    default:
                        break;
                }
                p_bodyOrderInfo.changingGun = true;
                return;
            }
            #endregion

            #region Change the bandolier
            if (p_bodyOrderInfo.changingBullet == true)
            {
                if (p_cameraJointPoints[JointType.HandLeft].Position.Z < p_cameraJointPoints[JointType.ShoulderRight].Position.Z - 0.1)
                {
                    p_bodyOrderInfo.changingBullet = false;
                }
                else
                {
                    return;
                }
            }
            if (p_cameraJointPoints[JointType.HandLeft].Position.Z >= p_cameraJointPoints[JointType.HipLeft].Position.Z)
            {
                if (p_bodyOrderInfo.changingBullet == false)
                {
                    p_bodyOrderInfo.changingBullet = true;

                    HGX_KeyDown(HGX_Keybd_ScanCode.R,p_bodyOrderInfo.sendMessageIndex);
                    HGX_KeyUp(HGX_Keybd_ScanCode.R,p_bodyOrderInfo.sendMessageIndex);
                    return;
                }
            }
            #endregion

            #region Give the Crouch order
            if (p_bodyOrderInfo.changingCrouch == true)
            {
                if (p_depthJointPoints[JointType.SpineBase].Y < 0.9 * p_bodyOrderInfo.refCrouchLine)
                {
                    p_bodyOrderInfo.changingCrouch = false;
                    HGX_KeyUp(HGX_Keybd_ScanCode.L_Control,p_bodyOrderInfo.sendMessageIndex);
                }
                else
                {
                    return;
                }
            }
            if (p_depthJointPoints[JointType.SpineBase].Y > p_bodyOrderInfo.refCrouchLine)
            {
                if (p_bodyOrderInfo.changingCrouch == false)
                {
                    p_bodyOrderInfo.changingCrouch = true;
                    HGX_KeyDown(HGX_Keybd_ScanCode.L_Control,p_bodyOrderInfo.sendMessageIndex);
                    return;
                }
            }
            #endregion

            #region Give the Jump order
            if (p_bodyOrderInfo.changingJump == true)
            {
                if (p_depthJointPoints[JointType.SpineBase].Y > 1.1 * p_bodyOrderInfo.refJumpLine)
                {
                    p_bodyOrderInfo.changingJump = false;
                    HGX_KeyUp(HGX_Keybd_ScanCode.Space,p_bodyOrderInfo.sendMessageIndex);
                }
                else
                {
                    return;
                }
            }
            if (p_depthJointPoints[JointType.SpineBase].Y < p_bodyOrderInfo.refJumpLine)
            {
                if (p_bodyOrderInfo.changingJump == false)
                {
                    p_bodyOrderInfo.changingJump = true;
                    HGX_KeyDown(HGX_Keybd_ScanCode.Space,p_bodyOrderInfo.sendMessageIndex);
                    return;
                }
            }
            #endregion

            #region Give the Forward order
            // Forward
            if (p_bodyOrderInfo.changingForward == true)
            {
                if (p_depthJointPoints[JointType.AnkleLeft].Y > p_bodyOrderInfo.refForwardLine
                    && p_depthJointPoints[JointType.AnkleRight].Y > p_bodyOrderInfo.refForwardLine)
                {
                    p_bodyOrderInfo.changingForward = false;
                    HGX_KeyUp(HGX_Keybd_ScanCode.W,p_bodyOrderInfo.sendMessageIndex);
                }
                else
                {
                    return;
                }
            }
            if (p_depthJointPoints[JointType.AnkleLeft].Y < p_bodyOrderInfo.refForwardLine
                || p_depthJointPoints[JointType.AnkleRight].Y < p_bodyOrderInfo.refForwardLine)
            {
                if (p_bodyOrderInfo.changingForward == false)
                {
                    p_bodyOrderInfo.changingForward = true;
                    HGX_KeyDown(HGX_Keybd_ScanCode.W,p_bodyOrderInfo.sendMessageIndex);
                }
            }
            #endregion
        }

        /// <summary>
        /// Refresh the flags we used
        /// </summary>
        /// <param name="p_bodyOrderInfo"></param>
        private void RefreshFlags(ref BodyOrderInfo p_bodyOrderInfo)
        {
            p_bodyOrderInfo.orderStart = false;

            if (p_bodyOrderInfo.changingJump == true)
            {
                p_bodyOrderInfo.changingJump = false;
                HGX_KeyUp(HGX_Keybd_ScanCode.Space,p_bodyOrderInfo.sendMessageIndex);
            }

            if (p_bodyOrderInfo.changingCrouch == true)
            {
                p_bodyOrderInfo.changingCrouch = false;
                HGX_KeyUp(HGX_Keybd_ScanCode.L_Control,p_bodyOrderInfo.sendMessageIndex);
            }

            if (p_bodyOrderInfo.changingBullet == true)
            {
                p_bodyOrderInfo.changingBullet = false;
                HGX_KeyUp(HGX_Keybd_ScanCode.R,p_bodyOrderInfo.sendMessageIndex);
            }

            if (p_bodyOrderInfo.changingForward == true)
            {
                p_bodyOrderInfo.changingForward = false;
                HGX_KeyUp(HGX_Keybd_ScanCode.W,p_bodyOrderInfo.sendMessageIndex);
            }

            if (p_bodyOrderInfo.orderingShoot == true)
            {
                p_bodyOrderInfo.orderingShoot = false;
                HGX_Mouse(HGX_MouseEventFlag.LeftUp, 0, 0,p_bodyOrderInfo.sendMessageIndex);
            }

            if (p_bodyOrderInfo.seeLeft == true)
            {
                HGX_KeyUp(HGX_Keybd_ScanCode.J,p_bodyOrderInfo.sendMessageIndex);
                p_bodyOrderInfo.seeLeft = false;
            }
            if (p_bodyOrderInfo.seeRight == true)
            {
                HGX_KeyUp(HGX_Keybd_ScanCode.K,p_bodyOrderInfo.sendMessageIndex);
                p_bodyOrderInfo.seeRight = false;
            }
        }
        #endregion

        #region Face tracking
#if false
        /// <summary>
        /// Requested face features
        /// </summary>
        private const FaceFrameFeatures faceFrameFeatures = FaceFrameFeatures.RotationOrientation
                                                            | FaceFrameFeatures.MouthOpen
                                                            | FaceFrameFeatures.LeftEyeClosed
                                                            | FaceFrameFeatures.RightEyeClosed;
        /// <summary>
        /// Face Source and Reader
        /// </summary>
        private FaceFrameSource[] FaceFrameSourceArray = new FaceFrameSource[PlayerNum];
        private FaceFrameReader[] FaceFrameReaderArray = new FaceFrameReader[PlayerNum];

        /// <summary>
        /// Face rotation display angle increment in degrees
        /// </summary>
        private const double FaceRotationIncrementInDegrees = 5.0;

        private Int32 SeeLeftRef_Yaw = 1000;
        private Int32 SeeRightRef_Yaw = -1000;

        private void FaceFrameReader_FrameArrivedEvent(object sender, FaceFrameArrivedEventArgs e)
        {
            try
            {   // Acquire the face frame
                using (FaceFrame faceFrame = e.FrameReference.AcquireFrame())
                {
                    if (faceFrame == null)
                        return;

                    for (int i = 0; i < bodyOrderInfoArray.Length; i++)
                    {
                        if (bodyOrderInfoArray[i].currentTrackingBodyID == faceFrame.FaceFrameSource.TrackingId)
                        {
                            if (bodyOrderInfoArray[i].orderStart == false)
                            {
                                break;
                            }

                            int pitch = 0, yaw = 0, roll = 0;

                            // extract face rotation in degrees as Euler angles
                            if (faceFrame.FaceFrameResult.FaceRotationQuaternion != null)
                            {
                                ExtractFaceRotationInDegrees(faceFrame.FaceFrameResult.FaceRotationQuaternion, out pitch, out yaw, out roll);
                                //Console.WriteLine(pitch + "   " + yaw + "   " + roll);
                                GetFaceOrder(ref bodyOrderInfoArray[i], pitch, yaw, roll);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Ingnore
                return;
            }
        }

        private void GetFaceOrder(ref BodyOrderInfo p_bodyOrderInfo, int p_pitch, int p_yaw, int p_roll)
        {
        #region See left and right -- Yaw
            if (p_bodyOrderInfo.seeLeft == true)
            {
                if (p_yaw < SeeLeftRef_Yaw - 5)
                {
                    p_bodyOrderInfo.seeLeft = false;
                    HGX_KeyUp(HGX_Keybd_ScanCode.J);
                }
                else
                {
                    return;
                }
            }
            if (p_yaw > SeeLeftRef_Yaw)
            {
                if (p_bodyOrderInfo.seeLeft == false)
                {
                    p_bodyOrderInfo.seeLeft = true;
                    HGX_KeyDown(HGX_Keybd_ScanCode.J);
                    return;
                }
            }

            if (p_bodyOrderInfo.seeRight == true)
            {
                if (p_yaw > SeeRightRef_Yaw + 5)
                {
                    p_bodyOrderInfo.seeRight = false;
                    HGX_KeyUp(HGX_Keybd_ScanCode.K);
                }
                else
                {
                    return;
                }
            }
            if (p_yaw < SeeRightRef_Yaw)
            {
                if (p_bodyOrderInfo.seeRight == false)
                {
                    p_bodyOrderInfo.seeRight = true;
                    HGX_KeyDown(HGX_Keybd_ScanCode.K);
                    return;
                }
            }
        #endregion
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
#endif
        #endregion

        #region Input event funtions
        /// <summary>
        /// Import the legacy functions in user32.dll
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetWindow")]
        public static extern IntPtr GetWindow(IntPtr WindowHandler, int wCmd);

        [DllImport("USER32.DLL", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandler);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetFocus")]
        public static extern IntPtr GetFocus();

        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr WindowHandler);

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
        public void HGX_KeyDown(HGX_Keybd_ScanCode p_ScanCode, int p_sendMessageIndex)
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
            else
            {
                ServerSendControl(ServerControlType.keybdDown, (UInt16)p_ScanCode, 0, p_sendMessageIndex);
            }
        }
        public void HGX_KeyUp(HGX_Keybd_ScanCode p_ScanCode, int p_sendMessageIndex)
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
            else
            {
                ServerSendControl(ServerControlType.keybdUp, (UInt16)p_ScanCode, 0, p_sendMessageIndex);
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
        public void HGX_Mouse(HGX_MouseEventFlag p_MouseEventFlag, Int32 p_dx, Int32 p_dy, int p_sendMessageIndex)
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
            else
            {
                switch (p_MouseEventFlag)
                {
                    case HGX_MouseEventFlag.Absolute:
                        break;
                    case HGX_MouseEventFlag.MouserEvent_Hwheel:
                        break;
                    case HGX_MouseEventFlag.Move:
                        if(p_dx < 0)
                            ServerSendControl(ServerControlType.mouseMoveLeft, 0, 0, p_sendMessageIndex);
                        else
                            ServerSendControl(ServerControlType.mouseMoveRight, 0, 0, p_sendMessageIndex);
                        if(p_dy < 0)
                            ServerSendControl(ServerControlType.mouseMoveUp, 0, 0, p_sendMessageIndex);
                        else
                            ServerSendControl(ServerControlType.mouseMoveDown, 0, 0, p_sendMessageIndex);
                        break;
                    case HGX_MouseEventFlag.Move_noCoalesce:
                        break;
                    case HGX_MouseEventFlag.LeftDown:
                        ServerSendControl(ServerControlType.mouseLeftDown, 0, 0, p_sendMessageIndex);
                        break;
                    case HGX_MouseEventFlag.LeftUp:
                        ServerSendControl(ServerControlType.mouseLeftUp, 0, 0, p_sendMessageIndex);
                        break;
                    case HGX_MouseEventFlag.MiddleDown:
                        break;
                    case HGX_MouseEventFlag.MiddleUp:
                        break;
                    case HGX_MouseEventFlag.RightDown:
                        
                        break;
                    case HGX_MouseEventFlag.RightUp:
                        
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
            }
        }
        #endregion

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.ToString() == "Stop")
            {
                GameStart = false;
                btnStart.Content = "Start";
            }
            else
            {
                GameStart = true;
                btnStart.Content = "Stop";
            }
        }

        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (btnSendMessage.Content.ToString() == "Stop")
            {
                SendMessage = false;
                btnSendMessage.Content = "Start";
            }
            else
            {
                SendMessage = true;
                btnSendMessage.Content = "Stop";
            }
        }


        //创建一个监听客户端的套接字
        Socket SocketWatch = null;

        //用于保存所有通信客户端的Socket
        Dictionary<int, Socket> dictionaryConnectingSocket = new Dictionary<int, Socket>();

        Socket socketConnecting = null;
        string clientName = null;
        IPAddress clientIP;
        int clientPort;

        private void SocketInitialize()
        {
            //定义一个套接字用于监听客户端发来的信息  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            SocketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //获取服务端IPv4地址
            IPAddress ipAddress = GetLocalIPv4Address();

            //给服务端赋予一个端口号
            int port = 8888;

            Console.WriteLine("IP: " + ipAddress + " Port: " + port);

            //将IP地址和端口号绑定到网络节点endpoint上 
            IPEndPoint endpoint = new IPEndPoint(ipAddress, port);
            //将负责监听的套接字绑定网络端点
            SocketWatch.Bind(endpoint);
            //将套接字的监听队列长度设置为20
            SocketWatch.Listen(20);

            // Wait for getting all the player
            while (dictionaryConnectingSocket.Count < 1)
            {
                try
                {
                    socketConnecting = SocketWatch.Accept();
                }
                catch (Exception)
                {
                    continue;
                }

                clientIP = (socketConnecting.RemoteEndPoint as IPEndPoint).Address;
                clientPort = (socketConnecting.RemoteEndPoint as IPEndPoint).Port;
                //创建访问客户端的唯一标识 由IP和端口号组成 
                clientName = " IP: " + clientIP + " Port: " + clientPort;

                int t_sendMessageIndex = 0;
                switch (clientIP.ToString())
                {
                    case "10.71.44.4":
                        t_sendMessageIndex = 2;
                        break;
                    case "10.71.44.21":
                        t_sendMessageIndex = 3;
                        break;
                    default:
                        break;
                }

                if (t_sendMessageIndex != 0)
                {
                    Console.WriteLine(clientName + " Index: " + t_sendMessageIndex);
                    dictionaryConnectingSocket.Add(t_sendMessageIndex, socketConnecting); //将客户端名字和套接字添加到添加到数据字典中
                }
            }
        }

        /// <summary>
        /// 获取本地IPv4地址
        /// </summary>
        /// <returns>本地IPv4地址</returns>
        private IPAddress GetLocalIPv4Address()
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

        /// <summary>
        /// 发送控制信息到子端的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        public void ServerSendControl(ServerControlType p_controlType, UInt16 p_parameter1, UInt16 p_parameter2, int p_sendIndex)
        {
            byte[] sendByte = BitConverter.GetBytes(0x55000000000066);

            sendByte[1] = (byte)p_controlType;

            sendByte[2] = (byte)p_parameter1;
            sendByte[3] = (byte)(p_parameter1 >> 8);

            sendByte[4] = (byte)p_parameter2;
            sendByte[5] = (byte)(p_parameter2 >> 8);

            //获得相应的套接字 并将字节数组信息发送出去
            dictionaryConnectingSocket[p_sendIndex].Send(sendByte);
        }
    }
}
