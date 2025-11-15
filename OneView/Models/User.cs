using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneView.Models
{
    /// <summary>
    /// Represents a user's profile information
    /// Stores personal details separate from login credentials
    /// Persisted to JSON in app's private data directory
    /// </summary>
    public class User
    {
        // Private backing fields
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _nickName = string.Empty;

        /// <summary>
        /// User's first name
        /// Used for personalization in the UI
        /// Public setter required for JSON deserialization
        /// </summary>
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        /// <summary>
        /// User's last name
        /// Used for full name display
        /// Public setter required for JSON deserialization
        /// </summary>
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        /// <summary>
        /// User's nickname or display name
        /// Optional alternative to first/last name
        /// Public setter required for JSON deserialization
        /// </summary>
        public string NickName
        {
            get { return _nickName; }
            set { _nickName = value; }
        }
    }
}
