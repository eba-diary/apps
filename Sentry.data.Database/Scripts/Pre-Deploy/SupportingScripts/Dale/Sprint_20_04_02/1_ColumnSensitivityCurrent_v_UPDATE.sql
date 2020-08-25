ALTER view [dbo].[ColumnSensitivityCurrent_v] as
select 
  bqd.Asset_CDE,
  bqd.Environment_NME,
  bqd.Logical_NME,
  bqd.Alias_NME,
  bs.Content_NME [Server_NME],
  s.Active_FLG,
  s.Prod_TYP,
  bd.Content_NME [Database_NME],
  bo.Content_NME [Object_NME],
  bo.Schema_NME [Schema_NME],
  bo.Base_NME [Base_NME],
  case bo.Content_TYP when 'ST' then 'Table' when 'SV' then 'View' else bo.Content_TYP end [Type_DSC],
  bc.Content_NME [Column_NME],
  bc.Sensitive_FLG [IsSensitive_FLG],
  c.Column_ID,
  c.Column_TYP,
  c.User_TYP,
  c.MaxLength_LEN,
  c.Precision_LEN,
  c.Scale_LEN,
  c.Collation_NME,
  c.XMLSchema_NME,
  c.IsNullable_FLG,
  c.IsAnsiPadded_FLG,
  c.IsRowGuidCol_FLG,
  c.IsIdentity_FLG,
  c.IsComputed_FLG,
  c.IsFilestream_FLG,
  c.IsReplicated_FLG,
  c.IsNonSqlSubscribed_FLG,
  c.IsMergePublished_FLG,
  c.IsDtsReplicated_FLG,
  c.IsXmlDocument_FLG,
  c.IsSparse_FLG,
  c.IsColumnSet_FLG,
  c.IsHidden_FLG,
  c.IsMasked_FLG,
  c.GeneratedAlways_TYP,
  c.Encryption_TYP,
  br.Effective_DTM,
  br.Expiration_DTM,
  bs.Base_ID [BaseServer_ID],
  bd.Base_ID [BaseDatabase_ID],
  bo.Base_ID [BaseTable_ID],
  bc.Base_ID [BaseColumn_ID],
  c.Content_ID,
  bc.OwnerVerify_FLG AS IsOwnerVerified_FLG
from [dbo].[Base] bc 
  inner join [dbo].[Base] bo on bc.Parent_ID=bo.Base_ID
  inner join [dbo].[Base] bd on bo.Parent_ID=bd.Base_ID
  inner join [dbo].[Base] bs on bd.Parent_ID=bs.Base_ID
  inner join [dbo].[Run] r on bs.LastRun_ID=r.Run_ID
  inner join [dbo].[Server] s on r.Server_ID=s.Server_ID
  inner join [dbo].[BaseRevision] br on bc.Base_ID=br.Base_ID 
  inner join [dbo].[BaseColumn] c on br.Content_ID=c.Content_ID
  left outer join [dbo].[BaseQMData_v] bqd on bd.Base_ID=bqd.Base_ID and bqd.Expiration_DTM is null
where bo.Content_TYP in('ST','SV')
  and br.Expiration_DTM is null


