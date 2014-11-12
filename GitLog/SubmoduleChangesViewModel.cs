using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitLog
{
    public abstract class SubmoduleChangesViewModelBase : ObservableBase
    {
        public abstract string Name { get; }
        public abstract IList<CommitViewModelBase> Commits { get; }
    }

    public class SubmoduleChangesViewModel : SubmoduleChangesViewModelBase
    {
        private readonly string _parentRepoPath;
        private readonly string _since;
        private readonly string _until;

        public SubmoduleChangesViewModel(Submodule submodule, string parentRepoPath, string since, string until)
        {
            _parentRepoPath = parentRepoPath;
            _name = submodule.Name;
            _path = submodule.Path;
            _since = since;
            _until = until;
        }

        private readonly string _name;
        public override string Name
        {
            get { return _name; }
        }

        private readonly string _path;
        public string Path
        {
            get { return _path; }
        }

        private static readonly CommitViewModelBase[] _preLoadCommits = {new DummyCommitViewModel()};
        private IList<CommitViewModelBase> _commits;
        public override IList<CommitViewModelBase> Commits
        {
            get
            {
                if (_commits == null && IsExpanded)
                    _commits = LoadCommits();

                return _commits ?? _preLoadCommits;
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (Set(ref _isExpanded, value) && value)
                    OnPropertyChanged("Commits");
            }
        }

        private IList<CommitViewModelBase> LoadCommits()
        {
            var filter = new CommitFilter {Since = _since, Until = _until};
            string fullPath = System.IO.Path.Combine(_parentRepoPath, _path);
            using (var subRepo = new Repository(fullPath))
            {
                var subPaths = new HashSet<string>(subRepo.Submodules.Select(s => System.IO.Path.Combine(fullPath, s.Path)));
                return subRepo.Commits.QueryBy(filter)
                    .Select(c => new CommitViewModel(c, fullPath, subPaths))
                    .ToList<CommitViewModelBase>();
            }
        }
    }

    public class DummySubmoduleChangesViewModel : SubmoduleChangesViewModelBase
    {
        public override string Name
        {
            get { return ""; }
        }

        public override IList<CommitViewModelBase> Commits
        {
            get { return null; }
        }
    }
}
