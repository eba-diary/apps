CREATE TABLE [dbo].[SecurityTicket]
(
	[SecurityTicket_ID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Ticket_ID] VARCHAR(64) NOT NULL, 
    [RequestedBy_ID] VARCHAR(32) NOT NULL, 
    [ApprovedBy_ID] VARCHAR(32) NULL, 
    [RejectedBy_ID] VARCHAR(32) NULL, 
    [Requested_DTM] DATETIME NOT NULL, 
    [Approved_DTM] DATETIME NULL, 
    [Rejected_DTM] DATETIME NULL, 
    [TicketStatus_DSC] VARCHAR(32) NOT NULL, 
    [AdGroup_NME] VARCHAR(64) NULL, 
    [IsRemovingPermission_IND] BIT NOT NULL, 
    [IsAddingPermission_IND] BIT NOT NULL, 
    [Security_ID] UNIQUEIDENTIFIER NOT NULL,
	[Rejected_DSC] VARCHAR(256) NULL, 
    [IsSecuredByUser] BIT NOT NULL DEFAULT 0, 
    [GrantPermissionToUser_ID] VARCHAR(8) NULL, 
    [AwsArn] VARCHAR(2048) NULL, 
    [IsSystemGenerated] BIT NULL DEFAULT 0, 
    CONSTRAINT [FK_SecurityTicket_Security] FOREIGN KEY ([Security_ID]) REFERENCES [Security]([Security_ID])
)
