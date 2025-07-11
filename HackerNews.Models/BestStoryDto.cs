
using System.Text.Json.Serialization;

namespace HackerNews.Models
{
    public class BestStoryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("url")]
        public string Uri { get; set; } = string.Empty;
        
        [JsonPropertyName("author")]
        public string PostedBy { get; set; } = string.Empty;
        
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }
        
        [JsonPropertyName("score")]
        public int Score { get; set; }
        
        [JsonPropertyName("comments")]
        public int CommentCount { get; set; }
    }
}
