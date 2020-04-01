CREATE TABLE [dbo].[BusinessArea]
(
	[BusinessArea_ID] INT NOT NULL PRIMARY KEY, 
    [Name_DSC] VARCHAR(255) NOT NULL, 
    [AbbreviatedName_DSC] VARCHAR(10) NULL,
	[PrimaryOwner_ID] [varchar](8) NOT NULL,
	[PrimaryContact_ID] VARCHAR(8) NOT NULL DEFAULT '000000', 
    [IsSecured_IND] BIT NOT NULL DEFAULT 0 , 
    [Security_ID] UNIQUEIDENTIFIER NULL,
	
	CONSTRAINT [FK_BusinessArea_Security] FOREIGN KEY ([Security_ID]) REFERENCES [Security]([Security_ID]) 
)

