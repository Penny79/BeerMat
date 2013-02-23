using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using BeerMat.Core;
using BeerMat.Core.Concrete;
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
        #region Fields

        private readonly Capture camera;

        private readonly Image<Bgr, byte> sampleImage;

        private BeerMatDetectionResult lastResult;
        private readonly BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

        private bool isRestartNeeded = false;

        private Timer timer;

        #endregion

        #region Constructors and Destructors

        public MainWindow()
        {
            this.InitializeComponent();

            //TestMarker();           
            this.camera = new Capture(0);            

            foreach (var step in Enum.GetNames(typeof(FilterType)))
            {
                this.cboSteps.Items.Add(step);
            }

            this.sampleImage = new Image<Bgr, byte>(@"D:\Projekte\BeerMat\BeerMat.Gui\images\Radeberger_sample1.bmp");
            this.cboSteps.SelectedIndex = 0;

            this.rbCamera.Checked += SourceChanged;
            this.rbSample.Checked += SourceChanged;

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            dispatcherTimer.IsEnabled = true;
            dispatcherTimer.Start();

           
            
            this.worker.ProgressChanged+=WorkerOnProgressChanged;
            this.worker.DoWork += WorkerDoWork;            
            this.worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            this.worker.RunWorkerAsync(this.rbCamera.IsChecked.Value);
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (this.lastResult != null)
            {
                this.imgResult.Source = this.ConvertToBitmapImage(this.lastResult.ImageWithShapes);
                this.imgOriginal.Source = this.ConvertToBitmapImage(this.lastResult.OriginalImage);
            }
        }
        

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.worker.RunWorkerAsync(this.rbCamera.IsChecked.Value);
        }

        private void SourceChanged(object sender, RoutedEventArgs e)
        {
            isRestartNeeded = true;
        }

        #endregion

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var useCamera = (bool)e.Argument;

            var backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker != null)
            {
                while (!backgroundWorker.CancellationPending)
                {
                    var shapeDetector = new BeerMatDetection();

                    Image<Bgr, byte> nextFrame = useCamera ? this.camera.QueryFrame() : this.sampleImage;

                    var result = shapeDetector.Process(nextFrame);

                    worker.ReportProgress(1, result);
                }
            }
        }


        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {           
            var processingResult = progressChangedEventArgs.UserState as BeerMatDetectionResult;
            this.lastResult = processingResult;
            
            if (isRestartNeeded)
            {
                this.worker.CancelAsync();
                isRestartNeeded = false;
            }
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