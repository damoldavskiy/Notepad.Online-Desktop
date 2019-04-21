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

using static DataBase.ReturnCodeDescriptions;

namespace CloudExtension
{
    [Export(typeof(IExtension))]
    public class Main : IExtension
    {
        public string Name { get; } = Resources.Name;
        public string Version { get; } = "1.0";
        public string Author { get; } = "DMSoft";
        public string Description { get; } = Resources.Info;

        public List<MenuItem> Menu
        {
            get
            {
                return new List<MenuItem>()
                {
                    openFolder,
                    update,
                    saveInCloud,
                    delete,
                    properties
                };
            }
        }

        MenuItem openFolder;
        MenuItem update;
        MenuItem saveInCloud;
        MenuItem delete;
        MenuItem properties;
        IApplicationInstance app;

        public Main()
        {
            saveInCloud = new MenuItem() { Header = Resources.SaveInCloud };
            saveInCloud.Click += SaveInCloud_Click;
            delete = new MenuItem() { Header = Resources.DeleteFile };
            delete.Click += Delete_Click;
            openFolder = new MenuItem() { Header = Resources.OpenCloudFolder };
            openFolder.Click += OpenFolder_Click;
            update = new MenuItem() { Header = Resources.UpdateFiles };
            update.Click += Update_Click;
            properties = new MenuItem() { Header = Resources.Properties };
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
                MessageBox.Show(Resources.LogInFailed + ". " + result.GetDescription(), Resources.Name);
                return;
            }

            await UpdateFolderAsync();
        }

        public async Task OnStop()
        {
            await Task.CompletedTask;
        }

        async void SaveInCloud_Click(object sender, RoutedEventArgs e)
        {
            if (DataBase.Manager.Status != DataBase.ManagerStatus.Ready)
            {
                MessageBox.Show(Resources.SignInFirstly, Resources.FileNotSaved);
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
            
            SaveToFolder(name, app.Text);
            await SaveToCloudAsync(name, app.Text);
        }

        void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (app.Name == null || !app.Name.StartsWith(Settings.Default.path) || !app.Name.EndsWith(".txt"))
            {
                MessageBox.Show(Resources.OpenFromCloudToOperate, Resources.IllegalFile);
                return;
            }
            var name = app.Name.Substring(Settings.Default.path.Length);
            name = name.Substring(0, name.Length - 4);
            var result = DataBase.Manager.DelData(name);
            if (result != DataBase.ReturnCode.Success)
            {
                MessageBox.Show(Resources.FileNotDeleted + ". " + result.GetDescription(), Resources.DeletingFailed);
                return;
            }
            File.Delete(app.Name);
            MessageBox.Show(Resources.FileDeleted, Resources.Success);
        }

        void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            app.OpenFolder(Settings.Default.path);
        }

        async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (DataBase.Manager.Status != DataBase.ManagerStatus.Ready)
            {
                MessageBox.Show(Resources.SignInFirstly, Resources.UpdatingFailed);
                return;
            }
            
            var result = DataBase.Manager.GetNames();

            if (result.Item1 != DataBase.ReturnCode.Success)
                MessageBox.Show(Resources.FilesNotUpdated + ". " + result.Item1.GetDescription(), Resources.UpdatingFailed);

            await UpdateFolderAsync();

            MessageBox.Show(Resources.FilesUpdated, Resources.Success);
        }

        void Properties_Click(object sender, RoutedEventArgs e)
        {
            new PropertiesWindow().ShowDialog();
        }

        async Task UpdateFolderAsync()
        {
            ClearFolder();

            var names = (await DataBase.Manager.GetNamesAsync()).Item2;
            foreach (var name in names)
                SaveToFolder(name, (await DataBase.Manager.GetDataAsync(name)).Item3);
        }

        void SaveToFolder(string name, string text)
        {
            File.WriteAllText(Settings.Default.path + name + ".txt", text, Encoding.UTF8);
        }

        async Task SaveToCloudAsync(string name, string text)
        {
            var result = await DataBase.Manager.AddDataAsync(name, Resources.Name, app.Text);

            if (result == DataBase.ReturnCode.Success)
                MessageBox.Show(Resources.FileInCloud + ": " + name, Resources.Success);
            else if (result == DataBase.ReturnCode.DataAlreadyExists)
            {
                if (MessageBox.Show(Resources.DoYouWantToReplace + $" \"{name}\"?", Resources.ReplacingFile, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    result = await DataBase.Manager.EditDescriptionAsync(name, Resources.Name);
                    if (result == DataBase.ReturnCode.Success)
                        result = await DataBase.Manager.EditTextAsync(name, app.Text);

                    if (result != DataBase.ReturnCode.Success)
                        MessageBox.Show(Resources.FileNotSaved + ". " + result.GetDescription(), Resources.FileNotSaved);
                }
            }
            else
                MessageBox.Show(Resources.FileNotSaved + ". " + result.GetDescription(), Resources.FileNotSaved);
        }

        private void ClearFolder()
        {
            foreach (var file in new DirectoryInfo(Settings.Default.path).GetFiles())
                file.Delete();
        }
    }
}
