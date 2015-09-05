using OneTox.Model;
using OneTox.Model.Avatars;
using OneTox.Model.FileTransfers;

namespace OneTox.Config
{
    public interface IDataService
    {
        IToxModel ToxModel { get; }
        IAvatarManager AvatarManager { get; }
        IFileTransferResumer FileTransferResumer { get; }
    }
}