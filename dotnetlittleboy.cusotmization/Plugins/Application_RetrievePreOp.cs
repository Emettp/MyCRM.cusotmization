using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
namespace dotnetlittleboy.cusotmization.Plugins
{
    using DataContract;
    using Helper;
    public class Application_RetrievePreOp : IPlugin
    {
        public readonly string unsecureValue;
        public readonly string secureValue;
        public Application_RetrievePreOp(string unsecureConfig, string secureConfig)//{"Name":"XYZ"}
        {
            if (String.IsNullOrWhiteSpace(unsecureConfig))
            {
                unsecureValue = string.Empty;

            }
            else
            {
               Config data=  JSONHelper.Deserialize<Config>(unsecureConfig);
                unsecureValue = data.Name;
            }
            if (String.IsNullOrWhiteSpace(secureConfig))
            {
                secureValue = string.Empty;
            }
            else
            {
                Config data = JSONHelper.Deserialize<Config>(secureConfig);
                secureValue = data.Name;
            }
        }
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (context.Depth > 1)
                {
                    return;
                }
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    EntityReference applicationEntityRef = context.InputParameters["Target"] as EntityReference;

                    Entity updateApplication = new Entity(applicationEntityRef.LogicalName);
                    updateApplication.Id = applicationEntityRef.Id;
                    updateApplication["dnlb_name"] = secureValue + " " + unsecureValue;
                    service.Update(updateApplication);

                }
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
