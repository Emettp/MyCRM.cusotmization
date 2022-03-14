using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace dotnetlittleboy.cusotmization.Plugins
{
    public class University_Update : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("University Update Plugin");
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                if (context.MessageName.ToLower() == "update")
                {
                    Entity targetEntity = context.InputParameters["Target"] as Entity;
                    targetEntity["dnlb_universitynumber"] = "Value from Plugin";
                    if (context.Stage == 40)
                    {
                        if (context.Depth > 1)
                        {
                            return;
                        }
                        service.Update(targetEntity);
                    }
                }
            }
        }
    }
}
