using System.Diagnostics;
using OneView.Models;

namespace OneView.Services
{
    /// <summary>
    /// Manages user authentication and login state
    /// Integrates with SaveprofileData for persistent storage of credentials
    /// Provides login, logout, and session management functionality
    /// </summary>
    public class LoginService
    {
        // Persistence service for saving/loading login data
        private readonly SaveprofileData _saveService = new();
        
        // Current logged-in user (null if not logged in)
        private Login? _currentLogin;

        /// <summary>
        /// Attempts to load previously saved login data from disk
        /// Called on app startup to restore login session
        /// </summary>
        /// <returns>True if login data was found and loaded, false otherwise</returns>
        public bool LoadLogin()
        {
            // Load login data from JSON file
            _currentLogin = _saveService.LoadLoginData();
            
            // Check if loaded login is valid (has username and password)
            if (_currentLogin != null && IsLoggedIn())
            {
                Debug.WriteLine($"✅ Login loaded: {_currentLogin.Username}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Performs user login with provided credentials
        /// Saves credentials to disk for persistence across app restarts
        /// WARNING: Passwords are stored in plain text - should use hashing in production
        /// </summary>
        /// <param name="username">User's username (required)</param>
        /// <param name="password">User's password (required)</param>
        /// <param name="email">User's email (optional)</param>
        /// <returns>True if login successful and saved, false on validation or save error</returns>
        public bool Login(string username, string password, string email = "")
        {
            // Validate required fields are not empty
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Debug.WriteLine("❌ Login failed: Username or password empty");
                return false;
            }

            // Create new login object with provided credentials
            _currentLogin = new Login
            {
                Username = username,
                Password = password,
                Email = email
            };

            // Persist login data to disk
            bool saved = _saveService.SaveLoginData(_currentLogin);
            
            if (saved)
            {
                Debug.WriteLine($"✅ Login successful: {username}");
            }
            else
            {
                Debug.WriteLine("❌ Failed to save login data");
            }

            return saved;
        }

        /// <summary>
        /// Logs out the current user by clearing the session
        /// Note: This does NOT delete the saved login file - use SaveprofileData.ClearAllData() for that
        /// </summary>
        public void Logout()
        {
            _currentLogin = null;
            Debug.WriteLine("🔓 User logged out");
        }

        /// <summary>
        /// Checks if a user is currently logged in
        /// Validates that credentials exist and are not empty
        /// </summary>
        /// <returns>True if user is logged in with valid credentials, false otherwise</returns>
        public bool IsLoggedIn()
        {
            return _currentLogin != null &&
                   !string.IsNullOrEmpty(_currentLogin.Username) &&
                   !string.IsNullOrEmpty(_currentLogin.Password);
        }

        /// <summary>
        /// Gets the current logged-in user's credentials
        /// Returns null if no user is logged in
        /// </summary>
        /// <returns>Login object with current credentials, or null if not logged in</returns>
        public Login? GetCurrentLogin()
        {
            return _currentLogin;
        }

        /// <summary>
        /// Gets the username of the currently logged-in user
        /// Returns "Guest" if no user is logged in
        /// Useful for displaying username in UI
        /// </summary>
        /// <returns>Username string, or "Guest" if not logged in</returns>
        public string GetUsername()
        {
            return _currentLogin?.Username ?? "Guest";
        }
    }
}
