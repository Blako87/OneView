using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneView.Models
{
    public class Profile
    {
        private double _distance=0;
        private TimeOnly _timeOnBike;
        private double _mediumSpeed = 0;
        private double _maxSpeed = 0;
        private double _minInclineAngle = 0;
        private double _maxInclineAngle = 0;

        public double Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }
        public TimeOnly TimeOnBike
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
    }
}
