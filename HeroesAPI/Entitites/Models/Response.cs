namespace HeroesAPI.Entitites.Models
{
    public class Response
    {
        public string Status { get; set; } = string.Empty;
        public List<object> Message { get; set; } = new List<object>();
    }
}
