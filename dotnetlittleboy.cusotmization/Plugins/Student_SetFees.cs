using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace dotnetlittleboy.cusotmization.Plugins
{
    public class Student_SetFees : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
                {
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    EntityReference studentEntityRef = null, CourseEntityRef=null;
                    Entity CourseEntity = null;
                    Money courseFee = null, universityFee = null, totalFee = null;

                    if (context.InputParameters.Contains("CourseName"))
                    {
                        CourseEntityRef = context.InputParameters["CourseName"] as EntityReference;
                    }
                    if (context.InputParameters.Contains("UniversityFee"))
                    {
                        universityFee = context.InputParameters["UniversityFee"] as Money;
                    }
                    else
                    {
                        universityFee = new Money(0);
                    }
                    CourseEntity = service.Retrieve(CourseEntityRef.LogicalName, CourseEntityRef.Id, new ColumnSet("dnlb_coursefee"));
                    if(CourseEntity !=null && CourseEntity.Contains("dnlb_coursefee"))
                    {
                        courseFee = CourseEntity["dnlb_coursefee"] as Money;
                    }
                    else
                    {
                        courseFee = new Money(0);
                    }
                    totalFee = new Money(courseFee.Value + universityFee.Value);

                    //Set the output parameters
                    context.OutputParameters["CourseFee"] = courseFee;
                    context.OutputParameters["TotalFee"] = totalFee;
                }
            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
