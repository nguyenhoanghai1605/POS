using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class KCVModel
    {
        public int ID { get; set; }
        public string SIZE { get; set; }
        public string KCV_Type { get; set; }
        public string KCV { get; set; }
        public string KCV_Name { get; set; }
        public string Thong_so_KCV { get; set; }
        public decimal MEASURE { get; set; }
        public string MEASURE_Details { get; set; }
        public decimal TLSP { get; set; }
        public int SL { get; set; }
        public string Retail_Price { get; set; }
        public float Discount { get; set; }
        public string POS_Discount { get; set; }
        public string Note { get; set; }
    }

}