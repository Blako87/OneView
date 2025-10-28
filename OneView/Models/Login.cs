

namespace OneView.Models
{
    public class Login
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _email = string.Empty;


        public string Username
        {
            get { return _username; }
            private set { _username = value; }
        }
        public string Password
        {
            get { return _password; }
            private set { _password = value; }
        }

        public string Email
        {
            get { return _email; }
            private set { _email = value; }

        }





    }
}
