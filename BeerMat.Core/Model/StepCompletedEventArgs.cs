namespace BeerMat.Core.Model
{
    /// <summary>
    /// A uniform result container
    /// </summary>
    public class StepCompletedEventArgs
    {
        #region Public Properties

        public ImageData ImageData { get; set; }

        public FilterType ProcessingStep { get; set; }

        #endregion
    }
}