using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Models
{
    public class DetailOrder
    {
        public int no_detail { get; set; }
        public int no_order { get; set; }
        public int id_produk { get; set; }
        public int id_bahan { get; set; }
        public int id_jns_sablon { get; set; }
        public string desain { get; set; }
        public int jumlah { get; set; }
        public int subtotal { get; set; }
        public string namaproduk { get; set; }
        public string namabahan { get; set; }
        public string namasablon { get; set; }
    }
}