using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace OneView.Models
{
    public class Rideprofile
    {
        private double _distance;
        private TimeSpan _timeOnBike = TimeSpan.Zero;
        private DateTime _lastTime = DateTime.Now;
        private double _mediumSpeed;
        private double _maxSpeed;
        private double _minInclineAngle;
        private double _maxInclineAngle;
        private System.Timers.Timer? _ticks;
        private double _speed;
        public double Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        public double Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }
        public TimeSpan TimeOnBike
        {
            get { return _timeOnBike; }
            set { _timeOnBike = value; }
        }
        public double MediumSpeed
        {
            get { return _mediumSpeed; }
            set { _mediumSpeed = value; }

        }
        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }
        public double MinInclineAngle
        {
            get { return _minInclineAngle; }
            set { _minInclineAngle = value; }
        }
        public double MaxInclineAngle 
        {
            get { return _maxInclineAngle; }
            set { _maxInclineAngle = value; }
        }

        public void UpdateDistanceDrive()
        {
            double totalDistance = Speeds(_speed);
            double time = UpdateTimeOnBike();         //d = s*t (distance = speed * time)
            _distance = totalDistance; // in km
        }

        public double UpdateTimeOnBike()     //will be caled once on start and last on stop of ride 
        {

            DateTime now = DateTime.Now;

            _timeOnBike = now - _lastTime;
            _lastTime = now;
            return (double)_timeOnBike.TotalMinutes;
        }
        private void SpeedsArrays(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Speeds(_speed);
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
            _maxSpeed = speeds.Max();

            return totalDistance;
        }
        public void AktuallSpeed(double speed)    // this hier musst be called on startride button later on UI
        {
            _ticks = new System.Timers.Timer(60000);
            _ticks.Elapsed += SpeedsArrays;
            _ticks.AutoReset = true;
            _ticks.Enabled = true;
            _speed = speed;
        }
        public void UpdateMediumSpeed()
        {
            double timeMinutes = UpdateTimeOnBike();
            double timeHours = timeMinutes / 60;
            _mediumSpeed = (_distance / timeHours);
        }
        private double UpdateMaxSpeed(double speed)
        {
           
            return _maxSpeed;
        }
    }
}
