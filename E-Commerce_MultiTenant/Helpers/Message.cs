using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace E_Commerce_MultiTenant.Helpers
{
    public static class Message
    {
        public static string GetPesan(string judul, string jenis, string keterangan)
        {
            return string.Format(@"<div class='alert alert-{0}'>
<a href='#' class='close' data-dismiss='alert' aria-label='close'>&times;</a>
<strong>{1}</strong>&nbsp;{2}</div>", jenis, judul, keterangan);
        }
    }
}