using Endeavor.Core;
using Endeavor.Messaging;
using Endeavor.Persistence;
using Endeavor.Server.Workflow;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Endeavor.Server
{
    public class Worker
    {
        private ServiceProvider _serviceProvider;

        public void Initialize(ServiceCollection services)
        {
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        public void Run()
        {
            if (_serviceProvider == null)
            {
                throw new NullReferenceException("The service provider is null. Make sure to call Initialize before calling Run.");
            }

            var dispatcher = _serviceProvider.GetService<IDispatcher>();
            dispatcher.Start();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<IDal, Dal>();
            services.AddTransient<IMessageCallback<Task>, Engine>();
            services.AddTransient<IDispatcher, Dispatcher>();
        }
    }
}
