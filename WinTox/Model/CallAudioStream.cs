using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using SharpTox.Av;

namespace WinTox.Model
{
    /// <summary>
    ///     Many thanks to: https://coderead.wordpress.com/2014/06/12/reading-the-microphones-audio-stream-in-winrt/
    /// </summary>
    public class CallAudioStream : IRandomAccessStream
    {
        private readonly int _friendNumber;
        private bool _canSend;

        public CallAudioStream(int friendNumber)
        {
            _friendNumber = friendNumber;
            ToxAvModel.Instance.CallStateChanged += CallStateChangedHandler;
        }

        public bool CanRead
        {
            get { return false; }
        }

        public bool CanWrite
        {
            get { return true; }
        }

        public IRandomAccessStream CloneStream()
        {
            throw new NotImplementedException();
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public ulong Position
        {
            get { return 0; }
        }

        public void Seek(ulong position)
        {
        }

        public ulong Size
        {
            get { return 0; }
            set { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count,
            InputStreamOptions options)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            return AsyncInfo.Run(_ => Task.Run(() => true));
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            return AsyncInfo.Run<uint, uint>((token, progress) =>
            {
                return Task.Run(() =>
                {
                    if (!_canSend)
                        return (uint) 0;

                    var shortArray = buffer.ToArray().Select(b => (short) b).ToArray();

                    ToxAvModel.Instance.SendAudioFrame(_friendNumber, new ToxAvAudioFrame(shortArray, 48000, 1));

                    return (uint) 0;
                });
            });
        }

        private void CallStateChangedHandler(object sender, ToxAvEventArgs.CallStateEventArgs e)
        {
            if (e.FriendNumber != _friendNumber)
                return;

            if (e.State.HasFlag(ToxAvFriendCallState.SendingAudio))
            {
                _canSend = true;
            }
        }
    }
}