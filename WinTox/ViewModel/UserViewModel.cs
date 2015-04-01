using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpTox.Core;
using WinTox.Model;

namespace WinTox.ViewModel {
    class UserViewModel {
        public string Name {
            get { return ToxSingletonModel.Instance.Name; }
        }

        public string StatusMessage {
            get { return ToxSingletonModel.Instance.StatusMessage; }
        }

        public ToxUserStatus Status {
            get { return ToxSingletonModel.Instance.Status; }
        }
    }
}
