namespace LebAssist.Application.Services
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusKm = 6371;

        /// <summary>
        /// Calculate distance between two points using Haversine formula
        /// </summary>
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Calculate distance using decimal coordinates
        /// </summary>
        public static double CalculateDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            return CalculateDistance((double)lat1, (double)lon1, (double)lat2, (double)lon2);
        }

        /// <summary>
        /// Check if a point is within a given radius
        /// </summary>
        public static bool IsWithinRadius(double lat1, double lon1, double lat2, double lon2, double radiusKm)
        {
            return CalculateDistance(lat1, lon1, lat2, lon2) <= radiusKm;
        }

        /// <summary>
        /// Format distance for display
        /// </summary>
        public static string FormatDistance(double distanceKm)
        {
            if (distanceKm < 1)
            {
                return $"{(distanceKm * 1000):F0} m";
            }
            return $"{distanceKm:F1} km";
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}