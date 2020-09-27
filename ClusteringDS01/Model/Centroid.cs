using ClusteringDS01.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusteringDS01.Model
{
    public class Centroid
    {
        //Centroidnummer + Coordinaten
        public static Dictionary<int, List<double>> Centroids { get; set; }

        // centroidnumber + customerinfo
        public static Dictionary<int, List<CustomerInfo>> Points { get; set; }

        public static double sse = 0.0;
        public static Dictionary<int, List<CustomerInfo>> sseCentroids { get; set; }

        public static Dictionary<int, List<Tuple<int, double>>> centroidDistances { get; set; }


        public Centroid() { }

        /// <summary>
        /// Creeren van aantal centroids K
        /// </summary>
        /// <param name="k"> aantal centroid </param>
        /// <returns></returns>
        public static Dictionary<int, List<double>> Initialize(int k)
        {
            Centroids = new Dictionary<int, List<double>>();
            for (int i = 1; i <= k; i++)
            {
                Centroids.Add(i , GenerateCentroidPosition());
            }
            return Centroids;

        }

        /// <summary>
        /// Genereert een random centroidposition met 32 points
        /// </summary>
        /// <returns>Lijst van points in double</returns>
        public static List<double> GenerateCentroidPosition()
        {
            List<double> centroidposition = new List<double>();
            Random rng = new Random();
            for (int i = 1; i <= 32; i++)
            {
                double randomNumber = rng.NextDouble() * (1.0 - 0.0) + 0.0;
                centroidposition.Add(randomNumber);
            }
            return centroidposition;
        }

        /// <summary>
        ///  Toevoegen personen naar een cluster
        /// </summary>
        /// <param name="centroidNumber">Centroid X</param>
        /// <param name="customer">klant object X</param>
        public static void AddPoint(int centroidNumber, CustomerInfo customer)
        {
            if (Points is null)
            {
                Points = new Dictionary<int, List<CustomerInfo>>();
            }
            if (Points.ContainsKey(centroidNumber))
            {
                Points[centroidNumber].Add(customer);
            }
            else { Points.Add(centroidNumber, new List<CustomerInfo>() { customer }); }

        }

        /// <summary>
        /// Lijst van points leeghalen
        /// </summary>
        public static void ClearPointList()
        {
            Points.Clear();
        }

        /// <summary>
        /// Nieuwe centroid positie bereken
        /// </summary>
        /// <returns>lijst van nieuwe centroid posities </returns>
        public static Dictionary<int, List<double>> CalculateCentroidPosition()
        {
            foreach (var cluster in Points)
            {
                List<CustomerInfo> customers = cluster.Value;
                List<double> positions = new List<double>();
                for (int i = 0; i < 32; i++)
                {
                    double totalOfferPoints = 0.0; // todo andere naam
                    foreach (var customer in customers)
                    {
                        totalOfferPoints += customer.Offer.ToArray()[i];
                    }
                    positions.Add(totalOfferPoints / customers.Count);
                }
                Centroids[cluster.Key] = positions;
            }

            return Centroids;
        }

        /// <summary>
        /// Afstand berekenen van Centroid points X naar Persoon points Y 
        /// </summary>
        /// <param name="X">Centroid points X</param>
        /// <param name="Y">Persoon points Y</param>
        /// <returns></returns>
        public static double ComputeDistance( double[] X, int[] Y)
        {
            double distance = 0.0;
            int row2DArrayX = X.Length;
            int row2DArrayY = Y.Length;
            for (int i = 0; i < row2DArrayX; i++)
            {
                distance += Math.Pow((X[i] - Y[i]), 2);
            }
            var result = Math.Sqrt(distance);
            return result;
        }

        /// <summary>
        /// Updaten van beste SSE en beste cluster 
        /// </summary>
        public static void UpdateSSE()
        {
            double newSse = CalcAverageSSECentroids();

            if (sse != 0.0)
            {
                if (sse > newSse)
                {
                    sse = newSse;
                }
                sseCentroids = Points;
            }
            else { sse = newSse; sseCentroids = Points; }

        }

        /// <summary>
        /// Bereken gemiddelde SSE
        /// </summary>
        /// <returns>SSE in double </returns>
        public static double CalcAverageSSECentroids()
        {
            Dictionary<int, double> centroidsAvgSSE = new Dictionary<int, double>();
            double sseAverage = 0.0;
            foreach (var cluster in Points)
            {
                List<CustomerInfo> customers = cluster.Value;
                double totalDistance = 0.0;
                foreach (var customer in customers)
                {
                    totalDistance += ComputeDistance(Centroids[cluster.Key].ToArray(), customer.Offer.ToArray());
                }
                sseAverage += (totalDistance / customers.Count);
            }
            return sseAverage;
        }

        /// <summary>
        /// Bereken afstand tussen persoon X en centroid X
        /// </summary>
        public static void CalcCentroidDistance()
        {
            centroidDistances = new Dictionary<int, List<Tuple<int, double>>>();
            foreach (var customer in CsvReader.customersDictionary)
            {
                DistanceToCentroid(customer.Value);
            }

        }

        /// <summary>
        /// Het berekening van de distance en het koppelen naar een dictionary
        /// </summary>
        /// <param name="customer"></param>
        public static void DistanceToCentroid(CustomerInfo customer)
        {

            var pointsDistance = new List<Tuple<int, double>>();
            foreach (var centroid in Centroids)
            {
                double distance = ComputeDistance(centroid.Value.ToArray(), customer.Offer.ToArray());

                if (centroidDistances.ContainsKey(customer.CustomerId))
                {
                    centroidDistances[customer.CustomerId].Add(new Tuple<int, double>(centroid.Key, distance));
                }
                else
                {
                    pointsDistance.Add(new Tuple<int, double>(centroid.Key, distance));
                    centroidDistances.Add(customer.CustomerId, pointsDistance);
                }

            }


        }

        /// <summary>
        /// Alle points koppelen naar een centroid
        /// </summary>
        public static void AssignToCluster()
        {
            foreach (var distance in centroidDistances)
            {
                int centroidNumber = ShortestDistance(distance.Value).Item1;
                AddPoint(centroidNumber, CsvReader.customersDictionary[distance.Key]);
            }
        }
        /// <summary>
        /// Pak de klantId en de afstand die hets dichtsbij de centroid behoord
        /// </summary>
        /// <param name="centroidDistance">Lijst van klantId, afstand</param>
        /// <returns>KlantId en afstand</returns>
        public static Tuple<int, double> ShortestDistance(List<Tuple<int, double>> centroidDistance)
        {
            Tuple<int, double> distanceCentroid = centroidDistance.OrderBy(x => x.Item2).First();
            return distanceCentroid;
        }

        /// <summary>
        /// Ophalen van totale gekochte offertes van elke cluster
        /// </summary>
        /// <returns>Lijst OfferteId, (Centroid, aantal) </returns>
        public static Dictionary<int, List<Tuple<int,int>>> GetTopDeals()
        {
            Dictionary<int, List<Tuple<int, int>>> topDeals = new Dictionary<int, List<Tuple<int, int>>>();
            for (int offerID = 1; offerID <= 32; offerID++)
            {
                foreach (var cluster in Points)
                {
                    List<CustomerInfo> customers = cluster.Value;
                    int sum = 0;
                    foreach (var customer in customers)
                    {
                        sum += customer.Offer.ElementAt(offerID-1);
                    }
                    if (topDeals.ContainsKey(cluster.Key))
                    {
                        topDeals[cluster.Key].Add( new Tuple<int, int>( offerID, sum ));
                    }
                    else { topDeals.Add(cluster.Key, new List<Tuple<int, int>> { new Tuple<int, int>(offerID, sum) }); }
                }
            }
            return topDeals;
        }

        /// <summary>
        /// Haal centroidnummer d.m.v klantId
        /// </summary>
        /// <param name="id">KlantId X</param>
        /// <returns>Centroidnummer</returns>
        public static int GetSSECentroidByCustomerId(int id)
        {
            int centroid = 0;
            foreach (var cluster in sseCentroids)
            {
                var customers = cluster.Value;
               if(customers.Any(k => k.CustomerId == id))
                {
                    centroid = cluster.Key;
                    return centroid;
                }
            }
           return centroid;
        }
    }
}
