using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneView.Models;

namespace OneView.Services
{
    public class ProfileService
    {
        public Rideprofile _rideprofile = new Rideprofile();
        public Sensordata _sensordata = new Sensordata();
        public void UpdateDistanceDrive()
        {
            double totalDistance = Speeds(_sensordata.SpeedKmh);
            double time = UpdateTimeOnBike();         //d = s*t (distance = speed * time)
            _rideprofile.Distance = totalDistance; // in km
        }

        public double UpdateTimeOnBike()     //will be caled once on start and last on stop of ride 
        {

            DateTime now = DateTime.Now;

            _rideprofile.TimeOnBike = now - _rideprofile.LastTime;
            _rideprofile.LastTime = now;
            return (double)_rideprofile.TimeOnBike.TotalMinutes;
        }
        private void SpeedsArrays(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Speeds(_sensordata.SpeedKmh);
        }

        private double Speeds(double speed)
        {
            double totalDistance = 0;
            double deltaTime = 1.0 / 60.0;     // every minutes = 1/60 hours
            List<double> speeds = new List<double>();
            speeds.Add(speed);
            foreach (double s in speeds)
            {
                totalDistance += s * deltaTime;

            }
            _rideprofile.MaxSpeed = speeds.Max();

            return totalDistance;
        }
        public void AktuallSpeed(double speed)    // this hier musst be called on startride button later on UI
        {
            _rideprofile.Ticks = new System.Timers.Timer(60000);
            _rideprofile.Ticks.Elapsed += SpeedsArrays;
            _rideprofile.Ticks.AutoReset = true;
            _rideprofile.Ticks.Enabled = true;
            _rideprofile.Speed = speed;
        }
        public void UpdateMediumSpeed()
        {
            double timeMinutes = UpdateTimeOnBike();
            double timeHours = timeMinutes / 60;
            _rideprofile.MediumSpeed = (_rideprofile.Distance / timeHours);
        }
        private double UpdateMaxSpeed(double speed)
        {

            return _rideprofile.MaxSpeed;
        }
    }
}

