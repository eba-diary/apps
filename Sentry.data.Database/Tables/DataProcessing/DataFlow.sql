﻿CREATE TABLE [dbo].[DataFlow]
(
	[Id] INT NOT NULL IDENTITY , 
    [FlowGuid] UNIQUEIDENTIFIER NOT NULL, 
    [Name] VARCHAR(250) NOT NULL, 
    [Create_DTM] DATETIME NOT NULL, 
    [CreatedBy] VARCHAR(10) NOT NULL, 
	[Questionnaire] VARCHAR(MAX) NULL, 
    [FlowStorageCode] VARCHAR(7) NULL,
	[SaidKeyCode] VARCHAR(10) NULL,
    [ObjectStatus] INT NOT NULL DEFAULT 1, 
    [DeleteIssuer] VARCHAR(10) NULL, 
    [DeleteIssueDTM] DATETIME NOT NULL, 
    [NamedEnvironment] VARCHAR(25) NULL , 
    [NamedEnvironmentType] VARCHAR(25) NULL , 
    CONSTRAINT [PK_DataFlow] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The Quartermaster Named Environment associated with this Data Flow.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlow',
    @level2type = N'COLUMN',
    @level2name = N'NamedEnvironment'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The classification (prod or non-prod) of the Quartermaster Named Environment associated with this Data Flow.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlow',
    @level2type = N'COLUMN',
    @level2name = N'NamedEnvironmentType'