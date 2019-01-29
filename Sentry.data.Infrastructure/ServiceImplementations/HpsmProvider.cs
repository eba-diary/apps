using System;
using System.Collections.Generic;
using System.Net;
using Sentry.data.Core;
using Sentry.data.Infrastructure.HpsmChangeManagement;

namespace Sentry.data.Infrastructure
{
    public class HpsmProvider : IHpsmProvider
    {


        public HpsmProvider(){}

        private ChangeManagement _service;
        public ChangeManagement Service
        {
            get
            {
                if (_service is null)
                {
                    NetworkCredential nc = new NetworkCredential()
                    {
                        UserName = Configuration.Config.GetHostSetting("HpsmServiceId"),
                        Password = Configuration.Config.GetHostSetting("HpsmServicePassword"),
                        Domain = "SHOESD01" //is this needed?
                    };

                    _service = new ChangeManagement
                    {
                        Credentials = nc,
                        Url = Configuration.Config.GetHostSetting("HpsmServiceUrl"),
                        ConnectionGroupName = GlobalConstants.System.ABBREVIATED_NAME,
                    };
                }
                return _service;
            }
        }

        /// <summary>
        /// Creates a new HPSM Change ticket.
        /// </summary>
        public string CreateHpsmTicket(RequestAccess model)
        {
            CreateChangeRequest request = new CreateChangeRequest()
            {
                model = new ChangeModelType()
                {
                    
                    instance = new ChangeInstanceType()
                    {
                        header = new ChangeInstanceTypeHeader()
                        {
                            AssignmentGroup = GetHpsmString("DSC"),
                            AssignedTo = GetHpsmString("DSC_User"),
                            Category = GetHpsmString("Standard Change"),
                            RequestedStart = GetHpsmDate(model.RequestedDate.ToString()),
                            RequestedEnd = GetHpsmDate(model.RequestedDate.ToString()),
                            InitiatedBy = GetHpsmString(model.RequestorsId.ToString()),
                            ApprovalStatus = GetHpsmString("pending"),
                            Title = GetHpsmString("Access Request for AD Group " + model.AdGroupName)
                        },
                        BackoutTime = GetHpsmString("No"),
                        NoCI = GetHpsmBoolean(true),
                        CriticalSys = GetHpsmString("No"),
                        TestingCompleted = GetHpsmString("Testing is not required for this change"),
                        ImplementationStart = GetHpsmDate(model.RequestedDate.ToString()),
                        ImplementationEnd = GetHpsmDate(model.RequestedDate.ToString()),
                        Urgency = GetHpsmString("Routine"),
                        ConfigState = GetHpsmString("Up"),
                        ProdWindow = GetHpsmString("Yes"),
                        ImpactUrgency = GetHpsmString("Routine"),
                        ReleaseType = GetHpsmString("Server Builds/Build_VM"),
                        Preapproved = GetHpsmBoolean(false),
                        ApprovalOperator = new ChangeInstanceTypeApprovalOperator()
                        {
                            ApprovalOperator = GetHpsmStringArr(model.PrimaryApproverId.ToString())
                        },
                        ApprovalComments = new ChangeInstanceTypeApprovalComments()
                        {
                            ApprovalComments = GetHpsmStringArr(model.BusinessReason)
                        },
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
                                Description = GetHpsmStringArr(model.BusinessReason)
                            }
                        }
                    }
                },
                ignoreEmptyElements = true
            };

            if (!model.IsProd)
            {  //just approve it everytime for non-prod....for now.
                request.model.instance.Preapproved = GetHpsmBoolean(true);
                request.model.instance.ApprovalOperator = null;
                request.model.instance.ApprovalComments = null;
            }


            CreateChangeResponse response = Service.CreateChange(request);


            if (response.status == StatusType.SUCCESS)
            {
                return response.model.keys.ChangeID.ToString();
            }
            return string.Empty;
        }



        public HpsmTicket RetrieveTicket(string hpsmChangeId)
        {

            RetrieveChangeRequest request = new RetrieveChangeRequest()
            {
                model = new ChangeModelType()
                {
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


        public bool CloseHpsmTicket(string hpsmChangeId, bool wasTicketDenied = false)
        {

            CloseChangeRequest request = new CloseChangeRequest()
            {
                model = new ChangeModelType()
                {
                    instance = new ChangeInstanceType()
                    {
                        ClosureComments = new ChangeInstanceTypeClosureComments()
                        {
                            ClosureComments = wasTicketDenied ? GetHpsmStringArr("Permissions Denied") : GetHpsmStringArr("Permissions Granted"),
                            //TODO: Check that the denied type is okay.
                            type = wasTicketDenied ? "Denied" : "Successful"
                        },
                    },
                    keys = new ChangeKeysType()
                    {
                        ChangeID = GetHpsmString(hpsmChangeId)
                    }
                },
                ignoreEmptyElements = true
            };
            
            CloseChangeResponse response = Service.CloseChange(request);

            if (response.status == StatusType.SUCCESS)
            {
                return true;
            }
            return false;
        }



        private HpsmTicket MapToTicket(ChangeInstanceType ticket)
        {
            return new HpsmTicket()
            {
                //how do i get the approval date?
                ApprovedById = ticket.ApprovalOperator.ApprovalOperator.GetValue(0).ToString(),
                RejectedById = ticket.ApprovalOperator.ApprovalOperator.GetValue(0).ToString(),
                TicketStatus = ticket.header.ApprovalStatus.Value.ToUpper()
            };
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
                StringType hstr = new StringType();
                hstr.Value = str;
                hstrl.Add(hstr);
            }

            return hstrl.ToArray();
        }

        private DateTimeType GetHpsmDate(string hpsmDate)
        {
            return new DateTimeType{Value = DateTime.Parse(hpsmDate)};
        }

        private BooleanType GetHpsmBoolean(bool HPSMBool)
        {
            return new BooleanType{Value = HPSMBool};
        }
        
    }
}
