namespace HomeBlaze.Abstractions
{
    public record GeoCoordinate
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public override string ToString()
        {
            return $"({Latitude}, {Longitude})";
        }
    }
}
