CREATE TABLE [HangFire7].[Set] (
    [Key]      NVARCHAR (100) NOT NULL,
    [Score]    FLOAT (53)     NOT NULL,
    [Value]    NVARCHAR (256) NOT NULL,
    [ExpireAt] DATETIME       NULL,
    CONSTRAINT [PK_HangFire_Set] PRIMARY KEY CLUSTERED ([Key] ASC, [Value] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_HangFire_Set_ExpireAt]
    ON [HangFire7].[Set]([ExpireAt] ASC) WHERE ([ExpireAt] IS NOT NULL);


GO
CREATE NONCLUSTERED INDEX [IX_HangFire_Set_Score]
    ON [HangFire7].[Set]([Key] ASC, [Score] ASC);

