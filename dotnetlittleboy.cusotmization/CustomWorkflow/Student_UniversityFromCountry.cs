using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
namespace dotnetlittleboy.cusotmization.CustomWorkflow
{
    public class Student_UniversityFromCountry : CodeActivity
    {
        [Input("Country")]
        [RequiredArgument]
        [ReferenceTarget("dnlb_country")]
        public InArgument<EntityReference> Country {get; set;}

        [Output("UniversityFee")]
        public OutArgument<Money> UniversityFee { get; set; }
        
        protected override void Execute(CodeActivityContext context)
        {

            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.InitiatingUserId);
            Guid countryId = this.Country.Get(context).Id;
            if(countryId != null)
            {
                Entity countryEntity = service.Retrieve("dnlb_country", countryId, new ColumnSet("dnlb_universityfee"));
                if(countryEntity != null && countryEntity.Contains("dnlb_universityfee"))
                {
                    this.UniversityFee.Set(context, countryEntity.GetAttributeValue<Money>("dnlb_universityfee"));
                }
            }
        }
    }
}
