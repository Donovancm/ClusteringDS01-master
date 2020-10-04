using ClusteringDS01.Model;
using ClusteringDS01.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusteringDS01.Distances
{
    public class Silhouette
    {
        /// <summary>
        ///  DistancesCohesion = een dictonary van  key: klantIdX, value: list van centroid nummer, klantidY en afstand
        /// </summary>
        public static Dictionary<int, List<Tuple<int, int, double>>> DistancesCohesion = new Dictionary<int, List<Tuple<int, int, double>>>();

        /// <summary>
        /// DistancesSeperation = een dictonary van  key: klantIdX, value: list van centroid nummer, klantidY en afstand
        /// </summary>
        public static Dictionary<int, List<Tuple<int, int, double>>> DistancesSeperation = new Dictionary<int, List<Tuple<int, int, double>>>();

        /// <summary>
        /// SilhouetteValues = een dictonary van key: persoon, value: silhouette waarde in double
        /// </summary>
        public static Dictionary<string, double> SilhouetteValues = new Dictionary<string, double>();

        /// <summary>
        /// Berekent de silhouette waarde tussen -1 en 1 per persoon.
        /// 
        /// </summary>
        /// <param name="customerId">KlantId X</param>
        public static void CalculateSilhouette(int customerId, int centroidnr)
        {
            double silhouette = 0.0;
            var cohesion = AverageCohesion(customerId);
            var seperation = AverageSeparation(customerId, centroidnr);
            if (cohesion < seperation)
            {
                silhouette = 1 - (cohesion / seperation);
            }
            if (cohesion == seperation)
            {
                // === 0
                silhouette = 0.0;
            }
            if (seperation < cohesion)
            {
                // (sep(i) / co(i) ) - 1
                silhouette = (seperation / cohesion) - 1;
            }
            if (silhouette == 0)
            {
                Console.WriteLine("Customer: " + customerId + ", coh: " + cohesion + ", sep: " + seperation + ", silhouttevalue: " + silhouette);
            }
            var customerName = CsvReader.customersDictionary[customerId].CustomerName;
            SilhouetteValues.Add(customerName, silhouette);
        }

        /// <summary>
        /// Checkt of de combinatie van personen al bestaat in Cohesion
        /// </summary>
        /// <param name="x">Persoon X</param>
        /// <param name="y">Persoon Y</param>
        /// <param name="centroid"> Cluster Centroid</param>
        /// <returns>True of false</returns>
        public static Boolean HasDuplicate(int x, int y, int centroid)
        {
            return DistancesCohesion[centroid].Any<Tuple<int, int, double>>(value => (value.Item1 == x && value.Item2 == y) || (value.Item1 == y && value.Item2 == x));
        }

        /// <summary>
        /// Checkt of de combinatie van personen al bestaat in Seperation
        /// </summary>
        /// <param name="x">Persoon X</param>
        /// <param name="y">Persoon Y</param>
        /// <returns>True of false</returns>
        public static Boolean HasDuplicate2(int x, int y)
        {
            if (DistancesSeperation.ContainsKey(x) && DistancesSeperation[x].Any<Tuple<int, int, double>>(value => (value.Item2 == y)))
            {
                return true;
            }
            else if (DistancesSeperation.ContainsKey(y) && DistancesSeperation[y].Any<Tuple<int, int, double>>(value => (value.Item2 == x)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Berekent afstand voor Seperation
        /// </summary>
        /// <param name="customerId">KlantId X</param>
        public static void CalculateDistanceSeperation(int customerId)
        {
            if (!DistancesSeperation.ContainsKey(customerId))
            {
                DistancesSeperation.Add(customerId, new List<Tuple<int, int, double>>());
            }
            Euclidean euclidean = new Euclidean();
            var customerCentroid = Centroid.GetSSECentroidByCustomerId(customerId);
            foreach (var cluster in Centroid.sseCentroids)
            {
                if (cluster.Key != customerCentroid)
                {
                    foreach (var customer in cluster.Value)
                    {
                        if (!HasDuplicate2(customerId, customer.CustomerId))
                        {
                            int[] pointX = Centroid.sseCentroids[customerCentroid].Find(c => c.CustomerId == customerId).Offer.ToArray();
                            DistancesSeperation[customerId].Add(new Tuple<int, int, double>(cluster.Key, customer.CustomerId, euclidean.ComputeDistance(pointX, customer.Offer.ToArray())));
                        }
                    }
                }

            }

        }

        /// <summary>
        ///  Berekent gemiddelde afstand van Seperation
        /// </summary>
        /// <param name="customerId">KlantId X</param>
        /// <returns>afstand in double</returns>
        public static double AverageSeparation(int customerId, int centroidnr)
        {
            var sum = 0.0;
            var keys = DistancesSeperation.Keys;
            foreach (var id in keys)
            {
                if (DistancesSeperation[id].Any(x => x.Item2 == customerId))
                {
                    sum += DistancesSeperation[id].Find(a => a.Item2 == customerId).Item3;
                }
            }
            sum += DistancesSeperation[customerId].Sum(x => x.Item3);
            var observationCount = DistancesSeperation[customerId].Count();
            double result = sum / observationCount;
            return result;
        }

        /// <summary>
        ///  Afstanden bereken binnen een cluster voor Cohesion
        /// </summary>
        /// <param name="customerId">KlantId X</param>
        /// <param name="centroid">Cluster Centroid X</param>
        public static void CalculateDistanceCohesion(int customerId, int centroid)
        {
            if (!DistancesCohesion.ContainsKey(centroid))
            {
                DistancesCohesion.Add(centroid, new List<Tuple<int, int, double>>());
            }
            Euclidean euclidean = new Euclidean();
            var cluster = Centroid.sseCentroids[centroid];
            var filterCustomers = cluster.Where(customer => customer.CustomerId != customerId);
            foreach (var customer in filterCustomers)
            {
                if (!HasDuplicate(customerId, customer.CustomerId, centroid))
                {
                    int[] pointX = Centroid.sseCentroids[centroid].Find(c => c.CustomerId == customerId).Offer.ToArray();
                    DistancesCohesion[centroid].Add(new Tuple<int, int, double>(customerId, customer.CustomerId, euclidean.ComputeDistance(pointX, customer.Offer.ToArray())));
                }
            }
        }

        /// <summary>
        /// Gemiddelde afstand voor Cohesion
        /// </summary>
        /// <param name="customerId">KlantId X</param>
        /// <returns>afstand cohesion in double</returns>
        public static double AverageCohesion(int customerId)
        {
            double result = 0.0;
            int centroid = Centroid.GetSSECentroidByCustomerId(customerId);
            var cluster = Centroid.sseCentroids[centroid];
            var filterList = DistancesCohesion[centroid].Where(c => c.Item1 == customerId || c.Item2 == customerId).ToList();
            var observationCount = filterList.Count;
            result = filterList.Sum(x => x.Item3) / observationCount;
            return result;
        }

        /// <summary>
        /// Uitvoeren Cohesion en Seperation van klanten in clusters
        /// </summary>
        public static void Init()
        {
            foreach (var cluster in Centroid.sseCentroids)
            {
                foreach (var customer in cluster.Value)
                {
                    CalculateDistanceCohesion(customer.CustomerId, cluster.Key);
                    CalculateDistanceSeperation(customer.CustomerId);
                }
            }

        }
    }
}
