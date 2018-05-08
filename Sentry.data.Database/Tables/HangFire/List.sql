CREATE TABLE [HangFire].[List] (
    [Id]       INT            IDENTITY (1, 1) NOT NULL,
    [Key]      NVARCHAR (100) NOT NULL,
    [Value]    NVARCHAR (MAX) NULL,
    [ExpireAt] DATETIME       NULL,
    CONSTRAINT [PK_HangFire_List] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_HangFire_List_Key]
    ON [HangFire].[List]([Key] ASC)
    INCLUDE([ExpireAt], [Value]);


GO
CREATE NONCLUSTERED INDEX [IX_HangFire_List_ExpireAt]
    ON [HangFire].[List]([ExpireAt] ASC)
    INCLUDE([Id]);

