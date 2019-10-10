CREATE TABLE [dbo].[DataActionTypes]
(
	[Id] INT NOT NULL , 
    [Name] VARCHAR(50) NULL, 
    [CreatedDTM] DATETIME NULL DEFAULT (sysdatetime()), 
    [CreatedBy] NVARCHAR(125) NULL DEFAULT (suser_sname()), 
    CONSTRAINT [PK_DataActionTypes] PRIMARY KEY CLUSTERED([Id] ASC)
)
