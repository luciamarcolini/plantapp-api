namespace PlantAPI.Models
{
    public class Desease
    {
        public bool isDesease { set; get; }
        public string scientific_name { set; get; }
        public string description { set; get; }
        public List<DeseaseDetails> deseases { set; get; }
    }

    public class DeseaseDetails
    {
        public string? deseaseName { set; get; }
        public string? deseaseDescription { set; get; }
        public string? deseaseScientificName { set; get; }
        public string? solution { set; get; }
        public string? fertilizer { set; get; }

    }
}
