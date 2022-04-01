namespace HeroesAPI.Entitites.Models
{
    public class EmailModel
    {
        public string RecipientEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<IFormFile>? Attachments { get; set; }
    }
}
