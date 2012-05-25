using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noxiousET.src.data.accounts
{
    class Account
    {
        public String l { set; get; }
        public String p {set; get;}

        public Account(String l, String p)
        {
            this.l = l;
            this.p = p;
        }
    }
}
