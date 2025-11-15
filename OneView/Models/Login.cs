namespace OneView.Models
{
    /// <summary>
    /// Represents user login credentials
    /// Stored securely in JSON format in app's private data directory
    /// All properties have public setters to allow JSON deserialization
    /// </summary>
    public class Login
    {
        // Private backing fields for encapsulation
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _email = string.Empty;

        /// <summary>
        /// User's login username
        /// Required for authentication
        /// Public setter needed for JSON deserialization
        /// </summary>
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// User's login password
        /// WARNING: Stored in plain text - should be hashed in production!
        /// Public setter needed for JSON deserialization
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// User's email address
        /// Optional field for account recovery or notifications
        /// Public setter needed for JSON deserialization
        /// </summary>
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }
    }
}
