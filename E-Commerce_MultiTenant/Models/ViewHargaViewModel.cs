using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Models
{
    public class ViewHargaViewModel
    {
        public IEnumerable<ViewHarga> ListViewnya { get; set; }
        public ViewHarga ViewTerpilih { get; set; }
    }
}