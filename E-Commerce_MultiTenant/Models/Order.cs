using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Models
{
    public class Order
    {
        public int no_order { get; set; }
        public int id_customer { get; set; }
        public string tgl_order { get; set; }
        public string status_bayar { get; set; }
        public string tgl_konfirmasi { get; set; }
        public int total_harga { get; set; }
        public string dikirim { get; set; }
        public string nama_customer { get; set; }
    }
}