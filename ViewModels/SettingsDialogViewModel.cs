using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using CustomEmailer.Settings;
using System;

namespace CustomEmailer.ViewModels
{
    public partial class SettingsDialogViewModel(SettingsManager<AppSettings> settingsManager) : ViewModelBase
    {
        public AppSettings AppSettings { get; set; } = settingsManager.LoadSettings() ?? new AppSettings();

        public event EventHandler OnRequestClose;

        [RelayCommand]
        public async void SelectAttachmentsFolder(Window parent)
        {
            var topLevel = TopLevel.GetTopLevel(parent);

            var attachmentsFolder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Choose Folder With Attachments",
                AllowMultiple = false
            });

            if (attachmentsFolder.Count != 1) return;

            AppSettings.AttachmentsFolder = attachmentsFolder[0].Path.AbsolutePath;
            //RaisePropertyChanged(nameof(AppSettings));
        }

        [RelayCommand]
        public void Save()
        {
            settingsManager.SaveSettings(AppSettings);
            OnRequestClose(this, EventArgs.Empty);
        }
    }
}
