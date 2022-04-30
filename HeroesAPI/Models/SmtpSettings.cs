namespace HeroesAPI.Models
{
    public class SmtpSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public int SenderName { get; set; }
        public string SenderMail { get; set; }
        public string Password { get; set; }
    }
}
