using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace POS.Models
{
    public class Common
    {
        public class Input
        {

        }
        public class Output
        {
            public class CommonOutput
            {
                public int KetQua { get; set; }
                public string ThongBao { get; set; }
                public object DuLieu { get; set; }
            }
        }
    }
}