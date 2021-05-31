using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountClass
{
    class Account
    {
        public string login { get; set; }
        public string password { get; set; }

        public Account() { }
        public Account(string Login, string Password) 
        {
            this.login = login;
            this.password = password;
        }
    }
}
