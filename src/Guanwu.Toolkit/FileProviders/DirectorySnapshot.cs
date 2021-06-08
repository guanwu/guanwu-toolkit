using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Guanwu.Toolkit.FileProviders
{
    internal class DirectorySnapshot
    {
        private readonly string _directory;
        private readonly string _pattern;
        private readonly SearchOption _option;
        private readonly DirectoryComparer _comparer;
        private Dictionary<string, FileSnapshot> _previousSnapshot;
        private Dictionary<string, FileSnapshot> _currentSnapshot;

        public DirectorySnapshot(string directory)
            : this(directory, SearchOption.TopDirectoryOnly) { }

        public DirectorySnapshot(string directory, SearchOption searchOption)
            : this(directory, "*", searchOption) { }

        public DirectorySnapshot(string directory, string searchPattern, SearchOption searchOption)
        {
            _directory = directory;
            _pattern = searchPattern;
            _option = searchOption;
            _comparer = new DirectoryComparer();
        }

        public void CreateSnapshot() => CreateSnapshot(_ => true);

        public void CreateSnapshot(Func<string, bool> predicate)
        {
            _previousSnapshot = _currentSnapshot ?? new Dictionary<string, FileSnapshot>();
            _currentSnapshot = Directory
                .EnumerateFiles(_directory, _pattern, _option)
                .Where(file => predicate(file))
                .Select(file => new FileSnapshot(file))
                .Where(snapshot => !string.IsNullOrEmpty(snapshot.Name))
                .ToDictionary(snapshot => snapshot.Name);
        }

        public IEnumerable<FileSnapshot> GetCreatedSnapshots(string[] filters = null)
        {
            return _currentSnapshot
                .Except(_previousSnapshot, _comparer)
                .Where(x => MatchPattern(x.Key, filters))
                .Select(x => x.Value);
        }

        public IEnumerable<FileSnapshot> GetChangedSnapshots(string[] filters = null)
        {
            return _currentSnapshot
                .Intersect(_previousSnapshot, _comparer)
                .Where(x => MatchPattern(x.Key, filters))
                .Where(x => !x.Value.Equals(_previousSnapshot[x.Key]))
                .Select(x => x.Value);
        }

        public IEnumerable<FileSnapshot> GetDeletedSnapshots(string[] filters = null)
        {
            return _previousSnapshot
                .Except(_currentSnapshot, _comparer)
                .Where(x => MatchPattern(x.Key, filters))
                .Select(x => x.Value);
        }

        private static bool MatchPattern(string value, params string[] patterns)
        {
            if (patterns == null) return true;
            if (string.IsNullOrWhiteSpace(value)) return false;
            foreach (string p in patterns) {
                bool isMatch = PatternMatcher.StrictMatchPattern(
                    p.ToUpper(CultureInfo.InvariantCulture),
                    value.ToUpper(CultureInfo.InvariantCulture)
                );
                if (isMatch) return true;
            }
            return false;
        }

    }
}