using System.IO;
using OneView.Models;

namespace OneView.Services
{
    public class SaveprofileData
    {
        private readonly string _filePath;
        public SaveprofileData()
        {
            _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OneView", "profiledata.txt");
            var directory = Path.GetDirectoryName(_filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        public void SaveData(Rideprofile profile)
        {
            using (StreamWriter writer = new StreamWriter(_filePath))
            {
                writer.WriteLine(profile.Distance);
                writer.WriteLine(profile.MinInclineAngle);
                writer.WriteLine(profile.MaxInclineAngle);
                writer.WriteLine(profile.TimeOnBike);
                writer.WriteLine(profile.MediumSpeed);
                writer.WriteLine(profile.MaxSpeed);
                
            }

        }
        public Rideprofile LoadData()
        {
            Rideprofile profile = new Rideprofile();
            if (!File.Exists(_filePath))
            {
                return profile;
            }
            using (StreamReader reader = new StreamReader(_filePath))
            {
                profile.Distance = double.Parse(reader.ReadLine() ?? "0");
                profile.MinInclineAngle = double.Parse(reader.ReadLine() ?? "0");
                profile.MaxInclineAngle = double.Parse(reader.ReadLine() ?? "0");
                profile.TimeOnBike = TimeOnly.Parse(reader.ReadLine() ?? "00:00:00");
                profile.MediumSpeed = double.Parse(reader.ReadLine() ?? "0");
                profile.MaxSpeed = double.Parse(reader.ReadLine() ?? "0");
            }
            return profile;
        }

    }
}
