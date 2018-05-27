using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Models
{
    public class UkuranOrder
    {
        public int id_ukuran_order { get; set; }
        public int id_ukuran { get; set; }
        public string ukuran { get; set; }
        public int no_detail { get; set; }
        public int jumlah { get; set; }
        public string tambahan { get; set; }
    }
}