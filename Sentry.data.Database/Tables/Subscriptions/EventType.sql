CREATE TABLE [dbo].[EventType]
(
	[Type_ID] INT  NOT NULL PRIMARY KEY,
	[Description] [varchar](1024) NOT NULL,
	[Severity] INT NOT NULL, 
    [Display_IND] BIT NOT NULL,
	[Group_CDE] [varchar](60) NULL
)
