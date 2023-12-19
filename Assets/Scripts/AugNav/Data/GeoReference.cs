using System;

namespace AugNav.Data
{
    [Serializable]
    public class GeoReference
    {
        public double longitude;
        public double latitude;
        public double altitude;

        public GeoReference(double longitude, double latitude, double altitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.altitude = altitude;
        }
    }
}