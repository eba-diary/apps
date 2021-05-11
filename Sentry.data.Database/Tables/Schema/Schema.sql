﻿CREATE TABLE [dbo].[Schema]
(
	[Schema_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SchemaEntity_NME] VARCHAR(64) NOT NULL, 
    [Schema_NME] VARCHAR(100) NOT NULL, 
	[CreatedBy] VARCHAR(50) NOT NULL,
    [Created_DTM] DATETIME NOT NULL, 
    [LastUpdatd_DTM] DATETIME NOT NULL, 
    [FileExtension_Id] INT NULL, 
    [Delimiter] VARCHAR(10) NULL, 
    [HasHeader] BIT NULL, 
    [CreateCurrentView] BIT NULL, 
    [IsInSAS] BIT NULL, 
    [SASLibrary] VARCHAR(50) NULL, 
    [Description] VARCHAR(2000) NULL, 
    [HiveTable] VARCHAR(250) NULL, 
    [HiveDatabase] VARCHAR(250) NULL, 
    [HiveLocation] VARCHAR(250) NULL, 
    [StorageCode] VARCHAR(250) NULL, 
    [DeleteInd] BIT NOT NULL, 
    [DeleteIssuer] VARCHAR(10) NULL, 
    [DeleteIssueDTM] DATETIME NULL, 
    [HiveStatus] VARCHAR(50) NULL, 
    [CLA1396_NewEtlColumns] BIT NOT NULL, 
    [CLA1580_StructureHive ] BIT NOT NULL, 
	[CLA2472_EMRSend] BIT NOT NULL, 
	[CLA1286_KafkaFlag] BIT NOT NULL, 
    [SnowflakeTable] VARCHAR(250) NULL, 
    [SnowflakeDatabase] VARCHAR(250) NULL,
	[SnowflakeSchema] VARCHAR(250) NULL,
    [SnowflakeStatus] VARCHAR(250) NULL, 
    [ObjectStatus] INT NOT NULL DEFAULT 1, 
    [CLA3014_LoadDataToSnowflake] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT [FK_Schema_FileExtension] FOREIGN KEY ([FileExtension_Id]) REFERENCES [FileExtension]([Extension_Id]) 
)
