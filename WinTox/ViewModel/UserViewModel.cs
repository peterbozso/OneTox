using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTox.Core;

namespace WinTox.ViewModel {
    class UserViewModel {
        public string Name {
            get { return ToxViewModel.Instance.Name; }
        }

        public string StatusMessage {
            get { return ToxViewModel.Instance.StatusMessage; }
        }

        public ToxUserStatus Status {
            get { return ToxViewModel.Instance.Status; }
        }
    }
}
