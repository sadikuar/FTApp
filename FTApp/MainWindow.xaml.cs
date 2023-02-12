using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FTApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly VideoCapture? videoCapture;
        private Mat frame;
        private bool startedCapture = false;

        public MainWindow()
        {
            frame = new Mat();
            InitializeComponent();
            videoCapture = new VideoCapture(0); // 0 = first device detected (in the case of a laptop, it is the integrated webcam)

            videoCapture.ImageGrabbed += VideoCapture_ImageGrabbed;
        }

        private void VideoCapture_ImageGrabbed(object? sender, EventArgs e)
        {
            videoCapture?.Retrieve(frame);

            Dispatcher.BeginInvoke(new ThreadStart(delegate {
                VideoImage.Source = ConvertBitmap(ProcessFrame(frame));
            }));
        }


        // Source : https://stackoverflow.com/a/59753317
        /// <summary>
        /// Converts a Bitmap system image to a BitmapImage class.
        /// </summary>
        /// <param name="bitmap">The bitmap image</param>
        /// <returns></returns>
        private static BitmapImage ConvertBitmap(Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            memoryStream.Seek(0, SeekOrigin.Begin);
            image.StreamSource = memoryStream;
            image.EndInit();

            return image;
        }

        private static Bitmap ProcessFrame(Mat mat)
        {
            Image<Bgr, byte> img = mat.ToImage<Bgr, byte>();

            for (int i = 0; i < img.Data.GetLength(1); i++)
            {
                for (int j = 0; j < img.Data.GetLength(2); j++)
                {
                    img.Data[0, i, j] /= 4;
                }
            }

            return img.ToBitmap();
        }

        private void VideoCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (!startedCapture)
            {
                videoCapture?.Start();
                VideoCaptureButton.Content = "Stop video";
            }
            else
            {
                videoCapture?.Stop();
                VideoCaptureButton.Content = "Start video";
            }
            startedCapture = !startedCapture;
        }
    }
}
