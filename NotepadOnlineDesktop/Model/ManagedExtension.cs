using NotepadOnlineDesktopExtensions;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace NotepadOnlineDesktop.Model
{
    public class ManagedExtension : INotifyPropertyChanged
    {
        IExtension extension;
        IApplicationInstance instance;
        MenuItem menu;

        ExtensionStatus status = ExtensionStatus.Disabled;

        public ManagedExtension(IExtension extension, IApplicationInstance instance, MenuItem menu)
        {
            this.extension = extension;
            this.instance = instance;
            this.menu = menu;
        }

        public ExtensionStatus Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

        public string Name => extension.Name;

        public string Version => extension.Version;

        public string Author => extension.Author;

        public string Description => extension.Description;

        public async void Enable()
        {
            if (Status == ExtensionStatus.Disabled)
            {
                Status = ExtensionStatus.Loading;
                await extension.OnStart(instance);
                Status = ExtensionStatus.Enabled;
                menu.Visibility = System.Windows.Visibility.Visible;
                
                var prop = Properties.Settings.Default;
                prop.extStatus[prop.extNames.IndexOf(Name)] = "true";
                prop.Save();
            }
            else
                throw new Exception();
        }

        public async void Disable()
        {
            if (Status == ExtensionStatus.Enabled)
            {
                Status = ExtensionStatus.Loading;
                await extension.OnStop();
                Status = ExtensionStatus.Disabled;
                menu.Visibility = System.Windows.Visibility.Collapsed;

                var prop = Properties.Settings.Default;
                prop.extStatus[prop.extNames.IndexOf(Name)] = "false";
                prop.Save();
            }
            else
                throw new Exception();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
