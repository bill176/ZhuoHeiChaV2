using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZhuoHeiChaUI.Services
{
    public class LocalEventService
    {
        public event EventHandler ReturnTributeFailed;

        public void RaiseReturnTributeFailed()
        {
            ReturnTributeFailed?.Invoke(this, EventArgs.Empty);
        }
    }
}
