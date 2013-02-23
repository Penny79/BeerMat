using BeerMat.Core.Model;
using Emgu.CV;
using Emgu.CV.Structure;

namespace BeerMat.Core.Filter
{
    interface IImageFilter
    {
        ImageData Run(ImageData imageData);             

        FilterType FilterType { get;}
    }
}
