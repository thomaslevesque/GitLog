using System.ComponentModel.Composition;
using System.Windows.Input;
using GitLog.Services;

namespace GitLog
{
    [Export]
    public class MainWindowViewModel : ObservableBase
    {
        [Import]
        public IFilePickerService FilePicker { get; set; }

        private RepositoryViewModel _repository;
        public RepositoryViewModel Repository
        {
            get { return _repository; }
            set { Set(ref _repository, value); }
        }

        public ICommand OpenRepositoryCommand
        {
            get
            {
                return new DelegateCommand(OpenRepository);
            }
        }

        private void OpenRepository()
        {
            string current = null;
            if (Repository != null)
                current = Repository.Path;
            string path = FilePicker.PickFolder(current);
            if (path != null)
            {
                Repository = new RepositoryViewModel(path);
            }
        }
    }
}
