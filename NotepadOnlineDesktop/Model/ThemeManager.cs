using System;
using System.Windows;

namespace NotepadOnlineDesktop.Model
{
    public static class ThemeManager
    {
        public static void Update()
        {
            switch (Properties.Settings.Default.theme)
            {
                case "light": SetLightTheme(); break;
                case "dark": SetDarkTheme(); break;
            }
        }

        static void SetDarkTheme()
        {
            Clear();
            Add("Dark");
            Add("Common");
        }

        static void SetLightTheme()
        {
            Clear();
            Add("Light");
            Add("Common");
        }

        static void Clear()
        {
            Application.Current.Resources.Clear();
        }

        static void Add(string name)
        {
            var uri = new Uri($@"Themes\{name}.xaml", UriKind.Relative);
            var dict = (ResourceDictionary)Application.LoadComponent(uri);
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}
