using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace PlantAPI.Models
{
    public class OpenAIRequest
    {
        public string model { get; set; }
        public List<object> messages { get; set; }
        public int max_tokens { get; set; }
        public object response_format { set; get; }

        public class Content
        {
            public string type { get; set; }
            public string text { get; set; }
            public ImageUrl image_url { get; set; }
        }

        public class ImageUrl
        {
            public string url { get; set; }
        }
    }
}
