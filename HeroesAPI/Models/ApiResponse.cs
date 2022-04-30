namespace HeroesAPI.Models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<object> Message { get; set; } = new List<object>();
    }
}
