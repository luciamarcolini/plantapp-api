using System.ComponentModel.DataAnnotations;

namespace PlantAPI.Models
{
    public class Capture
    {
        [Key]
        public string Id { get; set; }
        public string deviceId { get; set; }
        public bool isDesease { set; get; }
        public string scientific_name { set; get; }
        public string description { set; get; }
        public string deseasesResult { set; get; }
        public string urlImage { set; get; }
    }
}
