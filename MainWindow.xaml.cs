using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectStreams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        Mode _mode = Mode.Color;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        bool _displayBody = false;
        bool isForwardGestureActive = false;
        bool isBackGestureActive = false;
        bool isZoomin = false;
        bool isZoomout = false;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Draw skeleton.
                                if (_displayBody)
                                {
                                    canvas.DrawSkeleton(body);
                                }
                                Joint head = body.Joints[JointType.Head];
                                Joint lefthand = body.Joints[JointType.HandLeft];
                                Joint righthand = body.Joints[JointType.HandRight];
                                Joint waist = body.Joints[JointType.SpineBase];

                                //Swiping right
                                if(righthand.Position.X > waist.Position.X + 0.6)
                                {
                                    if (!isForwardGestureActive)
                                    {
                                        System.Windows.Forms.SendKeys.SendWait("{Right}");
                                        isForwardGestureActive = true;
                                    }
                                }
                                else
                                {
                                    isForwardGestureActive = false;
                                }
                                //Swiping left
                                if (lefthand.Position.X < waist.Position.X - 0.6)
                                {
                                    if (!isBackGestureActive)
                                    {
                                        System.Windows.Forms.SendKeys.SendWait("{Left}");
                                        isBackGestureActive = true;
                                    }
                                }
                                else
                                {
                                    isBackGestureActive = false;
                                }

                                if (righthand.Position.Y > head.Position.Y - 0.5)
                                {
                                    if (!isZoomin)
                                    {
                                        System.Windows.Forms.SendKeys.SendWait("^{ADD}");
                                        isZoomin = true;
                                    }
                                }
                                else
                                {
                                    isZoomin = false;
                                }
                                if(lefthand.Position.Y > head.Position.Y - 0.5)
                                {
                                    System.Windows.Forms.SendKeys.SendWait("^{SUBTRACT}");
                                    isZoomout = true;
                                }
                                else
                                {
                                    isZoomout = false;
                                }
                            }
                        }
                    }
                }
            }
        }

       
        private void Color_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Infrared;
        }

        private void Body_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;
        }

        #endregion
    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }
}
