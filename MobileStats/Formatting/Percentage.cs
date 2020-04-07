using System;
using System.Diagnostics;

namespace MobileStats.Formatting
{
    struct Percentage
    {
        public double Fraction { get; }
        public int Samples { get; }

        public static Percentage FromSamples(int countedSamples, int totalSamples)
        {
            Debug.Assert(countedSamples >= 0, "counted samples must be non negative");
            Debug.Assert(totalSamples >= 0, "total samples must be non-negative");

            var fraction = (countedSamples, totalSamples) switch
            {
                (0, _) => 0,
                (_, 0) => double.NaN,
                _ => (double) countedSamples / totalSamples
            };
            
            return new Percentage(fraction, totalSamples);
        }
        public static Percentage FromFraction(double fraction, int totalSamples)
        {
            Debug.Assert(fraction >= 0 || double.IsNaN(fraction), "fraction must be non-negative");
            Debug.Assert(totalSamples >= 0, "total samples must be non-negative");
            
            return new Percentage(fraction, totalSamples);
        }
        
        private Percentage(double fraction, int samples)
        {
            Fraction = fraction;
            Samples = samples;
        }
        
        public string ToStringWithConfidence()
        {
            return double.IsNaN(Fraction) ? "N/A" : $"{Fraction * 100:0.00}% (±{formatConfidence()}%)";
        }

        private string formatConfidence()
        {
            if (Samples == 0)
                return "N/A";
            
            const double z = 1.96; // for 95% confidence interval

            var deviation = z * Math.Sqrt(Fraction * (1 - Fraction) / Samples);

            return deviation.ToString("0.00");
        }
        
        public string ToStringWithoutDecimals()
        {
            if (double.IsNaN(Fraction))
                return "N/A";
            
            return Fraction switch
            {
                0 => "0%",
                _ when Fraction < 0.01 => "<1%",
                _ => $"{Fraction * 100:0}%"
            };
        }

        public override string ToString()
        {
            return double.IsNaN(Fraction) ? "N/A" : $"{Fraction * 100:0.00}%";
        }
    }
}