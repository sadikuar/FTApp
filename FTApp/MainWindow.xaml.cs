using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FTApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly VideoCapture? videoCapture;
        private bool startedCapture = false;
        private IDictionary<string, int> cells;

        private Mat frame;

        private ConvolutionKernelF kernel;
        private Image<Gray, byte> staticImage;

        private static readonly Regex regex = new Regex("[^0-9.-]+");

        public MainWindow()
        {
            frame = new Mat();
            InitializeComponent();
            videoCapture = new VideoCapture(0); // 0 = first device detected (in the case of a laptop, it is the integrated webcam)

            videoCapture.ImageGrabbed += VideoCapture_ImageGrabbed;

            cells = new Dictionary<string, int>();

            cells[cell1.Name] = 1;
            cells[cell2.Name] = 1;
            cells[cell3.Name] = 1;
            cells[cell4.Name] = 1;
            cells[cell5.Name] = 1;
            cells[cell6.Name] = 1;
            cells[cell7.Name] = 1;
            cells[cell8.Name] = 1;
            cells[cell9.Name] = 1;

            //kernel = new Matrix<float>(3, 3);
            kernel = new ConvolutionKernelF(3, 3);

            for (int i = 0; i < kernel.Rows; i++)
            {
                for (int j = 0; j < kernel.Cols; j++)
                {
                    kernel[i, j] = 1;
                }
            }
        }

        private void VideoCapture_ImageGrabbed(object? sender, EventArgs e)
        {
            videoCapture?.Retrieve(frame);

            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                VideoImage.Source = ConvertBitmap(ProcessImage(frame));
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

        private static Bitmap ProcessImage(Mat mat)
        {
            Image<Bgr, byte> img = mat.ToImage<Bgr, byte>();

            byte[,,] data = img.Data;

            for (int i = 0; i < img.Rows; i++)
            {
                for (int j = 0; j < img.Cols; j++)
                {
                    data[i, j, 2] = (byte)(2 * data[i, j, 2] % 255);
                }
            }

            img.Data = data;

            return img.Flip(FlipType.Horizontal).ToBitmap();
        }

        private void ApplyKernelToImage()
        {
            // TODO: Change this because it doesn't work great
            Image<Gray, float> appliedKernelImage = staticImage.Convolution(kernel, 0, BorderType.Reflect101);

            VideoImage.Source = ConvertBitmap(appliedKernelImage.ToBitmap());
        }

        private static bool isTextAllowed(string text)
        {
            return regex.IsMatch(text);
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

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "Images (*.png)|*.png|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {

                staticImage = new Image<Bgr, byte>(openFileDialog.FileName).Convert<Gray, byte>();

                videoCapture?.Stop();
                VideoCaptureButton.Content = "Start video";

                if (startedCapture == true)
                {
                    startedCapture = !startedCapture;
                }

                VideoImage.Source = ConvertBitmap(staticImage.ToBitmap());
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? textBox = sender as TextBox;

            if (textBox != null)
            {
                if(!string.IsNullOrEmpty(textBox.Text))
                {
                    if (int.TryParse(textBox.Text, out int value))
                    {
                        cells[textBox.Name] = value;
                    }
                }
            }
        }

        private void ApplyKernelButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyKernelToImage();
        }

        private void cell1_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = isTextAllowed(e.Text);
        }
    }
}
