using System.Diagnostics;
using SharpTox.Core;

// Implements the Singleton pattern.

namespace WinTox.Model {
    internal class ToxSingletonModel {
        private static readonly ToxNode[] _nodes = {
            new ToxNode("192.254.75.98", 33445, new ToxKey(ToxKeyType.Public, "951C88B7E75C867418ACDB5D273821372BB5BD652740BCDF623A4FA293E75D2F")),
            new ToxNode("144.76.60.215", 33445, new ToxKey(ToxKeyType.Public, "04119E835DF3E78BACF0F84235B300546AF8B936F035185E2A8E9E0A67C8924F"))
        };

        private static ExtendedTox _tox;

        private ToxSingletonModel() {}

        public static ExtendedTox Instance {
            get {
                if (_tox == null) {
                    _tox = new ExtendedTox(new ToxOptions(true, true));

                    foreach (ToxNode node in _nodes)
                        _tox.Bootstrap(node);

                    _tox.Name = "User";
                    _tox.StatusMessage = "This is a test.";

                    _tox.Start();

                    string id = _tox.Id.ToString();
                    Debug.WriteLine("ID: {0}", id);
                }
                return _tox;
            }
        }
    }
}
