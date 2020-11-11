/*********************************
  This script will associated a security record for all datasets with a null Security ID
*********************************/

IF OBJECT_ID('tempdb..#DatasetList') IS NOT NULL DROP TABLE #DatasetList

select
ROW_NUMBER() OVER (
	ORDER BY DS.Dataset_ID
) row_num,
DS.Dataset_ID as 'DId'
into #DatasetList
from Dataset DS
where Security_ID IS NULL

IF OBJECT_ID('tempdb..#NewSecurityIds') IS NOT NULL DROP TABLE #NewSecurityIds
Create Table #NewSecurityIds
(
	row_num int,
	SecId VARCHAR(500)
)


/* Select * from #SecurityPrep */

/* Add new Security IDs to Security table based on number of datasets which need to be linked */

DECLARE @SecurityIterationMax INT, @sql VARCHAR(500), 
	@SecurityUpdateIteration INT, @NewIDValue VARCHAR(500), 
	@CurSecurityRow INT, @NewIdsql NVARCHAR(Max), @Securitysql NVARCHAR(Max)

SET @SecurityIterationMax = (select max(row_num) from #DatasetList)
SET @SecurityUpdateIteration = 0

print 'IterationMax: ' + CAST(@SecurityIterationMax as VARCHAR(5))
print 'SecurityUpdateInteration: ' + CAST(@SecurityUpdateIteration as VARCHAR(5))

WHILE @SecurityUpdateIteration <> @SecurityIterationMax
BEGIN

	SET @CurSecurityRow = @SecurityUpdateIteration + 1
	SET @NewIDValue = (SELECT NEWID())
	SET @Securitysql = ('insert into Security select ''' + @NewIDValue + ''', ''Dataset'', GETDATE(), GETDATE(), NULL, NULL, ''072984''')
	SET @NewIdsql = 'insert into #NewSecurityIds select ' + CAST(@CurSecurityRow as varchar(5)) + ', ''' + @NewIDValue + ''''

	print @Securitysql	
	EXECUTE sp_executesql @Securitysql

	PRINT @NewIdsql
	EXECUTE sp_executesql @NewIdsql

	SET @SecurityUpdateIteration += 1

END

/* Update Datasets with new, unassociated, security Ids */

Update Dataset
SET Security_ID = x.SecId
from 
(
	Select DSList.DId, NewIds.SecId
	from #DatasetList DSList
	Join #NewSecurityIds NewIds on
		DSList.row_num = NewIds.row_num
) x
where Dataset_ID = x.DId