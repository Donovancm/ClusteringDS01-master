using System;
using System.Collections.Generic;
using System.Text;

namespace ClusteringDS01.Distances
{
    interface IDistance
    {
        double ComputeDistance(double[] X, double[] Y);
    }
}
