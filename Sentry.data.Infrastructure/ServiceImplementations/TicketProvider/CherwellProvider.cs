using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Sentry.Configuration;
using Sentry.data.Core;
using Sentry.data.Infrastructure.CherwellService;
using Sentry.Common.Logging;

namespace Sentry.data.Infrastructure
{
    public class CherwellProvider : ITicketProvider
    {
        private BusinessObjectClient _businessObjectClient;
        private ServiceClient _tokenClient;
        private SearchesClient _searchesClient;
        private readonly string _apiUrl;
        private readonly string _clientId;
        private string _token;
        private DateTime _tokenTimer;
        private readonly object _tokenLockObject = new object();
        private readonly object _bussinessObjectClientLock = new object();
        private readonly object _searchesClientLock = new object();
        private readonly object _tokenClientLock = new object();
        private readonly int _tokenInterval;

        public CherwellProvider()
        {
            _apiUrl = Config.GetHostSetting("CherwellApiUrl");
            _clientId = Config.GetHostSetting("CherwellClientId");
            _tokenInterval = int.Parse(Config.GetHostSetting("CherwellTokenInterval"));
        }

        #region Public Methods
        public Task<string> CreateTicketAsync(AccessRequest request)
        {
            try
            {
                string newBusPublicObId = CreateNewChangeTicket(GlobalConstants.CherwellBusinessObjectNames.CHANGE_REQUEST, request);

                AddApproversToTicket(newBusPublicObId, request);

                ChangeStatus(newBusPublicObId, GlobalConstants.CherwellChangeStatusNames.WAITING_FOR_APPROVAL, GlobalConstants.CherwellChangeStatusOrder.WAITING_FOR_APPROVAL);

                return Task.FromResult(newBusPublicObId);
            }
            catch (Exception ex)
            {
                Logger.Error("Could not submit access request to Cherwell", ex);
                return Task.FromResult(string.Empty);
            }            
        }

        public Task<ChangeTicket> RetrieveTicketAsync(string ticketId)
        {
            ChangeTicket ticket = null;
            try
            {
                ReadResponse response = GetBusinessObjectByPublicId(ticketId);
                ticket = MapToHpsmTicket(response);
                ticket.TicketId = ticketId;
            }
            catch (Exception ex)
            {
                Logger.Error($"cherwell_retrieveTicket_failed {ticketId}", ex);
            }

            return Task.FromResult(ticket);
        }

        private ChangeTicket MapToHpsmTicket(ReadResponse response)
        {
            ChangeTicket ticket = new ChangeTicket()
            {
                PreApproved = false,
                ApprovedById = null,
                RejectedById = null
            };

            string ticketStatus = response.Fields.First(w => w.Name == "Status").Value;

            //If ticket is not approved, it will be moved back to Logging and Prep
            if (ticketStatus == GlobalConstants.CherwellChangeStatusNames.LOGGING_AND_PREP)
            {
                ticket.TicketStatus = GlobalConstants.ChangeTicketStatus.DENIED;
            }
            else if (ticketStatus == GlobalConstants.CherwellChangeStatusNames.IMPLEMENTING)
            {
                ticket.TicketStatus = GlobalConstants.ChangeTicketStatus.APPROVED;
            }
            else if (ticketStatus == GlobalConstants.CherwellChangeStatusNames.CLOSED)
            {
                ticket.TicketStatus = GlobalConstants.ChangeTicketStatus.WITHDRAWN;
            }
            else { return null; }

            return ticket;
        }

        public Task CloseTicketAsync(ChangeTicket ticket)
        {
            ChangeStatus(ticket.TicketId, GlobalConstants.CherwellChangeStatusNames.CLOSED, GlobalConstants.CherwellChangeStatusOrder.CLOSED);
            return Task.CompletedTask;
        }
        #endregion


        #region Clients

        private BusinessObjectClient BusinessObjectClient
        {
            get
            {
                lock (_bussinessObjectClientLock)
                {
                    if (_businessObjectClient is null || (DateTime.Now - _tokenTimer).TotalSeconds > _tokenInterval)
                    {
                        Logger.Info("cherwell_initialize_busobjclient");
                        RefreshToken();

                        var httpClientHandler = new HttpClientHandler() { UseDefaultCredentials = true };
                        var client = new HttpClient(httpClientHandler)
                        {
                            Timeout = new TimeSpan(0, 0, 30),
                            BaseAddress = new Uri(_apiUrl)
                        };

                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);

                        _businessObjectClient = new BusinessObjectClient(client);
                        _businessObjectClient.BaseUrl = _apiUrl;
                    }
                }                
                return _businessObjectClient;
            }
        }
        private SearchesClient SearchesClient
        {
            get
            {
                lock (_searchesClientLock)
                {
                    if (_searchesClient is null || (DateTime.Now - _tokenTimer).TotalSeconds > _tokenInterval)
                    {
                        Logger.Info("cherwell_initialize_searchesclient");
                        RefreshToken();

                        var httpClientHandler = new HttpClientHandler() { UseDefaultCredentials = true };
                        var client = new HttpClient(httpClientHandler)
                        {
                            Timeout = new TimeSpan(0, 0, 30),
                            BaseAddress = new Uri(_apiUrl)
                        };

                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);

                        _searchesClient = new SearchesClient(client);
                        _searchesClient.BaseUrl = _apiUrl;
                    }
                }
                return _searchesClient;
            }
        }
        private ServiceClient TokenClient
        {
            get
            {
                lock (_tokenClientLock)
                {
                    if (_tokenClient is null)
                    {
                        Logger.Info("cherwell_initialize_tokenclient");

                        var httpClientHandler = new HttpClientHandler() { UseDefaultCredentials = true };
                        var client = new HttpClient(httpClientHandler)
                        {
                            Timeout = new TimeSpan(0, 0, 30),
                            BaseAddress = new Uri(_apiUrl)
                        };

                        _tokenClient = new ServiceClient(client);
                        _tokenClient.BaseUrl = _apiUrl;
                    }
                }
                return _tokenClient;
            }
        }
        private async Task<TokenResponse> TokenAsync()
        {
            try
            {
                Logger.Info("cherwell_retrieve_token");
                TokenResponse response = await TokenClient.TokenAsync("password", _clientId,
                null, Config.GetHostSetting("ServiceAccountID"), Config.GetHostSetting("ServiceAccountPassword"), null, Auth_mode.LDAP, null).ConfigureAwait(false);
                Logger.Info("cherwell_retrieve_token_success");
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("cherwell_retrieve_token_failed", ex);
                throw;
            }
        }
        #endregion


        #region Private Methods
        private void RefreshToken()
        {
            lock (_tokenLockObject)
            {
                if ((DateTime.Now - _tokenTimer).TotalSeconds > 30)
                {
                    Logger.Info("cherwell_refreshing_token");
                    var token = TokenAsync().ConfigureAwait(false);
                    _token = token.GetAwaiter().GetResult().Access_token;
                    _tokenTimer = DateTime.Now;
                }
            }
        }

        private void AddApproversToTicket(string busPublicObId, AccessRequest model)
        {
            Summary busObj = GetBusinessObjectSummaryByName(GlobalConstants.CherwellBusinessObjectNames.APPROVAL);
            TemplateResponse templateResponse = GetBusinessObjectTemplate(busObj.BusObId);
            ReadResponse response = GetBusinessObjectByPublicId(busPublicObId);

            AddApproverToTemplate(response.BusObRecId, templateResponse, model);

            var saveApprovalRequest = new SaveRequest
            {
                BusObId = busObj.BusObId,
                Fields = templateResponse.Fields
            };

            SaveBusinessObject(saveApprovalRequest);
        }

        private SaveResponse SaveBusinessObject(SaveRequest saveReq)
        {
            var saveResponse = SaveBusinessObjectV1Async(saveReq).ConfigureAwait(false);
            return saveResponse.GetAwaiter().GetResult();
        }

        private string CreateNewChangeTicket(string templateName, AccessRequest model)
        {
            Logger.Info("cherwell_entered_getsaverequest");

            Summary busObj = GetBusinessObjectSummaryByName(templateName);
            Logger.Info($"cherwell_getsaverequest_recieved_busobjsummary - {busObj.DisplayName},{busObj.BusObId}");

            TemplateResponse changeTemplateResponse = GetBusinessObjectTemplate(busObj.BusObId);
            Logger.Info($"cherwell_getsaverequest_recieved_busobjtemplate");

            ToChangeTemplateResponse(model, changeTemplateResponse);
            SaveRequest changeRequestSaveReq = new SaveRequest()
            {
                BusObId = busObj.BusObId,
                Fields = changeTemplateResponse.Fields
            };

            return SaveBusinessObject(changeRequestSaveReq).BusObPublicId;
        }

        private SaveRequest GetSaveRequest(ReadResponse existingTicket)
        {
            Logger.Info("cherwell_entered_getsaverequest");
            TemplateResponse changeTemplateResponse = GetBusinessObjectTemplate(existingTicket.BusObId);
            Logger.Info($"cherwell_getsaverequest_recieved_busobjtemplate");

            MapTicketToTemplate(existingTicket, changeTemplateResponse);

            return new SaveRequest()
            {
                BusObId = existingTicket.BusObId,
                BusObPublicId = existingTicket.BusObPublicId,
                BusObRecId = existingTicket.BusObRecId,
                Fields =  changeTemplateResponse.Fields
            };
        }

        private TemplateResponse GetBusinessObjectTemplate(string busObId)
        {
            TemplateRequest tempReq = new TemplateRequest()
            {
                BusObId = busObId,
                IncludeAll = true,
                IncludeRequired = true
            };
            var response = GetBusinessObjectTemplateV1Async(tempReq).ConfigureAwait(false);
            return response.GetAwaiter().GetResult();
        }

        private Summary GetBusinessObjectSummaryByName(string templateName)
        {
            var summaryList = GetBusinessObjectSummaryByNameV1Async(templateName).ConfigureAwait(false);
            Logger.Info("received_cherwell_getbusobjsummary_request");
            return summaryList.GetAwaiter().GetResult().ToList()[0];
        }

        private List<ReadResponse> GetUserInfo(string id)
        {
            Summary UserInfoSummary = GetBusinessObjectSummaryByName(GlobalConstants.CherwellBusinessObjectNames.USER);
            SchemaResponse schemaResponse = GetBusinessObjectSchema(UserInfoSummary.BusObId);

            var searchResultsRequest = new SearchResultsRequest();
            searchResultsRequest.BusObId = UserInfoSummary.BusObId;
            searchResultsRequest.Filters = new List<FilterInfo>();
            var filterInfo = new FilterInfo
            {
                FieldId =
                schemaResponse.FieldDefinitions.First(f => f.Name == "EmployeeID").FieldId,
                Operator = "eq",
                Value = id
            };
            searchResultsRequest.Filters.Add(filterInfo);

            return GetSearchResults(searchResultsRequest).BusinessObjects.ToList();
        }

        private ReadResponse GetCustomerInfo(string id)
        {
            Summary CustomerInfoSummary = GetBusinessObjectSummaryByName(GlobalConstants.CherwellBusinessObjectNames.CUSTOMER);
            SchemaResponse schemaResponse = GetBusinessObjectSchema(CustomerInfoSummary.BusObId);

            var searchResultsRequest = new SearchResultsRequest();
            searchResultsRequest.BusObId = CustomerInfoSummary.BusObId;
            searchResultsRequest.Filters = new List<FilterInfo>();
            var filterInfo = new FilterInfo
            {
                FieldId =
                schemaResponse.FieldDefinitions.First(f => f.Name == "EmployeeID").FieldId,
                Operator = "eq",
                Value = id
            };
            searchResultsRequest.Filters.Add(filterInfo);

            SearchResultsResponse searchResponse = GetSearchResults(searchResultsRequest);
            List<ReadResponse> searchResponseList = searchResponse.BusinessObjects.ToList();

            return GetBusinessObjectByRecId(searchResponseList[0].BusObId, searchResponseList[0].BusObRecId);

        }

        private SearchResultsResponse GetSearchResults(SearchResultsRequest searchResultsRequest)
        {
            var searchResults = GetSearchResultsAdHocV1Async(searchResultsRequest).ConfigureAwait(false);
            return searchResults.GetAwaiter().GetResult();
        }

        private SchemaResponse GetBusinessObjectSchema(string busObId)
        {
            var schemaResponse = GetBusinessObjectSchemaV1Async(busObId).ConfigureAwait(false);
            return schemaResponse.GetAwaiter().GetResult();
        }

        private ReadResponse GetBusinessObjectByRecId(string busObId, string busObRecId)
        {
            var readResponse = GetBusinessObjectByRecIdV1Async(busObId, busObRecId).ConfigureAwait(false);
            return readResponse.GetAwaiter().GetResult();
        }

        private ReadResponse GetBusinessObjectByPublicId(string publicId)
        {
            Summary summary = GetBusinessObjectSummaryByName(GlobalConstants.CherwellBusinessObjectNames.CHANGE_REQUEST);
            var readResponse = BusinessObjectClient.GetBusinessObjectByPublicIdV1Async(summary.BusObId, publicId).ConfigureAwait(false);
            return readResponse.GetAwaiter().GetResult();
        }

        private void ChangeStatus(string busObPublicId, string statusName, string orderid)
        {
            ReadResponse response = GetBusinessObjectByPublicId(busObPublicId);
            SaveRequest saveReq = GetSaveRequest(response);
            SetFieldValue(saveReq.Fields, "Status", statusName);
            SetFieldValue(saveReq.Fields, "OrderStatus", orderid);
            SaveBusinessObject(saveReq);
        }

        #endregion


        #region Prviate API Endpoint Calls
        private async Task<ICollection<Summary>> GetBusinessObjectSummaryByNameV1Async(string name)
        {
            try
            {
                Logger.Info("sending_cherwell_getbusobjsummary_request");
                return await BusinessObjectClient.GetBusinessObjectSummaryByNameV1Async(name).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error("cherwell_getbusobjsummary_request_failed", ex);
                throw;
            }
        }

        private async Task<TemplateResponse> GetBusinessObjectTemplateV1Async(TemplateRequest request)
        {
            try
            {
                Logger.Info("sending_cherwell_getbusobjtemplate_request");
                return await BusinessObjectClient.GetBusinessObjectTemplateV1Async(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error("cherwell_getbusobjtemplate_request_failed", ex);
                throw;
            }
        }

        private async Task<SchemaResponse> GetBusinessObjectSchemaV1Async(string busObId)
        {
            try
            {
                Logger.Info("sending_cherwell_getbusobjschema_request");
                return await BusinessObjectClient.GetBusinessObjectSchemaV1Async(busObId, false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error("cherwell_getbusobjschema_request_failed", ex);
                throw;
            }
        }

        private async Task<SearchResultsResponse> GetSearchResultsAdHocV1Async(SearchResultsRequest searchRequest)
        {
            try
            {
                Logger.Info("sending_cherwell_getsearchresult_request");
                return await SearchesClient.GetSearchResultsAdHocV1Async(searchRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error("cherwell_getsearchresult_request_failed", ex);
                throw;
            }
        }

        private async Task<SaveResponse> SaveBusinessObjectV1Async(SaveRequest request)
        {
            try
            {
                Logger.Info("sending_cherwell_savebusobj_request");
                return await BusinessObjectClient.SaveBusinessObjectV1Async(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error("cherwell_savebusobj_request_failed", ex);
                throw;
            }
        }

        private async Task<ReadResponse> GetBusinessObjectByRecIdV1Async(string busObjId, string busObjRecId)
        {
            try
            {
                Logger.Info("sending_cherwell_getbusobjbyrecid_request");
                return await BusinessObjectClient.GetBusinessObjectByRecIdV1Async(busObjId, busObjRecId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error("cherwell_getbusobjbyrecid_request_failed", ex);
                throw;
            }
        }

        #endregion


        #region Extensions
        private void ToChangeTemplateResponse(AccessRequest model, TemplateResponse response)
        {
            SetFieldValue(response.Fields, "Title", BuildTitleByTemplate(model));
            SetFieldValue(response.Fields, "Description", BuildBodyByTemplate(model).ToString());
            SetFieldValue(response.Fields, "ProposedStartDate", DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"));
            SetFieldValue(response.Fields, "ScheduledEndDate", DateTime.Now.Add(TimeSpan.FromDays(14)).ToString("MM/dd/yyyy hh:mm tt"));

            var customerInfoResponse = GetCustomerInfo(model.RequestorsId);

            SetFieldValue(response.Fields, "RequestedBy", customerInfoResponse.Fields.FirstOrDefault(w => w.Name == "FullName").Value);
            SetFieldValue(response.Fields, "OwnedByTeam", "BI Portal Administration");

            var userInfoResponse = GetUserInfo("072984");
            SetFieldValue(response.Fields, "OwnedBy", userInfoResponse[0].Fields.FirstOrDefault(w => w.Name == "FullName").Value);

            //Standard or Normal
            SetFieldValue(response.Fields, "Type", "Standard");
            SetFieldValue(response.Fields, "ApprovalLock", "true");
            //We are always using Routine
            SetFieldValue(response.Fields, "RiskUrgency", "Routine");
            //The following three properties are needed to ensure the change details show on the UI
            SetFieldValue(response.Fields, "EmbeddedFormToggle", "Classify Standard");
            SetFieldValue(response.Fields, "EmbeddedFormDisplay", "Classify Standard");
            SetFieldValue(response.Fields, "ExpandedForm", "Classify Standard");
        }

        private void MapTicketToTemplate(ReadResponse existingTicket, TemplateResponse response)
        {
            foreach(FieldTemplateItem field in response.Fields)
            {
                if (existingTicket.Fields.Any(a => a.Name == field.Name && !String.IsNullOrEmpty(field.Value)))
                {
                    SetFieldValue(response.Fields, field.Name, existingTicket.Fields.First(w => w.Name == field.Name).Value);
                }
            }
        }

        private StringBuilder BuildBodyByTemplate(AccessRequest model)
        {
            StringBuilder sb = new StringBuilder();
            switch (model.Type)
            {
                case AccessRequestType.AwsArn:
                    if (model.IsAddingPermission)
                    {
                        sb.Append($"Please grant the AWS ARN {model.AwsArn} the following permissions to {(model.Scope == AccessScope.Asset ? model.SaidKeyCode : model.SecurableObjectName)}{(model.Scope != AccessScope.Asset ? $" ({model.SecurableObjectNamedEnvironment})" : String.Empty)} data. <br>");
                    }
                    else
                    {
                        sb.Append($"Please remove the following permissions for the AWS ARN {model.AwsArn} from {(model.Scope == AccessScope.Asset ? model.SaidKeyCode : model.SecurableObjectName)}{(model.Scope != AccessScope.Asset ? $" ({model.SecurableObjectNamedEnvironment})" : String.Empty)} data. <br>");
                    }

                    foreach (Permission item in model.Permissions)
                    {
                        sb.Append($"<li>{item.PermissionName} - {item.PermissionDescription} </li>");
                    }

                    sb.Append($"</ul>");
                    sb.Append($"<br>");
                    break;
                case AccessRequestType.SnowflakeAccount:
                    if (model.IsAddingPermission)
                    {
                        sb.Append($"Please grant the Snowflake Account {model.SnowflakeAccount} the following permissions to {(model.Scope == AccessScope.Asset ? model.SaidKeyCode : model.SecurableObjectName)}{(model.Scope != AccessScope.Asset ? $" ({model.SecurableObjectNamedEnvironment})" : String.Empty)} data. <br>");
                    }
                    else
                    {
                        sb.Append($"Please remove the following permissions for the Snowflake Account {model.SnowflakeAccount} from {(model.Scope == AccessScope.Asset ? model.SaidKeyCode : model.SecurableObjectName)}{(model.Scope != AccessScope.Asset ? $" ({model.SecurableObjectNamedEnvironment})" : String.Empty)} data. <br>");
                    }

                    foreach (Permission item in model.Permissions)
                    {
                        sb.Append($"<li>{item.PermissionName} - {item.PermissionDescription} </li>");
                    }

                    sb.Append($"</ul>");
                    sb.Append($"<br>");
                    break;
                case AccessRequestType.Inheritance:
                    sb.Append($"Please {(model.IsAddingPermission ? "enable" : "disable")} inheritance for dataset {model.SecurableObjectName} from Data.Sentry.com. {(model.IsAddingPermission ? "Enabling" : "Disabling")} inheritance will {(model.IsAddingPermission ? "allow" : "prevent")} the dataset {(model.IsAddingPermission ? "to" : "from")} {(model.IsAddingPermission ? "inherit" : "inheriting")} permissions from its parent asset {model.SaidKeyCode}.");
                    sb.Append($" When approved, users with access to {model.SaidKeyCode} in Data.Sentry.com {(model.IsAddingPermission ? "will" : "will not")} have access to {model.SecurableObjectName} data.");
                    sb.Append($"<br>");
                    sb.Append($"For more information on Authorization in DSC - <a href='https://confluence.sentry.com/pages/viewpage.action?pageId=361734893'>Auth Guide</a>");
                    sb.Append($"<br>");
                    if (!string.IsNullOrEmpty(model.SaidKeyCode))
                    {
                        sb.Append($"Said Asset: {model.SaidKeyCode} <br>");
                        sb.Append($"<br>");
                    }
                    sb.Append($"<ul>");
                    foreach (Permission item in model.Permissions)
                    {
                        sb.Append($"<li>{item.PermissionName} - {item.PermissionDescription} </li>");
                    }
                    sb.Append($"</ul>");
                    sb.Append($"<br>");
                    break;
                default:
                    if (model.IsAddingPermission)
                    {
                        sb.Append($"Please grant {(model.AdGroupName ?? model.PermissionForUserName)} the following permissions to {(model.Scope == AccessScope.Asset ? model.SaidKeyCode : model.SecurableObjectName)}{(model.Scope != AccessScope.Asset ? $" ({model.SecurableObjectNamedEnvironment})" : String.Empty )} in Data.sentry.com. <br>");
                    }
                    else
                    {
                        sb.Append($"Please remove the following permissions to {model.SecurableObjectName} in Data.sentry.com. <br>");
                    }
                    sb.Append($"<br>");
                    if (!string.IsNullOrEmpty(model.SaidKeyCode))
                    {
                        sb.Append($"Said Asset: {model.SaidKeyCode} <br>");
                        sb.Append($"<br>");
                    }
                    sb.Append($"<ul>");
                    foreach (Permission item in model.Permissions)
                    {
                        sb.Append($"<li>{item.PermissionName} - {item.PermissionDescription} </li>");
                    }
                    sb.Append($"</ul>");
                    sb.Append($"<br>");

                    break;
            }
            sb.Append($"Business Reason: {model.BusinessReason} <br>");
            sb.Append($"Requestor: {model.RequestorsId} - {model.RequestorsName} <br>");
            sb.Append($"DSC Environment: {Config.GetHostSetting("WebApiUrl").Replace("http://", "")}");
            return sb;
        }

        private string BuildTitleByTemplate(AccessRequest model)
        {
            switch (model.Type)
            {
                case AccessRequestType.AwsArn:
                    return $"Access {(model.IsAddingPermission ? "" : "Removal")} Request for AWS ARN {model.AwsArn}";
                case AccessRequestType.SnowflakeAccount:
                    return $"Access {(model.IsAddingPermission ? "" : "Removal")} Request for Snowflake Account {model.SnowflakeAccount}";
                case AccessRequestType.Inheritance:
                    return $"Inheritance {(model.IsAddingPermission ? "enable" : "disable")} request for {model.SecurableObjectName}";
                default:
                    if (model.AdGroupName != null)
                    {
                        return $"Access Request for AD Group {model.AdGroupName}";
                    }
                    else
                    {
                        return $"Access Request for user {model.PermissionForUserName}";
                    }
            }
        }


        private void AddApproverToTemplate(string ticketBusObRecId, TemplateResponse templateResponse, AccessRequest model)
        {
            var customerInfoResponse = GetCustomerInfo(model.ApproverId);

            SetFieldValue(templateResponse.Fields, "ApproverName", customerInfoResponse.Fields.FirstOrDefault(w => w.Name == "FullName").Value);
            SetFieldValue(templateResponse.Fields, "ApproverID", customerInfoResponse.Fields.FirstOrDefault(w => w.Name == "RecID").Value);
            SetFieldValue(templateResponse.Fields, "Details", "Approval Detailed reason");
            SetFieldValue(templateResponse.Fields, "Deadline", DateTime.Now.Add(TimeSpan.FromDays(3)).ToString("MM/dd/yyyy hh:mm tt"));
            SetFieldValue(templateResponse.Fields, "ParentTypeName", "Change Request");
            SetFieldValue(templateResponse.Fields, "ParentRecID", ticketBusObRecId);
            SetFieldValue(templateResponse.Fields, "ApproverSelectionType", "Customer");
        }

        static void SetFieldValue(ICollection<FieldTemplateItem> fields, string fieldName, string fieldValue)
        {
            if (fields.Any(s => s.Name.Equals(fieldName)))
            {
                var fieldTemplate = fields.First(s => s.Name.Equals(fieldName));
                fieldTemplate.Value = fieldValue;
                fieldTemplate.Dirty = true;
            }
            else
            {
                var newField = new FieldTemplateItem()
                {
                    Name = fieldName,
                    Value = fieldValue,
                    Dirty = true
                };

                fields.Add(newField);
            }
        }
        #endregion
    }
}
