using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AForge.Imaging;
using AForge.Vision.Motion;
using Microsoft.Kinect;

namespace KinectZor.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectZorSensor _sensor = new KinectZorSensor();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor.MotionDetected += OnMotion;
            _sensor.MotionStopped += MotionStopped;
            //_sensor.FrameUpdated += null;

            _sensor.Start();

            imageDisplay.Source = _sensor.ColorBitmap;
        }

        private void OnMotion(object sender, MotionDetectedArgs e)
        {
            textMotion.Text = "MOTION";
        }

        private void MotionStopped(object sender, MotionDetectedArgs e)
        {
            textMotion.Text = "-";
        }

        private void SaveScreenShot()
        {
            if (_sensor == null)
                return;

            var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), @"/kinect/");
            var time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, "KinectSnapshot-" + time + ".png");
            var fileStream = new FileStream(filePath, FileMode.Create);
            TakeScreenShot(fileStream);
        }

        private void TakeScreenShot(Stream stream)
        {
            if (_sensor == null)
                return;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(_sensor.ColorBitmap));

            try
            {
                using (stream)
                {
                    encoder.Save(stream);
                }

                Debug.WriteLine("Screenshot saved");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Screenshot failed: " + ex.Message);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _sensor.Stop();
        }

        private void btnScreenShot_Click(object sender, RoutedEventArgs e)
        {
            SaveScreenShot();
        }
    }
}
