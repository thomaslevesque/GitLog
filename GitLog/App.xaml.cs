using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace GitLog
{
    public partial class App
    {
        static App()
        {
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(
                        CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var catalog = new AssemblyCatalog(typeof(App).Assembly);
            var container = new CompositionContainer(catalog);
            var window = container.GetExport<MainWindow>().Value;
            window.ViewModel.Initialize();
            MainWindow = window;
            window.Show();
        }
    }
}
