using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneView.Models
{
    public class Login
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _email = string.Empty;

        public Login(string username, string password)
        {
            _username = username;
            _password = password;
        }
        public string Username
        {
            get { return _username; } 
            set { _username = value; }
        }
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        public Login(string email)
        {
            _email = email;
        }
        public string Email
        {
            get { return _email; }
            set { _email = value; }

        }
        
        



    }
}
