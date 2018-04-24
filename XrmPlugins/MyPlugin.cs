using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace XrmPlugins {
    public class MyPlugin : IPlugin {
        public void Execute(IServiceProvider serviceProvider) {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            // If you are not registering the plug-in in the sandbox, then you do  
            // not have to add any tracing service related code.  
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            //if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity) {
            // Obtain the target entity from the input parameters.  


            // Verify that the target entity represents an entity type you are expecting.   
            // For example, an account. If not, the plug-in was not registered correctly.  
            Entity entity = (Entity)context.InputParameters["Target"];
            if (entity.LogicalName == "new_specialentity")// audit entity
                return;
            #region sample

            //it was in sample
            //{
            //    Entity followupTask = new Entity("task");
            //    followupTask["subject"] = "send e-mail to the new customer";
            //    followupTask["description"] = "follow up with the customer";
            //    followupTask["scheduledstart"] = DateTime.Now.AddDays(7);
            //    followupTask["scheduledend"] = DateTime.Now.AddDays(7);
            //    followupTask["category"] = context.PrimaryEntityName;
            //    if (context.OutputParameters.Contains("id")) {
            //        Guid regardingId = new Guid(context.OutputParameters["id"].ToString());
            //        string regardingType = "account";
            //        followupTask["regardingobjectid"] = new EntityReference(regardingType, regardingId);
            //    }
            //    // Obtain the organization service reference which you will need for  
            //    // web service calls.  
            //    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            //    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            //    try {
            //        // Plug-in business logic goes here.  
            //        service.Create(followupTask);
            //    } catch (FaultException<OrganizationServiceFault> ex) {
            //        throw new InvalidPluginExecutionException("An error occurred in MyPlug-in.", ex);
            //    } catch (Exception ex) {
            //        tracingService.Trace("MyPlugin: {0}", ex.ToString());
            //        throw;
            //    }

            //} 
            #endregion



            //create operation
            #region Create operation
            if (context.PostEntityImages.Contains("create")) {
                //if not created
                if (!context.OutputParameters.Contains("id")) {
                    throw new InvalidPluginExecutionException("can't see id in output");
                }

                //Entity preEntity = (Entity)context.PreEntityImages["create"];
                Entity postEntity = (Entity)context.PostEntityImages["create"];


                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


                Entity auditEntity = new Entity("new_specialentity");
                auditEntity["new_name"] = entity.LogicalName;
                auditEntity["new_spaction"] = "created";
                //author
                Guid authorId = new Guid(context.InitiatingUserId.ToString());
                string authorType = "systemuser";
                auditEntity["new_spauthor"] = new EntityReference(authorType, authorId);
                //name
                //Entity details = service.Retrieve(entity.LogicalName, entity.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet("name"));
                //string accountName = details.Attributes.Contains("name") ? ((String)details["name"]) : "empty name";
                string name = "empty name";
                if (postEntity.Contains("name"))
                    name = postEntity.GetAttributeValue<string>("name");

                auditEntity["new_spentity"] = name;
                //
                try {
                    // Plug-in business logic goes here.  
                    service.Create(auditEntity);
                } catch (FaultException<OrganizationServiceFault> ex) {
                    throw new InvalidPluginExecutionException("An error occurred in MyPlugin.", ex);
                } catch (Exception ex) {
                    tracingService.Trace("MyPlugin: {0}", ex.ToString());
                    throw;
                }
            } else {
                throw new InvalidPluginExecutionException("can't see postimage");
            }
            #endregion






            //}




            try {
                // Plug-in business logic goes here.  
            } catch (FaultException<OrganizationServiceFault> ex) {
                throw new InvalidPluginExecutionException("An error occurred in MyPlug-in.", ex);
            } catch (Exception ex) {
                tracingService.Trace("MyPlugin: {0}", ex.ToString());
                throw;
            }

        }
    }
}
