using System;
using System.Collections.Generic;

namespace noxiousET.src.data.accounts
{
    internal class AccountManager
    {
        private readonly Dictionary<String, Account> _accounts;

        public AccountManager()
        {
            _accounts = new Dictionary<String, Account>();
        }

        public void AddAccount(Account account)
        {
            if (_accounts.ContainsKey(account.UserName))
                _accounts.Remove(account.UserName);
            _accounts.Add(account.UserName, account);
        }
    }
}