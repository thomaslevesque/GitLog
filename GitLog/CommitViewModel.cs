using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitLog
{
    public abstract class CommitViewModelBase : ObservableBase
    {
    }

    public class CommitViewModel : CommitViewModelBase
    {
        private readonly string _repoPath;
        private readonly ICollection<string> _subModulePaths;

        public CommitViewModel(Commit commit, string repoPath, ICollection<string> subModulePaths)
        {
            _sha = commit.Sha;
            _shortMessage = commit.MessageShort;
            _message = commit.Message;
            _authorName = commit.Author.Name;
            _authorEmail = commit.Author.Email;
            _authorDate = commit.Author.When;
            _committerName = commit.Committer.Name;
            _committerEmail = commit.Committer.Email;
            _committerDate = commit.Committer.When;

            _repoPath = repoPath;
            _subModulePaths = subModulePaths;
        }

        private readonly string _sha;
        public string Sha
        {
            get { return _sha; }
        }

        public string ShortSha
        {
            get { return Sha.Substring(0, 7); }
        }

        private readonly string _shortMessage;
        public string ShortMessage
        {
            get { return _shortMessage; }
        }

        private readonly string _message;
        public string Message
        {
            get { return _message; }
        }

        private readonly string _authorName;
        public string AuthorName
        {
            get { return _authorName; }
        }

        private readonly string _authorEmail;
        public string AuthorEmail
        {
            get { return _authorEmail; }
        }

        private readonly DateTimeOffset _authorDate;
        public DateTimeOffset AuthorDate
        {
            get { return _authorDate; }
        }

        public string AuthorInfo
        {
            get { return string.Format("{0} ({1}) - {2}", _authorName, _authorEmail, _authorDate); }
        }

        private readonly string _committerName;
        public string CommiterName
        {
            get { return _committerName; }
        }

        private readonly string _committerEmail;
        public string CommiterEmail
        {
            get { return _committerEmail; }
        }

        private readonly DateTimeOffset _committerDate;
        public DateTimeOffset CommitterDate
        {
            get { return _committerDate; }
        }

        public string CommitterInfo
        {
            get { return string.Format("{0} ({1}) - {2}", _committerName, _committerEmail, _committerDate); }
        }

        private static readonly SubmoduleChangesViewModelBase[] _preLoadSubmodules = {new DummySubmoduleChangesViewModel()};
        private IList<SubmoduleChangesViewModelBase> _submodules;
        public IList<SubmoduleChangesViewModelBase> Submodules
        {
            get
            {
                if (_submodules == null && IsExpanded)
                    _submodules = LoadSubmodules();

                return _submodules ?? _preLoadSubmodules;
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (Set(ref _isExpanded, value) && value)
                    OnPropertyChanged("Submodules");
            }
        }

        private IList<SubmoduleChangesViewModelBase> LoadSubmodules()
        {
            var list = new List<SubmoduleChangesViewModelBase>();
            using (var repo = new Repository(_repoPath))
            {
                var commit = (Commit)repo.Lookup(new ObjectId(Sha), ObjectType.Commit);
                foreach (var parent in commit.Parents)
                {
                    var diff = repo.Diff.Compare<TreeChanges>(
                        parent.Tree,
                        commit.Tree,
                        _subModulePaths)
                        .Where(tc => tc.Mode == Mode.GitLink);
                    foreach (var change in diff)
                    {
                        string changePath = NormalizePath(change.Path);
                        var submodule = repo.Submodules.FirstOrDefault(s => NormalizePath(s.Path) == changePath);
                        string since = change.Oid.Sha;
                        string until = null;
                        if (change.OldOid.Sha != "0000000000000000000000000000000000000000")
                            until = change.OldOid.Sha;
                        list.Add(new SubmoduleChangesViewModel(submodule, repo.Info.WorkingDirectory, since, until));
                    }
                }
            }
            return list;
        }

        private static string NormalizePath(string path)
        {
            return path.Replace(@"\", "/").Trim('/');
        }
    }

    public class DummyCommitViewModel : CommitViewModelBase
    {
    }
}
