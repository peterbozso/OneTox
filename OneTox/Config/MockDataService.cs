using OneTox.Model;
using OneTox.Model.Avatars;
using OneTox.Model.FileTransfers;

namespace OneTox.Config
{
    internal class MockDataService : IDataService
    {
        public IToxModel ToxModel => new MockToxModel();
        public IAvatarManager AvatarManager => new MockAvatarManager();
        public IFileTransferResumer FileTransferResumer => new MockFileTransferResumer();
    }
}