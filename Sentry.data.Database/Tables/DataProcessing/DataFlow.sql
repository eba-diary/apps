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
    [IngestionType] INT NULL, 
    [IsDecompressionRequired] BIT NOT NULL, 
    [CompressionType] INT NULL, 
    CONSTRAINT [PK_DataFlow] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
