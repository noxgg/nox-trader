using System;

namespace noxiousET.src.data.accounts
{
    internal class Account
    {
        public Account(String userName, String password, String user)
        {
            UserName = userName;
            Password = password;
            Id = user;
        }

        public String UserName { set; get; }
        public String Password { set; get; }
        public String Id { set; get; }
    }
}