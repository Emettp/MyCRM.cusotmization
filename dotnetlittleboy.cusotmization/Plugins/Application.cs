namespace dotnetlittleboy.cusotmization.Plugins
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;
    using System;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="Application" />.
    /// </summary>
    public class Application : IPlugin
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
                    if (context.MessageName.ToLower() != "create" && context.Stage != 20)
                    {
                        return;
                    }
                    Entity targetEntity = context.InputParameters["Target"] as Entity;
                    Entity updateAutoNumberConfig = new Entity("dnlb_autonumberconfiguration");
                    StringBuilder autoNumber = new StringBuilder();
                    string prefix, suffix, seperator, current, year, month, day;
                    DateTime today = DateTime.Now;
                    day = today.Day.ToString("00");
                    month = today.Month.ToString("00");
                    year = today.Year.ToString();

                    QueryExpression qeAutoNumberConfig = new QueryExpression()
                    {
                        EntityName = "dnlb_autonumberconfiguration",
                        ColumnSet = new ColumnSet("dnlb_prefix", "dnlb_suffix", "dnlb_seperator", "dnlb_currentnumber", "dnlb_name")
                    };

                    EntityCollection ecAutoNumberConfig = service.RetrieveMultiple(qeAutoNumberConfig);
                    if (ecAutoNumberConfig.Entities.Count == 0)
                    {
                        return;
                    }
                    foreach (Entity entity in ecAutoNumberConfig.Entities)
                    {
                        if (entity.Attributes["dnlb_name"].ToString().ToLower() == "applicationautonumber")
                        {
                            prefix = entity.GetAttributeValue<string>("dnlb_prefix");
                            suffix = entity.GetAttributeValue<string>("dnlb_suffix");
                            seperator = entity.GetAttributeValue<string>("dnlb_seperator");
                            current = entity.GetAttributeValue<string>("dnlb_currentnumber");
                            int tempCurrent = int.Parse(current);
                            tempCurrent++;
                            current = tempCurrent.ToString("000000");
                            updateAutoNumberConfig.Id = entity.Id;
                            updateAutoNumberConfig["dnlb_currentnumber"] = current;
                            service.Update(updateAutoNumberConfig);
                            autoNumber.Append(prefix + seperator + year + month + day + seperator + suffix + seperator + current);
                            break;
                        }
                    }
                    targetEntity["dnlb_name"] = autoNumber.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
