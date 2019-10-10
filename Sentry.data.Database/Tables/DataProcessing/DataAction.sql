﻿CREATE TABLE [dbo].[DataAction]
(
	[Id] INT NOT NULL , 
    [ActionGuid] UNIQUEIDENTIFIER NOT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    [TargetStoragePrefix] VARCHAR(250) NOT NULL, 
    [TargetStorageBucket] NCHAR(10) NOT NULL, 
	[ActionType] VARCHAR(250) NOT NULL, 
    CONSTRAINT [PK_DataAction] PRIMARY KEY CLUSTERED
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
