using OneTox.Model.Avatars;
using OneTox.Model.FileTransfers;
using OneTox.Model.Tox;

namespace OneTox.Config
{
    public interface IDataService
    {
        IToxModel ToxModel { get; }
        IAvatarManager AvatarManager { get; }
        IFileTransferResumer FileTransferResumer { get; }
    }
}