using OneTox.Model.Avatars;
using OneTox.Model.FileTransfers;
using OneTox.Model.Messaging;
using OneTox.Model.Tox;

namespace OneTox.Config
{
    internal class DataService : IDataService
    {
        private readonly AvatarManager _avatarManager;
        private readonly FileTransferResumer _fileTransferResumer;
        private readonly MessageHistoryManager _messageHistoryManager;
        private readonly ToxModel _toxModel;

        public DataService()
        {
            _toxModel = new ToxModel();
            _avatarManager = new AvatarManager(_toxModel);
            _fileTransferResumer = new FileTransferResumer(_toxModel);
            _messageHistoryManager = new MessageHistoryManager();
        }

        public IToxModel ToxModel => _toxModel;
        public IAvatarManager AvatarManager => _avatarManager;
        public IFileTransferResumer FileTransferResumer => _fileTransferResumer;
        public IMessageHistoryManager MessageHistoryManager => _messageHistoryManager;
    }
}