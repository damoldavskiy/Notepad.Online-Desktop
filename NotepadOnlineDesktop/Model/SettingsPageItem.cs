using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotepadOnlineDesktop.Model
{
    public class SettingsPageItem
    {
        public string Header { get; set; }
        public List<SettingsPropertyItem> Properties { get; set; }
    }
}
