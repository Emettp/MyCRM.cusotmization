using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace dotnetlittleboy.cusotmization.Plugins
{
    public class Student_RetrievePreOp : IPlugin
    {
        private readonly string unsecure;
        private readonly string secure;
        public Student_RetrievePreOp(string unsecureConfig, string secureConfig)
        {
            if (!String.IsNullOrWhiteSpace(unsecureConfig))
            {
                unsecure = unsecureConfig;
            }
            else
            {
                unsecure = string.Empty;
            }
            if (!String.IsNullOrWhiteSpace(secureConfig))
            {
                secure = secureConfig;
            }
            else
            {
                secure = string.Empty;
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
                // Obtain the organization service reference.
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                IOrganizationService ser = serviceFactory.CreateOrganizationService(context.InitiatingUserId);
                // The InputParameters collection contains all the data passed in the message request.
                if (context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is EntityReference)
                {
                    EntityReference studentEntity = context.InputParameters["Target"] as EntityReference;
                    Entity student = service.Retrieve(studentEntity.LogicalName, studentEntity.Id, new ColumnSet("dnlb_firstname","dnlb_name"));

                    student["dnlb_name"] = student["dnlb_name"].ToString() +"-"+ secure;

                    service.Update(student);
                    context.SharedVariables["contactid"] = "contactGuid";
                   

                }
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }

    public class Student_PostPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            string contactid = context.SharedVariables["contactid"].ToString();
        }


    }
}
