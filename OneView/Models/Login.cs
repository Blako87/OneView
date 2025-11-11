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
            set { _username = value; }  // Changed to public setter for JSON serialization
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }  // Changed to public setter for JSON serialization
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }  // Changed to public setter for JSON serialization
        }
    }
}
