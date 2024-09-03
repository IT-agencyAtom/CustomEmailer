using System.ComponentModel;

namespace CustomEmailer.Models
{
    public class Client
    {
        public bool IsSelected { get; set; } = true;
        public string Surname { get; set; }
        public string Prefix { get; set; }
        public string Email { get; set; }
        public string Attachment { get; set; }
    }
}
