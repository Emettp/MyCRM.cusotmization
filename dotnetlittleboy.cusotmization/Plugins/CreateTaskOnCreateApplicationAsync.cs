using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dotnetlittleboy.cusotmization.Plugins
{
    public class CreateTaskOnCreateApplicationAsync : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    if (context.MessageName != "Create" && context.Stage != 40)
                    {
                        return;
                    }
                    Entity targetApplication = context.InputParameters["Target"] as Entity;

                    Entity createTask = new Entity("task");
                    createTask["subject"] = "Application Id: " + targetApplication.Id.ToString();
                    createTask["regardingobjectid"] = new EntityReference(targetApplication.LogicalName, targetApplication.Id);
                    service.Create(createTask);


                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message, ex);
            }
        }
    }
}
