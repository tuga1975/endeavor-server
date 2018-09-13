using Endeavor.Core;
using System.Collections.Generic;

namespace Endeavor.Server.Tests.Helpers
{
    public class TestStep : Step<string, int>
    {
        public string Test { get; set; }
        protected override void Load(Dictionary<string, object> properties)
        {
            foreach (string key in properties.Keys)
            {
                switch (key)
                {
                    case "Test":
                        Test = properties[key].ToString();
                        break;
                }
            }
        }

        protected override StepResponse<int> Run(StepRequest<string> request)
        {
            StepResponse<int> response = new StepResponse<int>();
            response.Task = request.Task;
            response.Task.ReleaseValue = "release";
            response.Data = 1;

            return response;
        }
    }
}
