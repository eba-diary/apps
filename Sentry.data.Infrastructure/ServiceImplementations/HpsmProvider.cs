using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Sentry.Common.Logging;
using Sentry.data.Core;
using Sentry.data.Infrastructure.HPMSChangeManagement;

namespace Sentry.data.Infrastructure
{
    public class HpsmProvider : IHpsmProvider
    {


        public HpsmProvider() { }

        private ChangeManagementClient _service;
        public ChangeManagementClient Service
        {
            get
            {
                if (_service is null)
                {
                    try
                    {
                        _service = new ChangeManagementClient();
                        _service.ClientCredentials.UserName.UserName = Configuration.Config.GetHostSetting("HpsmServiceId");
                        _service.ClientCredentials.UserName.Password = Configuration.Config.GetHostSetting("HpsmServicePassword");
                        _service.ClientCredentials.UseIdentityConfiguration = true;
                        _service.Endpoint.Address = new EndpointAddress(Configuration.Config.GetHostSetting("HpsmServiceUrl"));
                    }
                    catch(Exception ex)
                    {
                        Logger.Fatal("HPSMProvider failed loading configuration", ex);
                    }
                }
                return _service;
            }
        }

        /// <summary>
        /// Creates a new HPSM Change ticket.
        /// </summary>
        public string CreateHpsmTicket(AccessRequest model)
        {
            try
            {
                CreateChangeRequest createRequest = GetCreateRequest(model);
                CreateChangeResponse createResponse = Service.CreateChange(createRequest);

                //now lets move it to the next phase.
                MoveToNextPhaseChangeRequest moveRequest = GetMoveRequest(createResponse.model.keys.ChangeID.Value);
                MoveToNextPhaseChangeResponse moveResponse = Service.MoveToNextPhaseChange(moveRequest);

                return moveResponse.model.keys.ChangeID.Value;
            }
            catch (Exception ex)
            {
                Logger.Error("Could not submit access request", ex);
                return string.Empty;
            }

        }



        public HpsmTicket RetrieveTicket(string hpsmChangeId)
        {

            RetrieveChangeRequest request = new RetrieveChangeRequest()
            {
                model = new ChangeModelType()
                {
                    instance = new ChangeInstanceType(),
                    keys = new ChangeKeysType()
                    {
                        ChangeID = GetHpsmString(hpsmChangeId)
                    }
                },
                ignoreEmptyElements = true,
            };

            RetrieveChangeResponse response = Service.RetrieveChange(request);

            if (response != null && response.status == StatusType.SUCCESS)
            {
                return MapToTicket(response.model.instance);
            }

            return null;
        }


        public void CloseHpsmTicket(string hpsmChangeId, bool wasTicketDenied = false)
        {
            string message = wasTicketDenied ? "Denied" : "Successful";

            CloseChangeRequest request = new CloseChangeRequest()
            {
                model = new ChangeModelType()
                {
                    instance = new ChangeInstanceType()
                    {
                        ClosureComments = new ChangeInstanceTypeClosureComments()
                        {
                            type = message
                        },
                        ImplementationResult = GetHpsmString(message)
                    },
                    keys = new ChangeKeysType()
                    {
                        ChangeID = GetHpsmString(hpsmChangeId)
                    }
                },
                ignoreEmptyElements = true
            };

            Service.CloseChange(request);

        }




        private CreateChangeRequest GetCreateRequest(AccessRequest model)
        {
            CreateChangeRequest request = new CreateChangeRequest()
            {
                model = new ChangeModelType()
                {

                    instance = new ChangeInstanceType()
                    {
                        header = new ChangeInstanceTypeHeader()
                        {
                            AssignmentGroup = GetHpsmString("data.sentry.com"),
                            Category = GetHpsmString("Standard Change"),
                            RequestedStart = GetHpsmDate(model.RequestedDate.ToString()),
                            RequestedEnd = GetHpsmDate(model.RequestedDate.ToString()),
                            InitiatedBy = GetHpsmString(model.RequestorsId.ToString()),
                            ApprovalStatus = GetHpsmString("pending"),
                            Title = GetHpsmString("Access Request for AD Group " + model.AdGroupName)
                        },
                        NoCI = GetHpsmBoolean(true),
                        CriticalSys = GetHpsmString("No"),
                        TestingCompleted = GetHpsmString("Testing is not required for this change"),
                        ImplementationStart = GetHpsmDate(model.RequestedDate.ToString()),
                        ImplementationEnd = GetHpsmDate(model.RequestedDate.ToString()),
                        Urgency = GetHpsmString("Routine"),
                        ConfigState = GetHpsmString("Up"),
                        BackoutTime = GetHpsmString("No"),
                        ProdWindow = GetHpsmString("Yes"),
                        ImpactUrgency = GetHpsmString("Routine"),
                        ReleaseType = GetHpsmString("Server Builds/Build_VM"),
                        descriptionstructure = new ChangeInstanceTypeDescriptionstructure()
                        {
                            ImplementationPlan = new ChangeInstanceTypeDescriptionstructureImplementationPlan()
                            {
                                ImplementationPlan = GetHpsmStringArr("Request access to datasets within Data.sentry.com")
                            },
                            BackoutMethod = new ChangeInstanceTypeDescriptionstructureBackoutMethod()
                            {
                                BackoutMethod = GetHpsmStringArr("Close this ticket if not already approved. If approved, remove permissions through admin screen in Data.sentry.com")
                            },
                            Description = new ChangeInstanceTypeDescriptionstructureDescription()
                            {
                                Description = new StringType[] { new StringType() {Value = model.BusinessReason } }
                            }
                        }
                    },
                    keys = new ChangeKeysType()
                },
                ignoreEmptyElements = true
            };

            if (model.IsProd)
            {
                request.model.instance.Preapproved = GetHpsmBoolean(false);
                request.model.instance.ApprovalOperator = new ChangeInstanceTypeApprovalOperator()
                {
                    ApprovalOperator = GetHpsmStringArr(model.ApproverId)
                };

            }
            else
            {
                //just approve it everytime for non-prod....for now.
                request.model.instance.Preapproved = GetHpsmBoolean(true);
            }

            return request;
        }

        private MoveToNextPhaseChangeRequest GetMoveRequest(string changeId)
        {
            MoveToNextPhaseChangeRequest request = new MoveToNextPhaseChangeRequest()
            {
                model = new ChangeModelType()
                {
                    instance = new ChangeInstanceType()
                    {
                        header = new ChangeInstanceTypeHeader()
                    },
                    keys = new ChangeKeysType()
                    {
                        ChangeID = GetHpsmString(changeId)
                    }
                },
                ignoreEmptyElements = true
            };

            return request;
        }


        private HpsmTicket MapToTicket(ChangeInstanceType instance)
        {
            HpsmTicket ticket = new HpsmTicket()
            {
                PreApproved = instance.Preapproved.Value,
                ApprovedById = instance.ApprovalOperator.ApprovalOperator[0].Value,
                RejectedById = instance.ApprovalOperator.ApprovalOperator[0].Value
            };

            if (instance.header.ApprovalStatus.Value == GlobalConstants.HpsmTicketStatus.APPROVED &&
                            instance.header.Phase.Value == GlobalConstants.HpsmTicketStatus.IMPLEMENTATION)
            {
                ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.APPROVED;
            }
            //if it is closed.
            else if (instance.header.ApprovalStatus.Value == GlobalConstants.HpsmTicketStatus.APPROVED &&
                            instance.header.Phase.Value == GlobalConstants.HpsmTicketStatus.LOG_AND_PREP)
            { //since it was moved back to log and prep, it was probably declined...who knows.
                ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.DENIED;
                ticket.RejectedReason = "Ticket approval was denied";
            }
            else if(instance.header.Status.Value == GlobalConstants.HpsmTicketStatus.CLOSED)
            {
                ticket.TicketStatus = GlobalConstants.HpsmTicketStatus.WIDHTDRAWN;
                ticket.RejectedReason = instance.Comments.Comments[0].Value;

                //Lets try to parse an ID from the withdrawn comment.
                string[] words = ticket.RejectedReason.Split(' ');
                foreach( string word in words)
                {
                    var w = word.Trim().Replace(".","");
                    if (Regex.IsMatch(w, "^[0-9]{6}$"))
                    {
                        ticket.RejectedById = w;
                    }
                }
            }

            return ticket;
        }


        private StringType GetHpsmString(string str)
        {
            return new StringType { Value = str };
        }

        private StringType[] GetHpsmStringArr(string str)
        {
            string[] parts = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            List<StringType> hstrl = new List<StringType>();
            foreach (string Part in parts)
            {
                hstrl.Add(new StringType() {Value = str });
            }

            return hstrl.ToArray();
        }

        private DateTimeType GetHpsmDate(string hpsmDate)
        {
            return new DateTimeType { Value = DateTime.Parse(hpsmDate) };
        }

        private BooleanType GetHpsmBoolean(bool HPSMBool)
        {
            return new BooleanType { Value = HPSMBool };
        }


    }
}
