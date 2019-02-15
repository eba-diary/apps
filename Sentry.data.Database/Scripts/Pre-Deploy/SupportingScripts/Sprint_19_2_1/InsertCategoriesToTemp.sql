--This will pull the categories off of the dataset table and insert a link into a temp table.
--a post deploy script will then insert the records from the temp table into the real datasetCategory Table once it is created.

CREATE table #TempDatasetCategory ( Dataset_Id int, Category_Id int)

INSERT into #TempDatasetCategory (Dataset_Id, Category_Id)
Select Dataset_ID, Category_ID from Dataset


/*
	Creating mapping from old Ids to new Static data Ids
*/
IF OBJECT_ID('tempdb..#OldCatNewCat') IS NOT NULL DROP TABLE #OldCatNewCat
Create table #OldCatNewCat(
	CatName varchar(100),
	Old_Id int,
	New_Id int
)

insert into #OldCatNewCat select 'Claim',				(Select Id from Category where Name = 'Claim'),				1
insert into #OldCatNewCat select 'Industry',			(Select Id from Category where Name = 'Industry'),			2
insert into #OldCatNewCat select 'Government',			(Select Id from Category where Name = 'Government'),		3
insert into #OldCatNewCat select 'Geographic',			(Select Id from Category where Name = 'Geographic'),		4
insert into #OldCatNewCat select 'Weather',				(Select Id from Category where Name = 'Weather'),			5
insert into #OldCatNewCat select 'Sentry',				(Select Id from Category where Name = 'Sentry'),			6
insert into #OldCatNewCat select 'Commercial Lines',	(Select Id from Category where Name = 'Commercial Lines'),	7
insert into #OldCatNewCat select 'Personal Lines',		(Select Id from Category where Name = 'Personal Lines'),	8
insert into #OldCatNewCat select 'Claims',				(Select Id from Category where Name = 'Claims'),			9
insert into #OldCatNewCat select 'Corporate',			(Select Id from Category where Name = 'Corporate'),			10
insert into #OldCatNewCat select 'IT',					(Select Id from Category where Name = 'IT'),				11
insert into #OldCatNewCat select '401k',				(Select Id from Category where Name = '401k'),				12

DECLARE @cnt INT = 1;
DECLARE @record nvarchar(max);

WHILE @cnt <= (Select count(*) from #OldCatNewCat)
BEGIN
	SET @record = (Select 'CatName:'+CatName+', Old_ID:'+Cast(Old_Id as nvarchar(max))+', New_ID:'+Cast(New_ID as nvarchar(max)) from #OldCatNewCat where New_Id = @cnt)
	print @record
	SET @cnt = @cnt + 1;
END;


/*
	Updating category ids in temp table to match static script Ids
*/
update #TempDatasetCategory
set Category_Id = x.New_Id
from (select New_Id, Old_Id from #OldCatNewCat) x
where Category_Id = x.Old_ID
