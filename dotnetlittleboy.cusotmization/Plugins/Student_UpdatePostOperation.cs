namespace dotnetlittleboy.cusotmization.Plugins
{
    using Constants;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using System;

    /// <summary>
    /// Defines the <see cref="Student_UpdatePostOperation" />.
    /// </summary>
    public class Student_UpdatePostOperation : IPlugin
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
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                    if (context.MessageName.ToLower() != "update" && context.Stage != 40)
                    {
                        return;
                    }
                    Entity preImage = context.PreEntityImages["preImage"] as Entity;
                    Entity postImage = context.PostEntityImages["postImage"] as Entity;
                    if (preImage == null || postImage == null)
                    {
                        return;
                    }
                    if (preImage.Contains("dnlb_semester") && postImage.Contains("dnlb_semester"))
                    {
                        if (((OptionSetValue)preImage["dnlb_semester"]).Value == (int)Constants.Semester.Semester1 && ((OptionSetValue)postImage["dnlb_semester"]).Value == (int)Constants.Semester.Semester2)
                        {
                            int cycle = (int)((OptionSetValue)preImage["dnlb_cycle"]).Value;
                            Entity updateStudent = null;
                            switch (cycle)
                            {
                                case (int)Constants.Cycle.PCycle:
                                    updateStudent = new Entity("dnlb_student");
                                    updateStudent.Id = preImage.Id;
                                    updateStudent["dnlb_cycle"] = new OptionSetValue((int)Constants.Cycle.CCycle);
                                    service.Update(updateStudent);
                                    updateStudentCourse(service, tracingService, preImage);
                                    createStudentCourse(service, tracingService, preImage, false);
                                    break;
                                case (int)Constants.Cycle.CCycle:
                                    updateStudent = new Entity("dnlb_student");
                                    updateStudent.Id = preImage.Id;
                                    updateStudent["dnlb_cycle"] = new OptionSetValue((int)Constants.Cycle.PCycle);
                                    service.Update(updateStudent);
                                    updateStudentCourse(service, tracingService, preImage);
                                    createStudentCourse(service, tracingService, preImage, true);
                                    break;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        /// <summary>
        /// The updateStudentCourse.
        /// </summary>
        /// <param name="service">The service<see cref="IOrganizationService"/>.</param>
        /// <param name="tracingService">The tracingService<see cref="ITracingService"/>.</param>
        /// <param name="studentEntity">The studentEntity<see cref="Entity"/>.</param>
        private void updateStudentCourse(IOrganizationService service, ITracingService tracingService, Entity studentEntity)
        {
            tracingService.Trace("Updating the student course to yes for Student Id " + studentEntity.Id);
            QueryExpression qeStudentCourse = new QueryExpression()
            {
                EntityName = "dnlb_studentcourse",
                ColumnSet = new ColumnSet("dnlb_name")
            };
            qeStudentCourse.Criteria.AddCondition("dnlb_studentid", ConditionOperator.Equal, studentEntity.Id);
            qeStudentCourse.Criteria.AddCondition("dnlb_issubjectcompleted", ConditionOperator.Equal, false);
            EntityCollection ecStuentCourse = service.RetrieveMultiple(qeStudentCourse);
            if (ecStuentCourse.Entities.Count > 0)
            {
                foreach (Entity entity in ecStuentCourse.Entities)
                {
                    entity["dnlb_issubjectcompleted"] = true;
                    service.Update(entity);
                }
            }
            tracingService.Trace("Completed updating the student course.");
        }

        /// <summary>
        /// The createStudentCourse.
        /// </summary>
        /// <param name="service">The service<see cref="IOrganizationService"/>.</param>
        /// <param name="tracingService">The tracingService<see cref="ITracingService"/>.</param>
        /// <param name="studentEntity">The studentEntity<see cref="Entity"/>.</param>
        /// <param name="isPCycle">The isPCycle<see cref="bool"/>.</param>
        private void createStudentCourse(IOrganizationService service, ITracingService tracingService, Entity studentEntity, bool isPCycle)
        {
            tracingService.Trace("Creating the Student Course");
            QueryExpression qeSubject = new QueryExpression()
            {
                EntityName = "dnlb_subject",
                ColumnSet = new ColumnSet("dnlb_name")
            };
            qeSubject.Criteria.AddCondition("dnlb_semester", ConditionOperator.Equal, (int)Constants.Semester.Semester2);
            if (isPCycle)
            {
                qeSubject.Criteria.AddCondition("dnlb_cycle", ConditionOperator.Equal, (int)Constants.Cycle.PCycle);
            }
            else
            {
                qeSubject.Criteria.AddCondition("dnlb_cycle", ConditionOperator.Equal, (int)Constants.Cycle.CCycle);
            }
            EntityCollection ecSubject = service.RetrieveMultiple(qeSubject);
            if (ecSubject.Entities.Count > 0)
            {
                foreach (Entity entity in ecSubject.Entities)
                {
                    Entity createStudentCourse = new Entity("dnlb_studentcourse");
                    createStudentCourse["dnlb_name"] = studentEntity["dnlb_name"].ToString() + " - " + entity["dnlb_name"].ToString();
                    createStudentCourse["dnlb_studentid"] = new EntityReference("dnlb_student", studentEntity.Id);
                    createStudentCourse["dnlb_subjectsid"] = new EntityReference("dnlb_subject", entity.Id);

                    service.Create(createStudentCourse);
                }
            }
        }
    }
}
