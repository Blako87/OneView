using System.Text.Json;
using OneView.Models;

namespace OneView.Services
{
    public class SaveprofileData
    {
        private readonly string _rideProfilePath;
        private readonly string _userProfilePath;
        private readonly string _loginDataPath;
        public SaveprofileData()
        {
            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OneView" );
            
            if (!string.IsNullOrEmpty(directory) &&!Directory.Exists(directory))
            {
               _ = Directory.CreateDirectory(directory);
            }
            _rideProfilePath = Path.Combine(directory, "rideprofile.json");
            _userProfilePath = Path.Combine(directory, "userprofile.json");
            _loginDataPath = Path.Combine(directory, "logindata.json");
        }
        public void SaveLoginData(Login logData)
        {
            string jsonString = JsonSerializer.Serialize(logData);
            File.WriteAllText(_loginDataPath, jsonString);
        }
        public void SaveRideData(Rideprofile profile)
        {
            string jsonString = JsonSerializer.Serialize(profile);
            File.WriteAllText(_rideProfilePath, jsonString);
        }
        public void SaveUserData(User user)
        {
            string JsonString = JsonSerializer.Serialize(user);
            File.WriteAllText(_userProfilePath, JsonString);
        }
        public Login LoadLoginData()
        {
            Login logData = new();
            if (!File.Exists(_loginDataPath))
            {
                return logData;
            }
            string jsonString = File.ReadAllText(_loginDataPath);
            logData = JsonSerializer.Deserialize<Login>(jsonString)!;
            return logData;
        }
        public Rideprofile LoadRideData()
        {
            Rideprofile profile = new();
            if (!File.Exists(_rideProfilePath))
            {
                return profile;
            }
            string JsonString = File.ReadAllText(_rideProfilePath);
            profile = JsonSerializer.Deserialize<Rideprofile>(JsonString)!;
           
            return profile;
        }
        public User LoadUserData()
        {
            User userData = new();
            if (!File.Exists(_userProfilePath))
            {
                return userData;
            }
            string jsonString = File.ReadAllText(_userProfilePath);
            userData = JsonSerializer.Deserialize<User>(jsonString)!;
            return userData;
        }

    }
}
