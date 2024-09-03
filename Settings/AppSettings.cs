using CommunityToolkit.Mvvm.ComponentModel;

namespace CustomEmailer.Settings
{
    public partial class AppSettings : ObservableObject
    {
        public string SenderName { get; set; }
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public string SmtpPort { get; set; } = "587";
        public string Username { get; set; }
        public string Password { get; set; }
        [ObservableProperty]
        public string _attachmentsFolder;
    }
}
