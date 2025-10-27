



namespace OneView.Models
{
    public class Rideprofile
    {
        private double _distance;
        private TimeSpan _timeOnBike = TimeSpan.Zero;
        private DateTime _lastTime = DateTime.Now;
        private double _mediumSpeed;
        private double _maxSpeed;
        private double _minInclineAngleLeft;
        private double _maxInclineAngleLeft;
        private double _minInclineAngleRight;
        private double _maxInclineAngleRight;
        private System.Timers.Timer? _ticks;
        private double _speed;
        public System.Timers.Timer? Ticks
        {
            get { return _ticks; }
            set { _ticks = value; }
        }
        public DateTime LastTime
        {
            get { return _lastTime; }
            set { _lastTime = value; }
        }
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
        public double MinInclineAngleLeft
        {
            get { return _minInclineAngleLeft; }
            set { _minInclineAngleLeft = value; }
        }
        public double MaxInclineAngleLeft
        {
            get { return _maxInclineAngleLeft; }
            set { _maxInclineAngleLeft = value; }
        }
        public double MinInclineAngleRight
        {
            get { return _minInclineAngleRight; }
            set { _minInclineAngleRight = value; }
        }
        public double MaxInclineAngleRight
        {
            get { return _maxInclineAngleRight; }
            set { _maxInclineAngleRight = value; }
        }
    }
}
