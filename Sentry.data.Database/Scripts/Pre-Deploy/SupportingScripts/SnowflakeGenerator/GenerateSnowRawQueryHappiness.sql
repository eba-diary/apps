/*************************************************************************************************************************************************************
Following script will generate Snowflake DDL for EXTERNAL tables over RAWQUERY

EXTERNAL TABLE RULES
--objectstatus = 1 --1(active)
--fileExtension_id 
-- evaluate FileExentension_ID	fixed width/delimited/xml/json all go to JSON file format
--make sure you create append snowflakechema and Database, do a case statement on which snowflake database if DATA_TEST then DATA_RAWQUERY_TEST
--filter out any file extensions of type 'ANY' AND 'XLSX'
--if no fields exist under a schema, then no schema revsion will exist (then just skip it because we don't care about it)
--send IM to darko of what is going to json from above list

--SNOWFLAKE SCHEMA SHOULD LOOK LIKE THIS:
create or replace external table FNOL_CLAUTO(
	DATE_PARTITION DATE AS (TO_DATE((SPLIT_PART(METADATA$FILENAME, '/', 5)) || '/' || (SPLIT_PART(METADATA$FILENAME, '/', 6)) || '/' || (SPLIT_PART(METADATA$FILENAME, '/', 4)), 'mm/dd/yyyy')))
partition by (DATE_PARTITION)
location=@SENTRY_DATASET/2911319/
auto_refresh=false
pattern='.*[.]json'
file_format=(TYPE=JSON NULL_IF=() COMPRESSION=auto)
;

select * from Dataset where Dataset_ID = 1
select * from DatasetFileConfigs where Dataset_ID = 1
select * from DatasetFile where Dataset_ID = 1
select * from SchemaRevision where ParentSchema_Id = 1


select * from [Schema] where Schema_Id = 1
select * from FileExtension

*************************************************************************************************************************************************************/

--DIRECTIONS! Execute this and then select the snowflake DDL column and paste into snowflake and test, may need to make tweeks when environment is up and running.
SELECT	--D.SaidKeyCode,D.NamedEnvironment,SCHEMA1.SnowflakeDatabase,SCHEMA1.SnowflakeSchema,SCHEMA1.SnowflakeTable, SCHEMA1.StorageCode,FILE_EXTENSION.Extension_NME,
		--REPLACE(SCHEMA1.SnowflakeTable,'.','_'),
		'create or replace external table ' + CASE WHEN SCHEMA1.SnowflakeDatabase LIKE '%PROD%'  THEN 'DATA_RAWQUERY_PROD'
												   WHEN SCHEMA1.SnowflakeDatabase LIKE '%QUAL%'  THEN 'DATA_RAWQUERY_QUAL'  
												   WHEN SCHEMA1.SnowflakeDatabase LIKE '%TEST%'  THEN 'DATA_RAWQUERY_TEST' 
												   ELSE 'DATA_RAWQUERY_DEV'
												   END  + '.' + SCHEMA1.SnowflakeSchema + '.' + REPLACE(SCHEMA1.SnowflakeTable,'.','_')
		+'(
				DATE_PARTITION DATE AS (TO_DATE((SPLIT_PART(METADATA$FILENAME, ''/'', 6)) || ''/'' || (SPLIT_PART(METADATA$FILENAME, ''/'', 7)) || ''/'' || (SPLIT_PART(METADATA$FILENAME, ''/'', 5)), ''mm/dd/yyyy'')))
				partition by (DATE_PARTITION)
				location=@S3_STAGE.DLST_RAWQUERY/' + D.SaidKeyCode + '/' + D.NamedEnvironment + '/' + SCHEMA1.StorageCode  +'/' + '
				
				
				
				
				
				
				auto_refresh=false
				pattern=''.*[.]'	+ CASE WHEN FILE_EXTENSION.Extension_NME IN ('JSON','FIXEDWIDTH','DELIMITED','XML') THEN 'json' ELSE FILE_EXTENSION.Extension_NME END +  '''
				file_format=(TYPE='	+ CASE WHEN FILE_EXTENSION.Extension_NME IN ('JSON','FIXEDWIDTH','DELIMITED','XML') THEN 'json ' ELSE FILE_EXTENSION.Extension_NME END + ' NULL_IF=() COMPRESSION=auto
			);
		  '

FROM [Schema] SCHEMA1

JOIN FileExtension FILE_EXTENSION
	ON SCHEMA1.FileExtension_Id = FILE_EXTENSION.Extension_Id

JOIN
(
	--JOIN TO SchemaRevision because we only want Schema's with atleast one SchemaRevision
	--since many revisions exist, get a distinct list
	SELECT ParentSchema_Id					
	FROM SchemaRevision 
	GROUP BY ParentSchema_Id

) SCHEMA_REVISION_CLEAN
	ON SCHEMA_REVISION_CLEAN.ParentSchema_Id = SCHEMA1.Schema_Id

JOIN DatasetFileConfigs DFC
	ON DFC.[Schema_Id] = SCHEMA1.[Schema_Id]

JOIN Dataset D
	ON DFC.Dataset_ID = D.Dataset_ID


WHERE	FILE_EXTENSION.Extension_NME NOT IN ('ANY','XLSX','TXT')
		AND SCHEMA1.ObjectStatus = 1  
		AND SCHEMA1.SnowflakeDatabase IS NOT NULL
		AND D.ObjectStatus = 1
		AND SCHEMA1.SnowflakeSchema <> 'HR'
		--AND SCHEMA1.[Schema_Id] IN (1245,1394)


ORDER BY D.Dataset_ID
		