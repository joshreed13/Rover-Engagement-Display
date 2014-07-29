namespace RED
{
	using System.Windows;
	using Views;
	using Views.ControlCenter;

	public partial class App
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			//Disable shutdown when the dialog closes
			Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

			var dialog = new StartupStatusView();
			dialog.Show();

			var mainWindow = new ControlCenterView();

			//Re-enable normal shutdown mode.
			Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
			Current.MainWindow = mainWindow;

			dialog.Close();
			mainWindow.Show();
		}
	}
}
