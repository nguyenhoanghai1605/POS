using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class KhachHangModel
    {
        public class Input
        {
            public class TimKiem
            {
                public string MaPhieuQuaTang { get; set; }
            }
            public class DangKy
            {
                public string MaPhieuQuaTang { get; set; }
                public string MaKH { get; set; }
            }
        }
    }
}