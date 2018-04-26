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
            if (context.InputParameters.Contains("Target")) {
                // Obtain the target entity from the input parameters.

                Entity targetEntity = context.InputParameters["Target"] as Entity;
                EntityReference targetReference = context.InputParameters["Target"] as EntityReference;

                if (targetEntity?.LogicalName == "new_specialentity" || targetReference?.LogicalName == "new_specialentity")// audit entity
                    return;

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
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

                Entity auditEntity = null;
                //create operation
                #region Create operation
                if (context.PostEntityImages.Contains("create")) {
                    //if not created
                    if (!context.OutputParameters.Contains("id")) {
                        throw new InvalidPluginExecutionException("can't see id in output");
                    }

                    //Entity preEntity = (Entity)context.PreEntityImages["create"];
                    Entity postEntity = (Entity)context.PostEntityImages["create"];

                    auditEntity = new Entity("new_specialentity");
                    auditEntity["new_name"] = targetEntity.LogicalName;
                    auditEntity["new_spaction"] = "created";
                    //author
                    Guid authorId = new Guid(context.InitiatingUserId.ToString());
                    string authorType = "systemuser";
                    auditEntity["new_spauthor"] = new EntityReference(authorType, authorId);
                    //name
                    //Entity details = service.Retrieve(entity.LogicalName, entity.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet("name"));
                    //string accountName = details.Attributes.Contains("name") ? ((String)details["name"]) : "empty name";
                    string name = "empty name";
                    name = extractName(postEntity);

                    auditEntity["new_spentity"] = name;

                    string new_spdescription = createString(postEntity);
                    auditEntity["new_spdescription"] = new_spdescription;
                    //
                }
                #endregion

                //delete operation
                #region Delete operation
                if (context.PreEntityImages.Contains("delete")) {

                    Entity preEntity = (Entity)context.PreEntityImages["delete"];

                    auditEntity = new Entity("new_specialentity");
                    auditEntity["new_name"] = targetReference.LogicalName;
                    auditEntity["new_spaction"] = "deleted";
                    //author
                    Guid authorId = new Guid(context.InitiatingUserId.ToString());
                    string authorType = "systemuser";
                    auditEntity["new_spauthor"] = new EntityReference(authorType, authorId);
                    //name
                    //Entity details = service.Retrieve(entity.LogicalName, entity.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet("name"));
                    //string accountName = details.Attributes.Contains("name") ? ((String)details["name"]) : "empty name";

                    auditEntity["new_spentity"] = extractName(preEntity);
                    //

                }
                #endregion

                //update operation
                #region Update operation
                if (context.PreEntityImages.Contains("update")) {

                    string new_spdescription = "";

                    Entity preEntity = (Entity)context.PreEntityImages["update"];
                    Entity postEntity = (Entity)context.PostEntityImages["update"];
                    new_spdescription = updateString(preEntity, postEntity);

                    auditEntity = new Entity("new_specialentity");
                    auditEntity["new_name"] = targetEntity.LogicalName;
                    auditEntity["new_spaction"] = "updated";
                    //author
                    Guid authorId = new Guid(context.InitiatingUserId.ToString());
                    string authorType = "systemuser";
                    auditEntity["new_spauthor"] = new EntityReference(authorType, authorId);
                    //name
                    //Entity details = service.Retrieve(entity.LogicalName, entity.Id,new Microsoft.Xrm.Sdk.Query.ColumnSet("name"));
                    //string accountName = details.Attributes.Contains("name") ? ((String)details["name"]) : "empty name";

                    auditEntity["new_spentity"] = extractName(postEntity);
                    auditEntity["new_spdescription"] = new_spdescription;
                }
                #endregion


                if (auditEntity != null) {
                    try {
                        // Plug-in business logic goes here.  
                        service.Create(auditEntity);
                    } catch (FaultException<OrganizationServiceFault> ex) {
                        throw new InvalidPluginExecutionException("An error occurred in MyPlugin.", ex);
                    } catch (Exception ex) {
                        tracingService.Trace("MyPlugin: {0}", ex.ToString());
                        throw;
                    }
                }
            }

        }

        private string createString(Entity postEntity) {
            if (postEntity == null)
                return "";
            StringBuilder sb = new StringBuilder("");
            int counter = 0;
            foreach(var kvp in postEntity.Attributes) {

                if (kvp.Value == null)
                    continue;
                counter++;

                if (sb.Length > 0) sb.Append("; ");
                sb.Append($"{kvp.Key}");
                //sb.Append($"[{kvp.Value.GetType().Name}]");
                sb.Append($"[{stringVal(kvp.Value)}]");
                if (counter >= 100)
                    break;
            }
            return sb.ToString();
        }
        private static string stringVal(object val) {
            if (val == null)
                return "null";
            //return "";
            if (val is bool || val is int || val is decimal || val is string || val is Guid) {
                return val.ToString();
            } else if (val is DateTime dtVal) {
                return dtVal.ToShortDateString();
            } else if (val is EntityReference erVal) {
                return $"{erVal.Name}({erVal.Id.ToString()}/{erVal.LogicalName})";
            } else if (val is Money mVal) {
                return mVal.Value.ToString();
            } else if (val is OptionSetValue osVal) {
                return osVal.Value.ToString();
            } else
                return "";

        }

        private static string updateString(Entity preEntity, Entity postEntity) {
            StringBuilder sb = new StringBuilder();

            var allKeys = preEntity.Attributes.Keys.Union(postEntity.Attributes.Keys);
            var diffKeys = allKeys
                .Where(k => stringVal((preEntity.Contains(k)) ? preEntity.Attributes[k]:null) !=
                            stringVal((postEntity.Contains(k)) ? postEntity.Attributes[k] : null))
                .Take(100); 
            foreach(var k in diffKeys) {
                if (sb.Length > 0) sb.Append("\n");
                sb.Append($"{k} [{stringVal(preEntity.Contains(k)?preEntity.Attributes[k]:null)}]" +
                    $"-[{stringVal(postEntity.Contains(k)?postEntity.Attributes[k]:null)}]");
            }
            return sb.ToString();
        }

        private static string extractName(Entity postEntity) {
            string name = "empty name";
            if (postEntity.Contains("name")) {
                name = postEntity.GetAttributeValue<string>("name");
            } else if (postEntity.Contains("fullname")) {
                name = postEntity.GetAttributeValue<string>("fullname");
            }
            return name;
        }
    }
}
