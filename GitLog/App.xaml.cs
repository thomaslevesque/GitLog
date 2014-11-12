using System.ComponentModel.Composition.Hosting;
using System.Windows;

namespace GitLog
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var catalog = new AssemblyCatalog(typeof(App).Assembly);
            var container = new CompositionContainer(catalog);
            var window = container.GetExport<MainWindow>().Value;
            MainWindow = window;
            window.Show();
        }
    }
}
