namespace ImageEditor.Filters
{
    public class GaussianSmoothing : ConvolutionFilterBase
    {
        public override double Divisor => 8.0;
        public override double Bias => 0.0;
        public override string Name => "Gaussian smoothing";
        public override double[,] Matrix => new double[,] { { 0, 1, 0 }, { 1, 4, 1 }, { 0, 1, 0 } };
    }
}
