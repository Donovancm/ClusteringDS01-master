using System;
using System.Collections.Generic;
using System.Text;

namespace ClusteringDS01.Distances
{
    public class Euclidean : IDistance
    {

        /// <summary>
        /// Bereken de Euclidean afstand tussen centroid X en persoon Y
        /// </summary>
        /// <param name="X">Lijst van Centroid points double</param>
        /// <param name="Y">Lijst van Persoon points double</param>
        /// <returns>afstand in double </returns>
        public double ComputeDistance(double[] X, double[] Y)
        {
            double distance = 0.0;
            //d(p,q) = d(q,p) = 
            int row2DArrayX = X.Length;
            int row2DArrayY = Y.Length;
            for (int i = 0; i < row2DArrayX; i++)
            {
               distance += Math.Pow((X[i] - Y[i]), 2); 
            }
            var result = Math.Sqrt(distance);
            return result;
        }

        //offer naar centroid berekenen
        //X offer Y = 1ste centroid
        /// <summary>
        /// Bereken de Euclidean afstand tussen centroid X en persoon Y 
        /// </summary>
        /// <param name="X">Lijst van Centroid point int</param>
        /// <param name="Y">Lijst van persoon point int</param>
        /// <returns></returns>
        public double ComputeDistance(int[] X, int[] Y) 
        {
            double distance = 0.0;
            //d(p,q) = d(q,p) = 
            int row2DArrayX = X.Length;
            int row2DArrayY = Y.Length;
            for (int i = 0; i < row2DArrayX; i++)
            {
                distance += Math.Pow((X[i] - Y[i]), 2);
            }
            var result = Math.Sqrt(distance);
            return result;
        }
    }
}
