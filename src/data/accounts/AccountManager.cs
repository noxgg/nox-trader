using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace noxiousET.src.data.accounts
{
    class AccountManager
    {
        Dictionary<String, Account> accounts;

        public AccountManager()
        {
            accounts = new Dictionary<String, Account>();
        }

        public void addAccount(Account account)
        {
            if (accounts.ContainsKey(account.l))
                accounts.Remove(account.l);
            accounts.Add(account.l, account);
        }
    }
}
