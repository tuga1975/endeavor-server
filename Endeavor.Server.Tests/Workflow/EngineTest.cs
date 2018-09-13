using Endeavor.Core;
using Endeavor.Messaging;
using Endeavor.Persistence;
using Endeavor.Server.Workflow;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Endeavor.Server.Tests
{
    public class EngineTest
    {
        [Fact]
        public void HandleMessageTest()
        {
            var mock = new Mock<IDal>();

            Dictionary<string, object> step = GetStep();
            mock.Setup(m => m.GetStep(It.IsAny<int>(), It.IsAny<string>())).Returns(step);

            Dictionary<string, object> link = GetStepLink();
            List<Dictionary<string, object>> links = new List<Dictionary<string, object>>();
            links.Add(link);
            mock.Setup(m => m.GetStepLinks(It.IsAny<int>())).Returns(links);

            Dictionary<string, object> stepData = GetStepData();
            mock.Setup(m => m.GetStepData(It.IsAny<int>())).Returns(stepData);

            Dictionary<string, object> savedTask = new Dictionary<string, object>();
            mock.Setup(m => m.SaveTask(It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
                .Callback<Dictionary<string, object>, string>((d,s) => savedTask = d);

            IMessageCallback<Task> engine = new Engine(mock.Object);

            Dictionary<string, object> tp = GetTask();
            Task t = new Task();
            t.Initialize(tp);

            engine.HandleMessage(t);

            Assert.Equal(1, (int)savedTask["ID"]);
            Assert.Equal(2, (int)savedTask["StepID"]);
            Assert.Equal(StatusType.Ready, (StatusType)savedTask["StatusID"]);
            Assert.Equal("release", savedTask["ReleaseValue"].ToString());
        }

        private Dictionary<string, object> GetStep()
        {
            Dictionary<string, object> step = new Dictionary<string, object>();
            step.Add("ID", 1);
            step.Add("WorkflowID", 1);
            step.Add("Name", "TestStep");
            step.Add("ClassName", "Endeavor.Server.Tests.Helpers.TestStep, Endeavor.Server.Tests");
            step.Add("InputType", "System.String");
            step.Add("OutputType", "System.Int32");
            step.Add("Test", "test");

            return step;
        }

        private Dictionary<string, object> GetStepLink()
        {
            Dictionary<string, object> link = new Dictionary<string, object>();
            link.Add("SourceID", 1);
            link.Add("DestinationID", 2);
            link.Add("ReleaseValue", "test");

            return link;
        }

        private Dictionary<string, object> GetStepData()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("ID", 1);
            data.Add("StepClassName", "Endeavor.Server.Tests.Helpers.TestStep, Endeavor.Server.Tests");

            return data;
        }

        private Dictionary<string, object> GetTask()
        {
            Dictionary<string, object> task = new Dictionary<string, object>();
            task.Add("ID", 1);
            task.Add("StepID", 1);
            task.Add("StatusID", StatusType.Ready);

            return task;
        }
    }
}