using ClusteringDS01.Distances;
using ClusteringDS01.Model;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClusteringDS01.Export
{
    public class ExportExcel
    {
        public static ExcelPackage excelHelper { get; set; }
        public static int k { get; set; }
        public static ExcelWorksheet currWorksheet { get; set; }

        /// <summary>
        /// Creeren van excelsheets 
        /// </summary>
        public static void Init()
        {
            k = Centroid.sseCentroids.Count;
            excelHelper = new ExcelPackage();
            currWorksheet = excelHelper.Workbook.Worksheets.Add("K:" + k);
        }

        /// <summary>
        /// Exporteren van alle worksheets
        /// </summary>
        public static void Export()
        {
            var curDir = Directory.GetCurrentDirectory();
            var rootProjectDir = curDir.Remove(curDir.IndexOf("\\bin\\Debug\\netcoreapp2.2"));

            var memStream = new MemoryStream();
            excelHelper.SaveAs(memStream);
            memStream.Position = 0;
            byte[] bytes = new byte[memStream.Length];
            memStream.Read(bytes, 0, (int)memStream.Length);
            System.IO.File.WriteAllBytes($"{rootProjectDir}\\Output\\" + "K_" + k + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".xlsx", bytes);
        }

        /// <summary>
        ///  Creeren van cluster worksheet
        /// </summary>
        public static void CreateClusterWorkSheet()
        {
            currWorksheet.Cells[1, 1].Value = "SSE: " + Centroid.sse;
            foreach (var cluster in Centroid.sseCentroids)
            {
                currWorksheet.Cells[cluster.Key + 3, 1].Value = cluster.Key;
                currWorksheet.Cells[cluster.Key + 3, 1].Style.Font.Bold = true;
                List<CustomerInfo> customerInfos = cluster.Value;
                for (int i = 0; i < customerInfos.Count; i++)
                {
                    currWorksheet.Cells[cluster.Key + 3, i + 3].Value = customerInfos.ElementAt(i).CustomerName;
                }
            }
        }

        /// <summary>
        /// Creeren van silhouette worksheet
        /// </summary>
        public static void CreateSilhouetteWorkSheet()
        {
            currWorksheet = excelHelper.Workbook.Worksheets.Add("Silhouette");
            currWorksheet.Cells[1, 1].Value = "Silhout: " + Silhouette.SilhouetteValues.Average(x => x.Value);
            int rowCount = 3;
            foreach (var customer in Silhouette.SilhouetteValues)
            {
                currWorksheet.Cells[rowCount, 1].Value = customer.Key;
                currWorksheet.Cells[rowCount, 2].Value = customer.Value;
                rowCount++;
            }
        }


        /// <summary>
        /// Creeren van TopDeals worksheet
        /// </summary>
        public static void CreateTopDealsWorkSheet()
        {
            currWorksheet = excelHelper.Workbook.Worksheets.Add("TopDeals");
            Dictionary<int, List<Tuple<int, int>>> topDeals = Centroid.GetTopDeals();
            List<int> topDealsK = new List<int>();

            int i = 0;
            foreach (var key in topDeals.Keys)
            {
                currWorksheet.Cells[1, i + 2].Value = "Label: " + (key - 1);
                i++;
            }

            int idx = 0;
            foreach (var cluster in topDeals) // key => label , value => product, sum
            {
                var products = cluster.Value.OrderByDescending( x => x.Item2);
                int position = 1;
                foreach (var product in products)
                {
                    currWorksheet.Cells[position + 1, idx + 2].Value = "Artikelnr: " + product.Item1 + "("+ product.Item2+")";
                    position++;
                }
                idx++;
                //
            }
        }
    }
}
