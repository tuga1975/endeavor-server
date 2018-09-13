using System;
using System.Collections.Generic;
using System.Text;

namespace Endeavor.Server.Workflow
{
    interface IDispatcher
    {
        void Start();
        void Stop();
    }
}
