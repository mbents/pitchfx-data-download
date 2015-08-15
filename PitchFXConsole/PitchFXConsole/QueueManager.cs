using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace PitchFXConsole
{
    public sealed class QueueManager
    {
        #region Properties

        private static readonly QueueManager _instance = new QueueManager();
        private static ConcurrentQueue<QueueItem> _urlsToDownload = new ConcurrentQueue<QueueItem>();
        private static ConcurrentQueue<QueueItem> _filesToParse = new ConcurrentQueue<QueueItem>();

        private static bool _finishedDownloading = false;

        public static QueueManager Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Constructors

        static QueueManager() { }

        private QueueManager() { }

        #endregion

        #region Public methods

        public void AddUrlToDownload(QueueItem item)
        {
            _urlsToDownload.Enqueue(item);
        }

        public QueueItem GetUrlToDownload()
        {
            QueueItem item;
            _urlsToDownload.TryDequeue(out item);
            return item;
        }

        public void AddFileToParse(QueueItem item)
        {
            _filesToParse.Enqueue(item);
        }

        public QueueItem GetFileToParse()
        {
            QueueItem item;
            _filesToParse.TryDequeue(out item);
            return item;
        }

        public bool IsFinishedDownloading()
        {
            return _finishedDownloading;
        }

        public void SetFinishedDownloading(bool value)
        {
            _finishedDownloading = value;
        }

        public int GetUrlCount()
        {
            return _urlsToDownload.Count;
        }

        public int GetFileCount()
        {
            return _filesToParse.Count;
        }

        #endregion
    }
}
