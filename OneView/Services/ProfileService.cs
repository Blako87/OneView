
using OneView.Models;

namespace OneView.Services
{
    public class ProfileService
    {
        public Rideprofile _rideprofile { get; private set; } = new();
        private readonly Sensordata _sensordata = new();


        public void UpdateDistanceDrive()
        {
            double totalDistance = Speeds(_sensordata.SpeedKmh).Item1;
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
        private void SpeedsAndAnglesTimings(object? sender, System.Timers.ElapsedEventArgs e)
        {
            
            var (deltaDistanceKm,intervalMaxSpeedKmh) = Speeds(_sensordata.SpeedKmh);
            _rideprofile.Distance += deltaDistanceKm;
            _rideprofile.MaxSpeed = Math.Max(_rideprofile.MaxSpeed, intervalMaxSpeedKmh);

            var (minL,maxL,minR,maxR) = InclineAngles(_sensordata.InclineAngleDegLeft, _sensordata.InclineAngleDegRight);
            _rideprofile.MinInclineAngleLeft = minL;
            _rideprofile.MaxInclineAngleLeft = maxL;
            _rideprofile.MinInclineAngleRight = minR;
            _rideprofile.MaxInclineAngleRight = maxR;
        }

        private static (double, double) Speeds(double speed)
        {
            double totalDistance = 0;
            double deltaTime = 1.0 / 60.0;     // every minutes = 1/60 hours
            List<double> speeds = new List<double>();
            speeds.Add(speed);
            foreach (double s in speeds)
            {
                totalDistance += s * deltaTime;

            }
            double maxSpeed = speeds.Max();

            return (totalDistance, maxSpeed);
        }
        public void AktuallSpeed(double speed)    // this hier musst be called on startride button later on UI
        {
            _rideprofile.Ticks = new System.Timers.Timer(60000);
            _rideprofile.Ticks.Elapsed += SpeedsAndAnglesTimings;
            _rideprofile.Ticks.AutoReset = true;
            _rideprofile.Ticks.Enabled = true;
            _rideprofile.Speed = speed;
        }
        public void UpdateMediumSpeed()
        {
            double timeMinutes = UpdateTimeOnBike();
            double timeHours = timeMinutes / 60;
            _rideprofile.MediumSpeed = _rideprofile.Distance / timeHours;
        }
        
        private static (double, double,double,double) InclineAngles(double inclineAngleLeft, double inclineAngleRight)
        {
            double minInclineAngleDegLeft;
            double maxInclineAngleDegLeft;
            double minInclineAngleDegRight;
            double maxInclineAngleDegRight;

            List<double> inclineAngleDegLeft = new List<double>();
            inclineAngleDegLeft.Add(inclineAngleLeft);
            List<double> inclineAngleDegRight = new List<double>();
            inclineAngleDegRight.Add(inclineAngleRight);
            minInclineAngleDegLeft = inclineAngleDegLeft.Min();
            maxInclineAngleDegLeft = inclineAngleDegLeft.Max();
            minInclineAngleDegRight = inclineAngleDegRight.Min();
            maxInclineAngleDegRight = inclineAngleDegRight.Max();

            return (minInclineAngleDegLeft,maxInclineAngleDegLeft,minInclineAngleDegRight,maxInclineAngleDegRight);
        }
       
    }
}

