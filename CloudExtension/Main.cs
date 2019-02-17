using CloudExtension.Properties;

using NotepadOnlineDesktopExtensions;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CloudExtension
{
    [Export(typeof(IExtension))]
    public class Main : IExtension
    {
        public string Name { get; } = "Cloud Database";
        public string Version { get; } = "1.0";
        public string Author { get; } = "DMSoft";
        public string Description { get; } = "Extension allows user to use Notepad.Online Cloud to keep files";

        public List<MenuItem> Menu
        {
            get
            {
                return new List<MenuItem>()
                {
                    saveInCloud,
                    openFolder,
                    update,
                    properties
                };
            }
        }

        MenuItem saveInCloud;
        MenuItem openFolder;
        MenuItem update;
        MenuItem properties;
        IApplicationInstance app;

        public Main()
        {
            saveInCloud = new MenuItem() { Header = "Save in cloud" };
            saveInCloud.Click += SaveInCloud_Click;
            openFolder = new MenuItem() { Header = "Open cloud folder" };
            openFolder.Click += OpenFolder_Click;
            update = new MenuItem() { Header = "Update files" };
            update.Click += Update_Click;
            properties = new MenuItem() { Header = "Properties" };
            properties.Click += Properties_Click;
        }

        public async Task OnStart(IApplicationInstance instance)
        {
            app = instance;

            if (!Directory.Exists(Settings.Default.path))
            {
                Settings.Default.path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\NotepadOnline\";
                Directory.CreateDirectory(Settings.Default.path);
            }

            if (Settings.Default.email.Length == 0)
                return;

            var result = await DataBase.Manager.LoginAsync(Settings.Default.email, Settings.Default.password, Settings.Default.token);
            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show("Log in failed: " + result, "Cloud Extension");
                return;
            }

            await UpdateFolderAsync();
        }

        public async Task OnStop()
        { }

        private async void SaveInCloud_Click(object sender, RoutedEventArgs e)
        {
            if (DataBase.Manager.Status != DataBase.ManagerStatus.Ready)
            {
                MessageBox.Show("Sign in firstly", "File not saved");
                return;
            }

            string name;

            if (app.Name != null)
            {
                name = app.Name.Substring(app.Name.LastIndexOf("\\") + 1);
                name = name.Substring(0, name.LastIndexOf('.'));
            }
            else
            {
                var input = new InputNameWindow();
                input.ShowDialog();
                if (!input.Canceled)
                    name = input.Text;
                else
                    return;
            }
            
            await SaveToFolderAsync(name, app.Text);
            await SaveToCloudAsync(name, app.Text);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            app.OpenFolder(Settings.Default.path);
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (DataBase.Manager.Status != DataBase.ManagerStatus.Ready)
            {
                MessageBox.Show("Sign in firstly", "Updating failed");
                return;
            }
            
            var result = DataBase.Manager.GetNames();

            if (result.Item1 != DataBase.ReturnCode.Success)
                MessageBox.Show($"Error: {result.Item1}", "Updating failed");

            await UpdateFolderAsync();

            MessageBox.Show("All files are now updated", "Updating successful");
        }

        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            new PropertiesWindow().ShowDialog();
        }

        private async Task UpdateFolderAsync()
        {
            ClearFolder();

            var names = (await DataBase.Manager.GetNamesAsync()).Item2;
            foreach (var name in names)
                await SaveToFolderAsync(name, (await DataBase.Manager.GetDataAsync(name)).Item3);
        }

        private async Task SaveToFolderAsync(string name, string text)
        {
            using (var stream = new StreamWriter(Settings.Default.path + name + ".txt", false, Encoding.Default))
            {
                await stream.WriteAsync(text);
            }
        }

        private async Task SaveToCloudAsync(string name, string text)
        {
            var result = await DataBase.Manager.AddDataAsync(name, "Desktop Extension", app.Text);

            if (result == DataBase.ReturnCode.Success)
                MessageBox.Show("The file now in the cloud: " + name, "Saving successful");
            else if (result == DataBase.ReturnCode.DataAlreadyExists)
            {
                if (MessageBox.Show($"Do you want to replace file \"{name}\"?", "Replacing file", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    result = await DataBase.Manager.EditDescriptionAsync(name, "Desktop Extension");
                    if (result == DataBase.ReturnCode.Success)
                        result = await DataBase.Manager.EditTextAsync(name, app.Text);

                    if (result != DataBase.ReturnCode.Success)
                        MessageBox.Show($"Error: {result}", "Saving failed");
                }
            }
            else
                MessageBox.Show($"Error: {result}", "Saving failed");
        }

        private void ClearFolder()
        {
            foreach (var file in new DirectoryInfo(Settings.Default.path).GetFiles())
                file.Delete();
        }
    }
}
