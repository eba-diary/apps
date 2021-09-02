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
    [UserDropLocationBucket] VARCHAR(1000) NULL, 
    [UserDropLocationPrefix] VARCHAR(1000) NULL, 
    [NamedEnvironment] VARCHAR(25) NULL, 
    [NamedEnvironmentType] VARCHAR(25) NULL, 
    [IngestionType] INT NULL, 
    [IsDecompressionRequired] BIT NOT NULL, 
    [CompressionType] INT NULL, 
    [IsPreProcessingRequired] BIT NOT NULL, 
    [PreProcessingOption] INT NULL, 
    CONSTRAINT [PK_DataFlow] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'User supplied bucket to override source bucket for drop location',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlow',
    @level2type = N'COLUMN',
    @level2name = 'UserDropLocationBucket'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'User supplied prefix to override source prefix for drop location',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlow',
    @level2type = N'COLUMN',
    @level2name = 'UserDropLocationPrefix'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The Quartermaster Named Environment that this Data Flow is for',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlow',
    @level2type = N'COLUMN',
    @level2name = N'NamedEnvironment'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'The Quartermaster Named Environment Type (prod or nonprod) that this Data Flow is for',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'DataFlow',
    @level2type = N'COLUMN',
    @level2name = N'NamedEnvironmentType'