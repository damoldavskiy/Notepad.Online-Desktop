using NotepadOnlineDesktopExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
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
        string cloudPath;

        public Main()
        {
            saveInCloud = new MenuItem() { Header = "Save in cloud" };
            saveInCloud.Click += SaveInCloud_Click;
            openFolder = new MenuItem() { Header = "Open cloud folder" };
            openFolder.Click += OpenFolder_Click;
            update = new MenuItem() { Header = "Update" };
            update.Click += Update_Click;
            properties = new MenuItem() { Header = "Properties" };
            properties.Click += Properties_Click;
        }

        public async Task OnStart(IApplicationInstance instance)
        {
            app = instance;
            var properties = Properties.Settings.Default;

            if (!Directory.Exists(properties.path))
            {
                properties.path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\NotepadOnline\";
                Directory.CreateDirectory(properties.path);
            }
            cloudPath = properties.path;

            var authResult = await DataBase.Manager.AuthorizeAsync(properties.login, properties.password, properties.token);
            if (authResult != DataBase.ReturnCode.Success)
            {
                MessageBox.Show("Message: " + authResult, "Cloud Extension: not authorized");
                return;
            }

            foreach (var file in new DirectoryInfo(cloudPath).GetFiles())
                file.Delete();

            var names = DataBase.Manager.GetNames().Item2;
            foreach (var name in names)
                using (var stream = new StreamWriter(cloudPath + name, false, Encoding.Default))
                {
                    stream.Write(DataBase.Manager.GetData(name).Item3);
                }
        }

        public async Task OnStop()
        {
        }

        private void SaveInCloud_Click(object sender, RoutedEventArgs e)
        {
            if (app.Name == null)
            {
                MessageBox.Show("Save the file to disk firstly", "File not saved");
                return;
            }
            if (!DataBase.Manager.Authorized)
            {
                MessageBox.Show("Authorize firstly", "File not saved");
                return;
            }

            var name = app.Name.Substring(app.Name.LastIndexOf("\\") + 1);

            using (var stream = new StreamWriter(cloudPath + name, false, Encoding.Default))
            {
                stream.Write(app.Text);
            }
            
            var result = DataBase.Manager.AddData(name, "Cloud Extension", app.Text);

            if (result == DataBase.ReturnCode.Success)
                MessageBox.Show("The file now in the cloud: " + name, "Saving successful");
            else if (result == DataBase.ReturnCode.DataAlreadyExists)
            {
                if (MessageBox.Show($"Do you want to replace file \"{name}\"?", "Replacing file", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var res = DataBase.Manager.EditDescription(name, "Cloud Extension");
                    DataBase.Manager.EditText(name, app.Text);

                    if (res != DataBase.ReturnCode.Success)
                        MessageBox.Show($"Error: {res}", "Saving failed");
                }
            }
            else
                MessageBox.Show($"Error: {result}", "Saving failed");
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            app.OpenFolder(cloudPath);
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (!DataBase.Manager.Authorized)
            {
                MessageBox.Show("Authorize firstly", "File not saved");
                return;
            }

            var result = DataBase.Manager.GetNames();

            if (result.Item1 != DataBase.ReturnCode.Success)
                MessageBox.Show($"Error: {result.Item1}", "Updating failed");

            foreach (var file in new DirectoryInfo(cloudPath).GetFiles())
                file.Delete();

            foreach (var name in result.Item2)
                using (var stream = new StreamWriter(cloudPath + name, false, Encoding.Default))
                {
                    stream.Write(DataBase.Manager.GetData(name).Item3);
                }

            MessageBox.Show("All files are now updated", "Updating successful");
        }

        private void Properties_Click(object sender, RoutedEventArgs e)
        {
            new PropertiesWindow().ShowDialog();
        }
    }
}
