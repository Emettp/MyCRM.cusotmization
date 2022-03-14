namespace dotnetlittleboy.cusotmization.Plugins
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using System;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// Defines the <see cref="Student_PostCreate" />.
    /// </summary>
    public class Student_PostCreate : IPlugin
    {
        /// <summary>
        /// The Execute.
        /// </summary>
        /// <param name="serviceProvider">The serviceProvider<see cref="IServiceProvider"/>.</param>
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                if (context.MessageName.ToLower() != "create" && context.Stage != 40)
                {
                    return;
                }
                Guid targetEntityId = Guid.Parse(context.OutputParameters["id"].ToString());
                Entity targetEntity = service.Retrieve("dnlb_student", targetEntityId, new ColumnSet("dnlb_name", "dnlb_cycle"));
                tracingService.Trace("Target enttiy retrieved");
                if (targetEntity != null)
                {
                    int cycle = (int)((OptionSetValue)targetEntity["dnlb_cycle"]).Value;
                    switch (cycle)
                    {
                        case (int)Constants.Constants.Cycle.PCycle:
                            createStudentCourse(service, tracingService, targetEntityId, targetEntity, true);
                            break;
                        case (int)Constants.Constants.Cycle.CCycle:
                            createStudentCourse(service, tracingService, targetEntityId, targetEntity, false);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// The createStudentCourse.
        /// </summary>
        /// <param name="service">The service<see cref="IOrganizationService"/>.</param>
        /// <param name="tracingService">The tracingService<see cref="ITracingService"/>.</param>
        /// <param name="targetEntityId">The targetEntityId<see cref="Guid"/>.</param>
        /// <param name="targetEntity">The targetEntity<see cref="Entity"/>.</param>
        /// <param name="isPCycle">The isPCycle<see cref="bool"/>.</param>
        private void createStudentCourse(IOrganizationService service, ITracingService tracingService, Guid targetEntityId, Entity targetEntity, bool isPCycle)
        {
            tracingService.Trace("Executing the creation of Student - Course");
            QueryExpression qeSubject = new QueryExpression()
            {
                EntityName = "dnlb_subject",
                ColumnSet = new ColumnSet("dnlb_name")

            };
            qeSubject.Criteria.AddCondition("dnlb_semester", ConditionOperator.Equal, (int)Constants.Constants.Semester.Semester1);
            if (isPCycle)
            {
                qeSubject.Criteria.AddCondition("dnlb_cycle", ConditionOperator.In, (int)Constants.Constants.Cycle.PCycle, (int)Constants.Constants.Cycle.Both);
            }
            else
            {
                qeSubject.Criteria.AddCondition("dnlb_cycle", ConditionOperator.In, (int)Constants.Constants.Cycle.CCycle, (int)Constants.Constants.Cycle.Both);
            }

            EntityCollection ecSubject = service.RetrieveMultiple(qeSubject);
            tracingService.Trace("Total Subject retrieved " + ecSubject.Entities.Count);
            if (ecSubject.Entities.Count != 0)
            {
                foreach (Entity entity in ecSubject.Entities)
                {
                    Entity createStudentCourse = new Entity("dnlb_studentcourse");
                    createStudentCourse["dnlb_studentid"] = new EntityReference("dnlb_student", targetEntityId);
                    createStudentCourse["dnlb_subjectsid"] = new EntityReference("dnlb_subject", entity.Id);
                    createStudentCourse["dnlb_name"] = targetEntity["dnlb_name"].ToString() + " - " + entity["dnlb_name"].ToString();
                    service.Create(createStudentCourse);
                }
            }
        }
    }
}
