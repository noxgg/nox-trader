using System;

namespace noxiousET.src.data.accounts
{
    class Account
    {
        public String l { set; get; }
        public String p { set; get; }
        public String id { set; get; }

        public Account(String l, String p, String user)
        {
            this.l = l;
            this.p = p;
            this.id = user;
        }
    }
}
