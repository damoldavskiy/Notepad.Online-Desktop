using System.Collections.ObjectModel;

namespace NotepadOnlineDesktop.ViewModel
{
    public class ManagerWindow
    {
        public ObservableCollection<Model.ManagedExtension> Extensions
        {
            get
            {
                return Model.ExtensionManager.Extensions;
            }
        }

        public Model.ManagedExtension SelectedExtension { get; set; }

        public Model.ActionCommand Enable
        {
            get
            {
                return new Model.ActionCommand(sender =>
                {
                    if (SelectedExtension?.Status == Model.ExtensionStatus.Disabled)
                        SelectedExtension.Enable();
                });
            }
        }

        public Model.ActionCommand Disable
        {
            get
            {
                return new Model.ActionCommand(sender =>
                {
                    if (SelectedExtension?.Status == Model.ExtensionStatus.Enabled)
                        SelectedExtension.Disable();
                });
            }
        }
    }
}
