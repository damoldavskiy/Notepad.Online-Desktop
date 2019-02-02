using NotepadOnlineDesktopExtensions;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows.Controls;

namespace NotepadOnlineDesktop.Model
{
    public class ExtensionManager
    {
        public static ObservableCollection<ManagedExtension> Extensions { get; private set; }

        public static MenuItem ParentMenu { get; private set; }

        [ImportMany]
        private static List<IExtension> loadedExtensions { get; set; }

        public static void Load(string path)
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path));

            var container = new CompositionContainer(catalog);
            loadedExtensions = new List<IExtension>(container.GetExportedValues<IExtension>());
        }

        public static void Initialize(IApplicationInstance instance, MenuItem parent)
        {
            ParentMenu = parent;

            Extensions = new ObservableCollection<ManagedExtension>();
            foreach (var extension in loadedExtensions)
            {
                var menu = new MenuItem
                {
                    Header = extension.Name,
                    Visibility = System.Windows.Visibility.Collapsed,
                    Template = (ControlTemplate)App.Current.Resources["SubRootMenuItemTemplate"]
                };
                var curExt = new ManagedExtension(extension, instance, menu);

                foreach (var item in extension.Menu)
                    menu.Items.Add(item);
                parent.Items.Add(menu);

                Extensions.Add(curExt);

                var prop = Properties.Settings.Default;

                if (prop.extNames == null)
                    prop.extNames = new System.Collections.Specialized.StringCollection();
                if (prop.extStatus == null)
                    prop.extStatus = new System.Collections.Specialized.StringCollection();

                if (prop.extNames.Contains(curExt.Name))
                {
                    if (prop.extStatus[prop.extNames.IndexOf(curExt.Name)] == "true")
                        curExt.Enable();
                }
                else
                {
                    prop.extNames.Add(curExt.Name);
                    prop.extStatus.Add("false");
                    prop.Save();
                }
            }
        }
    }
}
