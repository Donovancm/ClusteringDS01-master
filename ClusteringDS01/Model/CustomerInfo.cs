using System;
using System.Collections.Generic;
using System.Text;

namespace ClusteringDS01.Model
{
    public class CustomerInfo
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<int> Offer { get; set; }

        public CustomerInfo(int customerNumber, string customerName ,List<int> points )
        {
            CustomerId = customerNumber;
            CustomerName = customerName;
            Offer = points;
        }

        public CustomerInfo() { }
    }
}
