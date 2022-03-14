namespace dotnetlittleboy.cusotmization.Plugins
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using System;

    /// <summary>
    /// Defines the <see cref="Student_DeletePreValidation" />.
    /// </summary>
    public class Student_DeletePreValidation : IPlugin
    {
        /// <summary>
        /// The Execute.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/>.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                if (context.MessageName.ToLower() != "delete" && context.Stage != 10)
                {
                    return;
                }
                EntityReference targetEntity = context.InputParameters["Target"] as EntityReference;
                QueryExpression qeStudentCourse = new QueryExpression()
                {
                    EntityName = "dnlb_studentcourse",
                    ColumnSet = new ColumnSet("dnlb_name")
                };
                qeStudentCourse.Criteria.AddCondition("dnlb_studentid", ConditionOperator.Equal, targetEntity.Id);

                EntityCollection ecStudentCourse = service.RetrieveMultiple(qeStudentCourse);
                if (ecStudentCourse.Entities.Count > 0)
                {
                    throw new Exception("Child record of Student Course found. Can not delete this record.");
                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
