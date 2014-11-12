using System.ComponentModel.Composition;

namespace GitLog
{
    [Export]
    public partial class MainWindow
    {
        [Import]
        public MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel) DataContext; }
            set { DataContext = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
