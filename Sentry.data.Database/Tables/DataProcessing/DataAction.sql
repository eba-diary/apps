﻿CREATE TABLE [dbo].[DataAction]
(
	[Id] INT NOT NULL, 
    [ActionGuid] UNIQUEIDENTIFIER NOT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    [TargetStoragePrefix] VARCHAR(250) NOT NULL, 
    [TargetStorageBucket] VARCHAR(250) NOT NULL, 
	[ActionType] VARCHAR(250) NOT NULL, 
    [TargetStorageSchemaAware] BIT NOT NULL, 
    [Description] VARCHAR(250) NOT NULL, 
    [TriggerPrefix] VARCHAR(250) NULL, 
    CONSTRAINT [PK_DataAction] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
) ON [PRIMARY]
