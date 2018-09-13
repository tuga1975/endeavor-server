using Endeavor.Core;
using Endeavor.Messaging;
using Endeavor.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Endeavor.Server.Workflow
{
    public class Engine : IMessageCallback<Task>
    {
        private readonly IDal _dal;

        public Engine(IDal dal)
        {
            _dal = dal;
        }

        public void HandleMessage(Task message)
        {
            Dictionary<string, object> stepData = _dal.GetStepData(message.Id);
            string data = stepData["ID"].ToString();
            string stepClassName = stepData["StepClassName"].ToString();

            dynamic step = CreateStep(message.StepId, stepClassName);
            dynamic req = CreateRequest(step.InputType, message, data);
            dynamic res = CreateResponse(step.OutputType);

            try
            {
                res = step.Start(req);

                switch (res.Task.Status)
                {
                    case StatusType.Wait:
                        _dal.SaveTask(res.Task, res.Data);
                        break;
                    case StatusType.Error:
                        _dal.SaveTask(res.Task, res.Data);
                        break;
                    default:
                        ReleaseTask(res);
                        break;
                }
            }
            catch (Exception ex)
            {
                if (res.Task != null)
                {
                    res.Task.Status = StatusType.Error;
                    res.Task.StatusMessage = ex.Message;
                    _dal.SaveTask(res.Task, res.Data);
                }
            }
        }

        public dynamic CreateStep(int stepId, string stepClassName)
        {
            Dictionary<string, object> stepProps = _dal.GetStep(stepId, stepClassName);

            Type dataType = Type.GetType(stepClassName);
            dynamic step = Activator.CreateInstance(dataType);
            step.Initialize(stepProps);

            return step;
        }

        public dynamic CreateRequest(string dataType, Task task, string data)
        {
            Type inputType = Type.GetType(dataType);
            Type requestType = typeof(StepRequest<>).MakeGenericType(inputType);
            dynamic req = Activator.CreateInstance(requestType);
            req.Task = task;

            dynamic dataObj = JsonConvert.DeserializeObject(data, inputType);
            req.Data = dataObj; 

            return req;
        }

        public dynamic CreateResponse(string dataType)
        {
            Type outputType = Type.GetType(dataType);
            Type responseType = typeof(StepResponse<>).MakeGenericType(outputType);
            dynamic res = Activator.CreateInstance(responseType);

            return res;
        }

        public StepLink GetNextLink(Task task, List<StepLink> links)
        {
            StepLink link;
            if (links.Count == 0)
            {
                link = null;
            }
            else if (links.Count == 1)
            {
                link = links[0];
            }
            else
            {
                link = links.Find(s => s.SourceStep == task.StepId && s.Value.ToLower() == task.ReleaseValue.ToString().ToLower()); 
            }

            return link;
        }

        private void ReleaseTask(dynamic res)
        {
            Task task = res.Task;
            string data = JsonConvert.SerializeObject(res.Data);

            List<StepLink> stepLinks = new List<StepLink>();
            List<Dictionary<string, object>> links = _dal.GetStepLinks(task.StepId);
            foreach(Dictionary<string, object> link in links)
            {
                StepLink sl = new StepLink(link);
                stepLinks.Add(sl);
            }

            StepLink nextLink = GetNextLink(task, stepLinks);

            if (nextLink != null)
            {
                task.StepId = nextLink.DestinationStep;
                task.Status = StatusType.Ready;
            }
            else
            {
                task.Status = StatusType.Complete;
            }

            _dal.SaveTask(task.Properties, data);
        }
    }
}
