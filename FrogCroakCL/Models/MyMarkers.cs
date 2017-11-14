using System.Collections.Generic;

namespace FrogCroakCL.Models
{
    public class MyMarkers
    {
        public List<MyMarker> MarkerList { get; set; }
        public class MyMarker
        {
            public int MarkerId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
        }
    }
}