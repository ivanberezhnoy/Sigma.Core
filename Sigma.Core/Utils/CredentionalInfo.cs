﻿namespace Sigma.Core.Utils
{
    public class CredentionalInfo
    {
        public CredentionalInfo(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
