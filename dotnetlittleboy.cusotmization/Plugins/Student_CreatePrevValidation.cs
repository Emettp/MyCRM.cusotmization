namespace dotnetlittleboy.cusotmization.Plugins
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using System;

    /// <summary>
    /// Defines the <see cref="Student_CreatePrevValidation" />.
    /// </summary>
    public class Student_CreatePrevValidation : IPlugin
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

                    if (context.MessageName.ToLower() != "create" && context.Stage != 10)
                    {
                        return;
                    }
                    string autoNumber = string.Empty, prefix = string.Empty, suffix = string.Empty, seperator = string.Empty, currentNumber = string.Empty, year = string.Empty, month = string.Empty, day = string.Empty;
                    DateTime today = DateTime.Now;
                    year = today.Year.ToString();
                    month = today.Month.ToString("00");
                    day = today.Day.ToString("00");

                    QueryExpression qeAutoNumberConfig = new QueryExpression()
                    {
                        EntityName = "dnlb_autonumberconfiguration",
                        ColumnSet = new ColumnSet("dnlb_name", "dnlb_prefix", "dnlb_suffix", "dnlb_seperator", "dnlb_currentnumber")
                    };
                    qeAutoNumberConfig.Criteria.AddCondition("dnlb_name", ConditionOperator.Equal, "studentautonumber");
                    EntityCollection ecAutoNumberConfig = service.RetrieveMultiple(qeAutoNumberConfig);
                    if (ecAutoNumberConfig.Entities.Count > 0)
                    {
                        Entity studentAutoNumber = ecAutoNumberConfig.Entities[0];
                        prefix = studentAutoNumber["dnlb_prefix"].ToString();
                        suffix = studentAutoNumber["dnlb_suffix"].ToString();
                        seperator = studentAutoNumber["dnlb_seperator"].ToString();
                        currentNumber = studentAutoNumber["dnlb_currentnumber"].ToString();
                        int tempCurrentNumber = int.Parse(currentNumber);
                        tempCurrentNumber++;
                        currentNumber = tempCurrentNumber.ToString("000000");
                        autoNumber = prefix + seperator + year + month + day + seperator + suffix + seperator + currentNumber;

                        QueryExpression qeStudent = new QueryExpression()
                        {
                            EntityName = "dnlb_student",
                            ColumnSet = new ColumnSet("dnlb_name")
                        };
                        qeStudent.Criteria.AddCondition("dnlb_name", ConditionOperator.Equal, autoNumber);
                        EntityCollection ecStudent = service.RetrieveMultiple(qeStudent);
                        if (ecStudent.Entities.Count > 0)
                        {
                            throw new Exception("Duplicat student found with student Id : " + autoNumber);
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
