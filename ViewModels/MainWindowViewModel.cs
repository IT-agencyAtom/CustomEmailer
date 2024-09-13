using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.Input;
using CustomEmailer.Models;
using CustomEmailer.Settings;
using MailKit.Net.Smtp;
using MimeKit;
using Path = System.IO.Path;

namespace CustomEmailer.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly SettingsManager<AppSettings> _settingsManager;
        private AppSettings _appSettings;
        public ObservableCollection<Client> Clients { get; } = new();
        public string? Subject { get; set; }
        public string? Body { get; set; }

        public MainWindowViewModel()
        {
            _settingsManager = new SettingsManager<AppSettings>("appsettings.json");
            _appSettings = _settingsManager.LoadSettings() ?? new AppSettings();
        }

        [RelayCommand]
        public async void OpenFile(Window parent)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(parent);

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose Excel File",
                FileTypeFilter = new List<FilePickerFileType>
                    { new("Excel Files") { Patterns = new List<string> { "*.xls", "*.xlsx" } } },
                AllowMultiple = false
            });

            if (files.Count != 1) return;

            using (var workbook = new XLWorkbook(files[0].Path.AbsolutePath))
            {
                const int colSurname = 1;
                const int colPrefix = 2;
                const int colEmail = 3;
                const int colAttachment = 4;

                Clients.Clear();

                var ws = workbook.Worksheets.First();

                // Look for the first row used
                var firstRowUsed = ws.FirstRowUsed();

                // Narrow down the row so that it only includes the used part
                var clientRow = firstRowUsed.RowUsed();

                // Move to the next row (it now has the titles)
                clientRow = clientRow.RowBelow();

                // Get all clients
                while (!clientRow.Cell(colSurname).IsEmpty())
                {
                    Clients.Add(new Client
                    {
                        Surname = clientRow.Cell(colSurname).GetString(),
                        Prefix = clientRow.Cell(colPrefix).GetString(),
                        Email = clientRow.Cell(colEmail).GetString(),
                        Attachment = Path.Combine(_appSettings.AttachmentsFolder, clientRow.Cell(colAttachment).GetString())
                    });

                    clientRow = clientRow.RowBelow();
                }
            }
        }

        [RelayCommand]
        public void SendCommand()
        {
            foreach (var client in Clients)
            {
                if (!client.IsSelected) continue;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_appSettings.SenderName, _appSettings.Username));
                message.To.Add(new MailboxAddress($"{client.Prefix} {client.Surname}", client.Email));
                message.Subject = Subject;

                var builder = new BodyBuilder
                {
                    TextBody = CreateEmailBody(client)
                };
                builder.Attachments.Add(client.Attachment);

                message.Body = builder.ToMessageBody();

                using var smtpClient = new SmtpClient();
                smtpClient.Connect(_appSettings.SmtpServer, int.Parse(_appSettings.SmtpPort), false);
                smtpClient.Authenticate(_appSettings.Username, _appSettings.Password);
                smtpClient.Send(message);
                smtpClient.Disconnect(true);
            }
        }

        private string CreateEmailBody(Client client)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendLine($"Dear {client.Prefix} {client.Surname},");
            emailBody.AppendLine(Body);

            return emailBody.ToString();
        }

        [RelayCommand]
        public async void OpenSettings(Window parent)
        {
            var vm = new SettingsDialogViewModel(_settingsManager);
            var dialog = new SettingsDialog
            {
                DataContext = vm
            };
            vm.OnRequestClose += (s, e) =>
            {
                dialog.Close();
                _appSettings = _settingsManager.LoadSettings() ?? new AppSettings();
            };
            await dialog.ShowDialog(parent);
        }

        [RelayCommand]
        public void Exit()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime) 
                lifetime.Shutdown();
        }
    }
}
