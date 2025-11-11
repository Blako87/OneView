using System.Text.Json;
using System.Diagnostics;
using OneView.Models;

namespace OneView.Services
{
    public class SaveprofileData
    {
        private readonly string _rideProfilePath;
        private readonly string _userProfilePath;
        private readonly string _loginDataPath;
        private readonly string _dataDirectory;

        public SaveprofileData()
        {
            _dataDirectory = FileSystem.AppDataDirectory;
            _rideProfilePath = Path.Combine(_dataDirectory, "rideprofile.json");
            _userProfilePath = Path.Combine(_dataDirectory, "userprofile.json");
            _loginDataPath = Path.Combine(_dataDirectory, "logindata.json");

            // Ensure data directory exists
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

        public bool SaveLoginData(Login logData)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(logData, options);
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

        public bool SaveRideData(Rideprofile profile)
        {
            try
            {
                // Create options to ignore non-serializable properties
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

        public Login? LoadLoginData()
        {
            try
            {
                if (!File.Exists(_loginDataPath))
                {
                    Debug.WriteLine($"⚠️ Login data file not found: {_loginDataPath}");
                    return new Login();
                }

                string jsonString = File.ReadAllText(_loginDataPath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    Debug.WriteLine("⚠️ Login data file is empty");
                    return new Login();
                }

                var logData = JsonSerializer.Deserialize<Login>(jsonString);
                Debug.WriteLine($"✅ Login data loaded from: {_loginDataPath}");
                return logData ?? new Login();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to load login data: {ex.Message}");
                return new Login();
            }
        }

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
        /// Deletes all saved profile data files
        /// </summary>
        public bool ClearAllData()
        {
            bool success = true;

            try
            {
                if (File.Exists(_loginDataPath))
                {
                    File.Delete(_loginDataPath);
                    Debug.WriteLine($"✅ Deleted login data: {_loginDataPath}");
                }

                if (File.Exists(_rideProfilePath))
                {
                    File.Delete(_rideProfilePath);
                    Debug.WriteLine($"✅ Deleted ride data: {_rideProfilePath}");
                }

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
