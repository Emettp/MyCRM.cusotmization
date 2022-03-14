using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Xrm.Sdk.Query;

namespace dotnetlittleboy.cusotmization.CustomWorkflow
{
    public class Student_NextDateOfBirth : CodeActivity
    {
        [Input("Date of Birth for Record")]
        [RequiredArgument]
        [ReferenceTarget("dnlb_student")]
        public InArgument<EntityReference> Student { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
                IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
                IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.InitiatingUserId);

                Guid studentId = this.Student.Get(context).Id;
                Entity studentEntity = service.Retrieve("dnlb_student", studentId, new ColumnSet("dnlb_dateofbirth"));

                DateTime? dateOfBirth;
                if (studentEntity.Contains("dnlb_dateofbirth"))
                {
                    dateOfBirth = (DateTime?)studentEntity["dnlb_dateofbirth"];
                    if (dateOfBirth.Value.Year > DateTime.Now.Year)
                    {
                        throw new Exception("Date of Birth can not have year greater than current year.");
                    }
                    else if (dateOfBirth.Value.Year == DateTime.Now.Year)
                    {
                        if (dateOfBirth.Value.Month > DateTime.Now.Month)
                        {
                            throw new Exception("Date of Birth can not have month greater than current month & year");
                        }
                        else if (dateOfBirth.Value.Month == DateTime.Now.Month)
                        {
                            if (dateOfBirth.Value.Day > DateTime.Now.Day)
                            {
                                throw new Exception("Date of birth can not be greater than current date.");
                            }
                        }
                    }
                }
                else
                {
                    dateOfBirth = null;
                }
                if (dateOfBirth == null)
                {
                    return;
                }

                DateTime nextDateOfBirth = GetNextDateOfBirth(dateOfBirth.Value);
                Entity updateStudent = new Entity("dnlb_student");
                updateStudent.Id = studentId;
                updateStudent["dnlb_nextdateofbirth"] = nextDateOfBirth;
                service.Update(updateStudent);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private DateTime GetNextDateOfBirth(DateTime dateofbirth)
        {
            DateTime nextDOB = new DateTime(dateofbirth.Year, dateofbirth.Month, dateofbirth.Day);
            if (nextDOB.Month == 2 && nextDOB.Day == 29)
            {
                //Todo
                if (!DateTime.IsLeapYear(DateTime.Now.Year))//Not leap year
                {
                    if (nextDOB.Month < DateTime.Now.Month)
                    {
                        if (DateTime.IsLeapYear(DateTime.Now.Year + 1))
                        {
                            nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day);
                        }
                        else
                        {
                            nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day - 1);
                        }
                    }
                    else
                    {
                        nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day - 1);
                    }
                }
                else//leap year
                {
                    if (nextDOB.Month < DateTime.Now.Month)
                    {
                        nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day - 1);
                    }
                    else
                    {
                        nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day);
                    }
                }
            }
            else
            {
                if (nextDOB.Month < DateTime.Now.Month)
                {
                    nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day);
                }
                else if (nextDOB.Month == DateTime.Now.Month)
                {
                    if (nextDOB.Day < DateTime.Now.Day)
                    {
                        nextDOB = new DateTime(DateTime.Now.Year + 1, nextDOB.Month, nextDOB.Day);
                    }
                    else
                    {
                        nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day);
                    }
                }
                else
                {
                    nextDOB = new DateTime(DateTime.Now.Year, nextDOB.Month, nextDOB.Day);
                }
            }
            return nextDOB;
        }
    }
}
