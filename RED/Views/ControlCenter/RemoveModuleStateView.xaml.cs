namespace RED.Views.ControlCenter
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels.ControlCenter;

    public partial class RemoveModuleStateView
    {
        public RemoveModuleStateView()
        {
            InitializeComponent();

            DataContext = ControlCenterVM.RemoveModuleStateVM;
        }
    }
}
