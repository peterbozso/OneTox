using OneTox.Model;
using OneTox.Model.Avatars;
using OneTox.Model.FileTransfers;

namespace OneTox.Config
{
    internal class DataService : IDataService
    {
        private readonly AvatarManager _avatarManager;
        private readonly FileTransferResumer _fileTransferResumer;
        private readonly ToxModel _toxModel;

        public DataService()
        {
            _toxModel = new ToxModel();
            _avatarManager = new AvatarManager(_toxModel);
            _fileTransferResumer = new FileTransferResumer(_toxModel);
        }

        public IToxModel ToxModel => _toxModel;
        public IAvatarManager AvatarManager => _avatarManager;
        public IFileTransferResumer FileTransferResumer => _fileTransferResumer;
    }
}