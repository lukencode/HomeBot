using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Vision.Motion;
using System.IO;
using System.Windows;
using AForge.Imaging;

namespace KinectZor
{
    public class KinectZorSensor
    {
        private KinectSensor _sensor;
        public WriteableBitmap ColorBitmap { get; private set; }
        public byte[] ColorPixels { get; private set; }

        private MotionDetector _detector = new MotionDetector(new SimpleBackgroundModelingDetector());
        private bool _hasMotion;

        public EventHandler<MotionDetectedArgs> MotionDetected;
        public EventHandler<MotionDetectedArgs> MotionStopped;
        public EventHandler FrameUpdated;

        public KinectZorSensor()
        {

        }

        public void Start()
        {
            if (KinectSensor.KinectSensors.Any())
            {
                _sensor = KinectSensor.KinectSensors.First();

                if (_sensor.Status == KinectStatus.Connected)
                {
                    _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    ColorPixels = new byte[_sensor.ColorStream.FramePixelDataLength];
                    ColorBitmap = new WriteableBitmap(_sensor.ColorStream.FrameWidth, _sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                    _sensor.AllFramesReady += AllFramesReady;

                    _sensor.Start();
                }
            }
            else
            {
                throw new ApplicationException("No kinect detected");
            }
        }

        public void Stop()
        {
            if (_sensor != null)
            {
                _sensor.Stop();
                _sensor.AudioSource.Stop();
            }
        }

        private void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null)
                    return;

                colorFrame.CopyPixelDataTo(ColorPixels);

                ColorBitmap.WritePixels(
                    new Int32Rect(0, 0, ColorBitmap.PixelWidth, ColorBitmap.PixelHeight),
                    ColorPixels,
                    ColorBitmap.PixelWidth * sizeof(int),
                    0);

                DetectMotion();
            }

            if (FrameUpdated != null)
                FrameUpdated(this, new EventArgs());
        }

        private void DetectMotion()
        {
            var frame = new UnmanagedImage(ColorBitmap.BackBuffer, ColorBitmap.PixelWidth, ColorBitmap.PixelHeight, ColorBitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            if (_detector.ProcessFrame(frame) > 0.02)
            {
                _hasMotion = true;
                FireMotionDetected();
            }
            else
            {
                if (_hasMotion)
                    FireMotionStopped();

                _hasMotion = false;
            }
        }

        private void FireMotionDetected()
        {
            if (MotionDetected != null)
                MotionDetected(this, null);
        }

        private void FireMotionStopped()
        {
            if (MotionStopped != null)
                MotionStopped(this, null);
        }
    }
}
