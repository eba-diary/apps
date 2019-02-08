CREATE TABLE [dbo].[SecurityPermission]
(
	[SecurityPermission_ID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [IsEnabled_IND] BIT NOT NULL, 
    [Added_DTM] DATETIME NOT NULL, 
    [Enabled_DTM] DATETIME NULL, 
    [Removed_DTM] DATETIME NULL, 
    [AddedFromTicket_ID] UNIQUEIDENTIFIER NOT NULL, 
    [RemovedFromTicket_ID] UNIQUEIDENTIFIER NULL,
	[Permission_ID] INT NOT NULL, 
    CONSTRAINT [FK_AddedSecurityPermission_SecurityTicket] FOREIGN KEY ([AddedFromTicket_ID]) REFERENCES [SecurityTicket]([SecurityTicket_ID]),
	CONSTRAINT [FK_RemovedSecurityPermission_SecurityTicket] FOREIGN KEY ([RemovedFromTicket_ID]) REFERENCES [SecurityTicket]([SecurityTicket_ID]),
	CONSTRAINT [FK_SecurityPermission_Permission] FOREIGN KEY ([Permission_ID]) REFERENCES [Permission] ([Permission_ID])
)
