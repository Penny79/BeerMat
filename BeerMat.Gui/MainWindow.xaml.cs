using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        private readonly BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

        private bool isRestartNeeded = false;

        private BeerMatDetectionResult lastResult;

        private Timer timer;

        #endregion

        #region Constructors and Destructors

        public MainWindow()
        {
            this.InitializeComponent();

            //TestMarker();           
            this.camera = new Capture(0);

            foreach (var step in Enum.GetNames(typeof(ProcessingSep)))
            {
                this.cboSteps.Items.Add(step);
            }

            this.sampleImage = new Image<Bgr, byte>(@"D:\Projekte\BeerMat\BeerMat.Gui\images\grabedFrame4.bmp");
            this.cboSteps.SelectedIndex = 0;

            this.rbCamera.Checked += this.SourceChanged;
            this.rbSample.Checked += this.SourceChanged;

            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += this.DispatcherTimerTick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            dispatcherTimer.IsEnabled = true;
          //  dispatcherTimer.Start();

            this.worker.ProgressChanged += this.WorkerOnProgressChanged;
            this.worker.DoWork += this.WorkerDoWork;
            this.worker.RunWorkerCompleted += this.worker_RunWorkerCompleted;
            this.worker.RunWorkerAsync(this.rbCamera.IsChecked.Value);

           // var frame = this.camera.QueryFrame();
           // frame.Save(@"D:\Projekte\BeerMat\BeerMat.Gui\images\grabedFrame.bmp");
        }

        #endregion

        #region Methods

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

        private void SourceChanged(object sender, RoutedEventArgs e)
        {
            this.isRestartNeeded = true;
        }

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

                    if (DateTime.Now.Year>2013)
                    {
                        nextFrame.Save(@"D:\Projekte\BeerMat\BeerMat.Gui\images\grabedFrame4.bmp");
                    }
                    var result = shapeDetector.Process(nextFrame);

                    this.worker.ReportProgress(1, result);
                }
            }
        }

        private void WorkerOnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            var processingResult = progressChangedEventArgs.UserState as BeerMatDetectionResult;
            this.lastResult = processingResult;

            if (this.isRestartNeeded)
            {
                this.worker.CancelAsync();
                this.isRestartNeeded = false;
            }
        }

        private void DispatcherTimerTick(object sender, EventArgs e)
        {
            var step = (ProcessingSep)Enum.Parse(typeof(ProcessingSep), (string)this.cboSteps.SelectedValue);
 
            if (this.lastResult != null)
            {
                switch (step)
                {
                    case ProcessingSep.Preprocessed:
                        this.imgResult.Source = this.ConvertToBitmapImage(this.lastResult.ThreshholdedImage);
                        break;
                    case ProcessingSep.ShapesDetected:
                        this.imgResult.Source = this.ConvertToBitmapImage(this.lastResult.ImageWithShapes);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
               

                this.imgOriginal.Source = this.ConvertToBitmapImage(this.lastResult.OriginalImage);

                int fps = Convert.ToInt32(1000f / this.lastResult.TimeTaken.Milliseconds);

                this.lblFps.Content = fps.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.worker.RunWorkerAsync(this.rbCamera.IsChecked.Value);
        }

        #endregion
    }
}