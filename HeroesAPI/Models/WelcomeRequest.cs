namespace HeroesAPI.Models
{
    public class WelcomeRequest
    {
        public string ToEmail { get; set; }
        public string UserName { get; set; }
        public string ConfirmationCode { get; set; }
        public UriBuilder UriBuilder { get; set; }
    }
}
