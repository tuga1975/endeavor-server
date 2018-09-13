using Endeavor.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Endeavor.Server.Workflow
{
    class Dispatcher : IDispatcher
    {
        private readonly IMessageListener<int> _listener;
        private readonly IMessageCallback<int> _callback;

        public Dispatcher(IMessageListener<int> listener, IMessageCallback<int> callback)
        {
            _listener = listener;
            _callback = callback;

        }

        public void Start()
        {
            _listener.Start(_callback);
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}
