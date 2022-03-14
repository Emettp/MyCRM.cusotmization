using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
namespace dotnetlittleboy.cusotmization.Plugins
{
    using Constants;
    public class Application_PostUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                Entity application = null;

                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (context.Depth > 2)
                {
                    return;
                }
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    if (context.MessageName.ToLower() != "update" && context.Stage != 40)
                    {
                        return;
                    }
                    application = context.InputParameters["Target"] as Entity;
                    if (application != null && application.Contains("dnlb_applicationstatus"))
                    {
                        int applicationStatus = (int)(application.GetAttributeValue<OptionSetValue>("dnlb_applicationstatus")).Value;
                        if (applicationStatus == (int)Constants.ApplicationStatus.Approved)
                        {
                            Entity applicationEntity = service.Retrieve(application.LogicalName, application.Id, new ColumnSet("dnlb_firstname", "dnlb_lastname", "dnlb_dateofbirth", "dnlb_contactnumber", "dnlb_courseappliedfor", "dnlb_addressline1", "dnlb_countryid"));
                            
                            OrganizationRequest reqCreateStudent = new OrganizationRequest("dnlb_CreateStudentFromApplication");
                            reqCreateStudent["First_Name"] = applicationEntity.Contains("dnlb_firstname") ? applicationEntity.GetAttributeValue<string>("dnlb_firstname") : string.Empty;
                            reqCreateStudent["Last_Name"] = applicationEntity.Contains("dnlb_lastname") ? applicationEntity.GetAttributeValue<string>("dnlb_lastname") : string.Empty;
                            reqCreateStudent["ContactNumber"] = applicationEntity.Contains("dnlb_contactnumber") ? applicationEntity.GetAttributeValue<string>("dnlb_contactnumber") : string.Empty;
                            reqCreateStudent["AddressLine1"] = applicationEntity.Contains("dnlb_addressline1") ? applicationEntity.GetAttributeValue<string>("dnlb_addressline1") : string.Empty;
                            reqCreateStudent["CourseAppliedFor"] = applicationEntity.Contains("dnlb_courseappliedfor") ? applicationEntity["dnlb_courseappliedfor"] as EntityReference : null;

                            reqCreateStudent["country"] = applicationEntity.Contains("dnlb_countryid") ? (EntityReference)applicationEntity["dnlb_countryid"] : null;
                            reqCreateStudent["DateOfBirth"] = applicationEntity.Contains("dnlb_dateofbirth") ? (DateTime)applicationEntity["dnlb_dateofbirth"] : new DateTime();

                            OrganizationResponse resCreateStudent = service.Execute(reqCreateStudent);

                            SetStateRequest setApplicationStatusInactive = new SetStateRequest()
                            {
                                EntityMoniker = new EntityReference
                                {
                                    LogicalName = application.LogicalName,
                                    Id = application.Id
                                },
                                State = new OptionSetValue(1),
                                Status = new OptionSetValue(2)
                            };
                            service.Execute(setApplicationStatusInactive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
