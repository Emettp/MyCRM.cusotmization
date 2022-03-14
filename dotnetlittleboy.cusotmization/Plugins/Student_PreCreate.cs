namespace dotnetlittleboy.cusotmization.Plugins
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using System;

    /// <summary>
    /// Defines the <see cref="Student_PreCreate" />.
    /// </summary>
    public class Student_PreCreate : IPlugin
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
                    ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    if (context.MessageName.ToLower() == "create" && context.Stage == 20)
                    {
                        Entity targetEntity = context.InputParameters["Target"] as Entity;
                        string autoNumber = string.Empty;
                        if (targetEntity.LogicalName.ToLower() != "dnlb_student")
                        {
                            return;
                        }
                        autoNumber = getAutoNumber(service);
                        targetEntity["dnlb_name"] = autoNumber;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        /// <summary>
        /// The getAutoNumber.
        /// </summary>
        /// <param name="service">The service<see cref="IOrganizationService"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string getAutoNumber(IOrganizationService service)
        {
            Entity updateAutoNumber = new Entity("dnlb_autonumberconfiguration");
            string autoNumber = string.Empty, prefix, suffix, seperator, currentNumber, year, month, day;
            DateTime today = DateTime.Now;
            year = today.Year.ToString();
            month = today.Month.ToString("00");
            day = today.Day.ToString("00");

            QueryExpression qeAutoNumberConfig = new QueryExpression()
            {
                EntityName = "dnlb_autonumberconfiguration",
                ColumnSet = new ColumnSet("dnlb_prefix", "dnlb_suffix", "dnlb_seperator", "dnlb_currentnumber", "dnlb_name")
            };

            EntityCollection ecAutoNumberConfig = service.RetrieveMultiple(qeAutoNumberConfig);
            if (ecAutoNumberConfig.Entities.Count != 0)
            {
                foreach (Entity entity in ecAutoNumberConfig.Entities)
                {
                    if (entity.Attributes["dnlb_name"].ToString().ToLower() == "studentautonumber")
                    {
                        prefix = entity.GetAttributeValue<string>("dnlb_prefix");
                        suffix = entity.GetAttributeValue<string>("dnlb_suffix");
                        seperator = entity.GetAttributeValue<string>("dnlb_seperator");
                        currentNumber = entity.GetAttributeValue<string>("dnlb_currentnumber");
                        int tempCurrentNumber = int.Parse(currentNumber);
                        tempCurrentNumber++;
                        currentNumber = tempCurrentNumber.ToString("000000");
                        autoNumber = (prefix + seperator + year + month + day + seperator + suffix + seperator + currentNumber);
                        updateAutoNumber.Id = entity.Id;
                        updateAutoNumber["dnlb_currentnumber"] = currentNumber;
                        service.Update(updateAutoNumber);
                        break;
                    }
                }
            }
            return autoNumber;
        }
    }
}
