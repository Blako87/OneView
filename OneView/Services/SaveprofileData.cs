using System.Text.Json;
using System.Diagnostics;
using OneView.Models;

namespace OneView.Services
{
    /// <summary>
    /// Handles persistence of profile data (Login, User, Rideprofile) to JSON files
    /// All data is stored in the app's private data directory (FileSystem.AppDataDirectory)
    /// Provides save/load functionality with error handling and debug logging
    /// </summary>
    public class SaveprofileData
    {
        // File paths for each data type
        private readonly string _rideProfilePath;
        private readonly string _userProfilePath;
        private readonly string _loginDataPath;
        private readonly string _dataDirectory;

        /// <summary>
        /// Initializes the SaveprofileData service and sets up file paths
        /// Creates the data directory if it doesn't exist
        /// All files are stored as JSON in FileSystem.AppDataDirectory
        /// </summary>
        public SaveprofileData()
        {
            // Get the app's private data directory (platform-specific)
            _dataDirectory = FileSystem.AppDataDirectory;
            
            // Define file paths for each data type
            _rideProfilePath = Path.Combine(_dataDirectory, "rideprofile.json");
            _userProfilePath = Path.Combine(_dataDirectory, "userprofile.json");
            _loginDataPath = Path.Combine(_dataDirectory, "logindata.json");

            // Ensure data directory exists (should always exist, but check anyway)
            try
            {
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                    Debug.WriteLine($"✅ Created data directory: {_dataDirectory}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to create data directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves login credentials to JSON file
        /// File location: {AppDataDirectory}/logindata.json
        /// WARNING: Password is stored in plain text - should be hashed in production!
        /// </summary>
        /// <param name="logData">Login object containing username, password, and email</param>
        /// <returns>True if save was successful, false on error</returns>
        public bool SaveLoginData(Login logData)
        {
            try
            {
                // Create JSON with nice formatting (indented)
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(logData, options);
                
                // Write to file (overwrites existing file)
                File.WriteAllText(_loginDataPath, jsonString);
                Debug.WriteLine($"✅ Login data saved to: {_loginDataPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to save login data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves ride profile statistics to JSON file
        /// File location: {AppDataDirectory}/rideprofile.json
        /// Note: Timer property (Ticks) is ignored via [JsonIgnore] attribute
        /// </summary>
        /// <param name="profile">Rideprofile object with ride statistics</param>
        /// <returns>True if save was successful, false on error</returns>
        public bool SaveRideData(Rideprofile profile)
        {
            try
            {
                // Create options to handle non-serializable properties
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IgnoreReadOnlyProperties = false
                };

                string jsonString = JsonSerializer.Serialize(profile, options);
                File.WriteAllText(_rideProfilePath, jsonString);
                Debug.WriteLine($"✅ Ride data saved to: {_rideProfilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to save ride data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves user profile information to JSON file
        /// File location: {AppDataDirectory}/userprofile.json
        /// </summary>
        /// <param name="user">User object containing first name, last name, nickname</param>
        /// <returns>True if save was successful, false on error</returns>
        public bool SaveUserData(User user)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(user, options);
                File.WriteAllText(_userProfilePath, jsonString);
                Debug.WriteLine($"✅ User data saved to: {_userProfilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to save user data: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads login credentials from JSON file
        /// Returns empty Login object if file doesn't exist or is invalid
        /// </summary>
        /// <returns>Login object with credentials, or empty Login if not found</returns>
        public Login? LoadLoginData()
        {
            try
            {
                // Check if file exists
                if (!File.Exists(_loginDataPath))
                {
                    Debug.WriteLine($"⚠️ Login data file not found: {_loginDataPath}");
                    return new Login(); // Return empty login
                }

                // Read JSON content
                string jsonString = File.ReadAllText(_loginDataPath);

                // Check if file is empty
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    Debug.WriteLine("⚠️ Login data file is empty");
                    return new Login();
                }

                // Deserialize JSON to Login object
                var logData = JsonSerializer.Deserialize<Login>(jsonString);
                Debug.WriteLine($"✅ Login data loaded from: {_loginDataPath}");
                return logData ?? new Login(); // Return deserialized object or empty if null
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to load login data: {ex.Message}");
                return new Login(); // Return empty on error
            }
        }

        /// <summary>
        /// Loads ride profile statistics from JSON file
        /// Returns empty Rideprofile if file doesn't exist or is invalid
        /// </summary>
        /// <returns>Rideprofile with statistics, or empty Rideprofile if not found</returns>
        public Rideprofile? LoadRideData()
        {
            try
            {
                if (!File.Exists(_rideProfilePath))
                {
                    Debug.WriteLine($"⚠️ Ride profile file not found: {_rideProfilePath}");
                    return new Rideprofile();
                }

                string jsonString = File.ReadAllText(_rideProfilePath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    Debug.WriteLine("⚠️ Ride profile file is empty");
                    return new Rideprofile();
                }

                var profile = JsonSerializer.Deserialize<Rideprofile>(jsonString);
                Debug.WriteLine($"✅ Ride data loaded from: {_rideProfilePath}");
                return profile ?? new Rideprofile();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to load ride data: {ex.Message}");
                return new Rideprofile();
            }
        }

        /// <summary>
        /// Loads user profile information from JSON file
        /// Returns empty User object if file doesn't exist or is invalid
        /// </summary>
        /// <returns>User object with profile data, or empty User if not found</returns>
        public User? LoadUserData()
        {
            try
            {
                if (!File.Exists(_userProfilePath))
                {
                    Debug.WriteLine($"⚠️ User profile file not found: {_userProfilePath}");
                    return new User();
                }

                string jsonString = File.ReadAllText(_userProfilePath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    Debug.WriteLine("⚠️ User profile file is empty");
                    return new User();
                }

                var userData = JsonSerializer.Deserialize<User>(jsonString);
                Debug.WriteLine($"✅ User data loaded from: {_userProfilePath}");
                return userData ?? new User();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to load user data: {ex.Message}");
                return new User();
            }
        }

        /// <summary>
        /// Deletes all saved profile data files (login, ride, user)
        /// Useful for logout, data reset, or testing
        /// Only deletes files that exist - doesn't fail if files are missing
        /// </summary>
        /// <returns>True if all deletions succeeded, false if any error occurred</returns>
        public bool ClearAllData()
        {
            bool success = true;

            try
            {
                // Delete login data if it exists
                if (File.Exists(_loginDataPath))
                {
                    File.Delete(_loginDataPath);
                    Debug.WriteLine($"✅ Deleted login data: {_loginDataPath}");
                }

                // Delete ride profile if it exists
                if (File.Exists(_rideProfilePath))
                {
                    File.Delete(_rideProfilePath);
                    Debug.WriteLine($"✅ Deleted ride data: {_rideProfilePath}");
                }

                // Delete user profile if it exists
                if (File.Exists(_userProfilePath))
                {
                    File.Delete(_userProfilePath);
                    Debug.WriteLine($"✅ Deleted user data: {_userProfilePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to clear data: {ex.Message}");
                success = false;
            }

            return success;
        }
    }
}
