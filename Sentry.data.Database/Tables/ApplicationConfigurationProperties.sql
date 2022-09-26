CREATE TABLE [dbo].[ApplicationConfigurationProperties]
(
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Application] [varchar](100) NULL,
    [Environment] [varchar](10) NULL,
    [ConfigurationKey] [varchar](100) NULL,
    [ConfigurationValue] [varchar](1000) NULL
);