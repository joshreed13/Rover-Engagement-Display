namespace RED
{
	using System.Windows;
	using Views.ControlCenter;

	public partial class App
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var controlCenter = new ControlCenterView();
			controlCenter.Show();
		}
	}
}
