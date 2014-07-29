namespace RED.Views.ControlCenter
{
    using ViewModels.ControlCenter;

    public partial class SaveModuleStateView
    {
        public SaveModuleStateView()
        {
            InitializeComponent();

            DataContext = ControlCenterVM.SaveModuleStateVM;
        }
    }
}
