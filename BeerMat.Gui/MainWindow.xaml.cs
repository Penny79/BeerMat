using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;
using BeerMat.Core;
using BeerMat.Core.Model;
using Emgu.CV;
using Emgu.CV.Structure;


namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly System.Timers.Timer timer;
        private readonly SynchronizationContext sychronizationContext = SynchronizationContext.Current;
        private readonly Capture camera;
        private Image<Bgr, byte> sampleImage;

        public MainWindow()
        {
            InitializeComponent();

            //TestMarker();           
            camera = new Capture(0);
            timer = new System.Timers.Timer(100); // Timer anlegen
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            foreach (var step in Enum.GetNames(typeof (FilterType)))
            {
                cboSteps.Items.Add(step);
            }

            sampleImage = new Image<Bgr, byte>(@"D:\Projekte\BeerMat\BeerMat.Gui\images\Radeberger_sample1.bmp");
            cboSteps.SelectedIndex = 0;
        }

        public void TestShapeDetection(double cannyTreshold, double linkingTreshold)
        {
            var shapeDetector = new ShapeDetection();
            shapeDetector.OnStepCompleted += shapeDetector_OnStepCompleted;


            var sample = GetSampleImage();
            imgOriginal.Source = ConvertToBitmapImage(sample);

            var imageData = new ImageData {OriginalImage = sample};

            shapeDetector.Process(imageData, cannyTreshold, linkingTreshold);
        }

        private void shapeDetector_OnStepCompleted(object sender, StepCompletedEventArgs e)
        {
            var selectedStep = FilterType.Treshold;

            var value = (string) cboSteps.SelectedValue;

            Enum.TryParse(value, true, out selectedStep);

            if (selectedStep == e.ProcessingStep)
            {
                imgResult.Source = ConvertToBitmapImage(e.ImageData.CurrentImage);
            }
        }


        private Image<Bgr, byte> GetSampleImage()
        {
            Image<Bgr, byte> image = null;

            if (rbCamera.IsChecked.Value)
            {
                image = camera.QueryFrame();
            }
            else
            {
                image = this.sampleImage;
            }

            return image;
        }


        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.sychronizationContext.Send(
                o => TestShapeDetection(cannyTreshold.Value, linkingTreshold.Value), null);

            //this.sychronizationContext.Send(
            //    o => TestTresholding(), null);
            //using (Image<Bgr, byte> frame = camera.QueryFrame())
            //{
            //    DisplayImage(frame);
            //}
        }

        private BitmapImage ConvertToBitmapImage<T>(Image<T, byte> processedImage) where T : struct, IColor
        {
            using (var stream = new MemoryStream())
            {
                // My way to display frame 
                processedImage.Bitmap.Save(stream, ImageFormat.Bmp);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(stream.ToArray());
                bitmap.EndInit();
                return bitmap;
            }
        }
    }
}