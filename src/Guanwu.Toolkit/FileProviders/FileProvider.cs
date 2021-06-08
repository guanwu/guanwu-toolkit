using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Guanwu.Toolkit.FileProviders
{
    public class FileProvider
    {
        public event EventHandler<FileMessage> OnFileCreated;
        public event EventHandler<FileMessage> OnFileChanged;
        public event EventHandler<FileMessage> OnFileDeleted;
        public event EventHandler<Exception> OnException;

        public string[] Directories { get; }
        public string[] Filters { get; set; } = new[] { "*" };
        public int Interval { get; set; } = 60000;
        public bool IncludeSubpath { get; set; } = false;
        public bool IncludeExistingFiles { get; set; } = false;

        private readonly SearchOption _searchOption;

        private readonly QueueBlock<FileMessage> _createdQueue;
        private readonly QueueBlock<FileMessage> _changedQueue;
        private readonly QueueBlock<FileMessage> _deletedQueue;
        private readonly CancellationTokenSource _producerTokenSource;
        private readonly CancellationTokenSource _consumerTokenSource;

        public FileProvider(params string[] directories)
        {
            Directories = directories;
            _searchOption = IncludeSubpath ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            _producerTokenSource = new CancellationTokenSource();
            _consumerTokenSource = new CancellationTokenSource();
            _createdQueue = new QueueBlock<FileMessage>();
            _changedQueue = new QueueBlock<FileMessage>();
            _deletedQueue = new QueueBlock<FileMessage>();
            _createdQueue.OnTaked += (_, e) => OnFileCreated(this, e);
            _changedQueue.OnTaked += (_, e) => OnFileChanged(this, e);
            _deletedQueue.OnTaked += (_, e) => OnFileDeleted(this, e);
        }

        public void Start()
        {
            Task taskProducing = Task.Run(() => {
                Parallel.ForEach(Directories, ProduceDirectory);
            });
            Task taskConsuming = Task.Run(() => {
                ConsumePipelines();
            });
        }

        public void Stop()
        {
            _producerTokenSource.Cancel();
            _createdQueue.CompleteAdding();
            _changedQueue.CompleteAdding();
            _deletedQueue.CompleteAdding();
        }

        public void Abort()
        {
            _producerTokenSource.Cancel();
            _consumerTokenSource.Cancel();
            _createdQueue.CompleteAdding();
            _changedQueue.CompleteAdding();
            _deletedQueue.CompleteAdding();
        }

        private void ConsumePipelines()
        {
            Task taskCreatedConsuming = OnFileCreated == null ?
                Task.CompletedTask : Task.Run(() => _createdQueue.Consume(_consumerTokenSource.Token));
            Task taskChangedConsuming = OnFileChanged == null ?
                Task.CompletedTask : Task.Run(() => _changedQueue.Consume(_consumerTokenSource.Token));
            Task taskDeletedConsuming = OnFileDeleted == null ?
                Task.CompletedTask : Task.Run(() => _deletedQueue.Consume(_consumerTokenSource.Token));

            Task.WaitAll(
                taskCreatedConsuming,
                taskChangedConsuming,
                taskDeletedConsuming
            );
        }

        private void ProduceDirectory(string directory)
        {
            Task taskCreatedProducing = OnFileCreated == null ?
                Task.CompletedTask : Task.Run(() => ProduceCreatedFiles(directory));
            Task taskChangedProducing = OnFileChanged == null ?
                Task.CompletedTask : Task.Run(() => ProduceChangedFiles(directory));
            Task taskDeletedProducing = OnFileDeleted == null ?
                Task.CompletedTask : Task.Run(() => ProduceDeletedFiles(directory));

            Task.WaitAll(
                taskCreatedProducing,
                taskChangedProducing,
                taskDeletedProducing
            );
        }

        private void ProduceCreatedFiles(string directory)
        {
            var snapshot = new DirectorySnapshot(directory, _searchOption);
            if (!IncludeExistingFiles) snapshot.CreateSnapshot();
            while (!_createdQueue.IsAddingCompleted) {
                SpinWait.SpinUntil(() => false, Interval);
                ProduceCreatedSnapshot(snapshot);
            }
        }

        private void ProduceCreatedSnapshot(DirectorySnapshot snapshot)
        {
            try {
                snapshot.CreateSnapshot();
                var snapshots = snapshot.GetCreatedSnapshots(Filters);
                var messages = snapshots.Select(x => ReadSnapshot(x)).ToArray();
                _createdQueue.Produce(_producerTokenSource.Token, messages);
            } catch (Exception e) {
                OnException?.Invoke(this, e);
            }
        }

        private void ProduceChangedFiles(string directory)
        {
            var snapshot = new DirectorySnapshot(directory, _searchOption);
            while (!_changedQueue.IsAddingCompleted) {
                SpinWait.SpinUntil(() => false, Interval);
                ProduceChangedSnapshot(snapshot);
            }
        }

        private void ProduceChangedSnapshot(DirectorySnapshot snapshot)
        {
            try {
                snapshot.CreateSnapshot();
                var snapshots = snapshot.GetChangedSnapshots(Filters);
                var messages = snapshots.Select(x => ReadSnapshot(x)).ToArray();
                _changedQueue.Produce(_producerTokenSource.Token, messages);
            } catch (Exception e) {
                OnException?.Invoke(this, e);
            }
        }

        private void ProduceDeletedFiles(string directory)
        {
            var snapshot = new DirectorySnapshot(directory, _searchOption);
            while (!_deletedQueue.IsAddingCompleted) {
                SpinWait.SpinUntil(() => false, Interval);
                ProduceDeletedSnapshot(snapshot);
            }
        }

        private void ProduceDeletedSnapshot(DirectorySnapshot snapshot)
        {
            try {
                snapshot.CreateSnapshot();
                var snapshots = snapshot.GetDeletedSnapshots(Filters);
                var messages = snapshots.Select(x => ReadSnapshot(x)).ToArray();
                _deletedQueue.Produce(_producerTokenSource.Token, messages);
            } catch (Exception e) {
                OnException?.Invoke(this, e);
            }
        }

        private FileMessage ReadSnapshot(FileSnapshot snapshot)
        {
            var fileMessage = new FileMessage {
                Name = snapshot.Name
            };

            try {
                var fileInfo = new FileInfo(snapshot.Name);
                if (fileInfo.Exists) {
                    var fileBytes = ExclusiveRead(fileInfo);
                    fileMessage.Content = Encoding.UTF8.GetString(fileBytes);
                    fileMessage.ContentLength = fileBytes.LongLength;
                }
            } catch { }

            return fileMessage;
        }

        private static byte[] ExclusiveRead(FileInfo file)
        {
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None)) {
                var cache = new MemoryStream();
                var buffer = new byte[4096];
                var len = 0;
                while ((len = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    cache.Write(buffer, 0, len);
                return cache.ToArray();
            }
        }

    }
}