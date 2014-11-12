using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitLog
{
    public class RepositoryViewModel
    {
        private readonly string _path;

        public RepositoryViewModel(string path)
        {
            _path = path;
            LoadCommits();
        }

        private IList<CommitViewModel> _commits;
        public IList<CommitViewModel> Commits
        {
            get { return _commits; }
        }

        public string Path
        {
            get { return _path; }
        }

        public void LoadCommits()
        {
            using (var repo = new Repository(Path))
            {
                var subPaths = new HashSet<string>(repo.Submodules.Select(s => s.Path));
                _commits = repo.Commits.Select(c => new CommitViewModel(c, Path, subPaths)).ToList();
            }
        }
    }
}
