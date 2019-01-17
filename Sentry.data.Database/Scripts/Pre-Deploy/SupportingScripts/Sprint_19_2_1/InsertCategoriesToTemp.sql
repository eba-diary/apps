--This will pull the categories off of the dataset table and insert a link into a temp table.
--a post deploy script will then insert the records from the temp table into the real datasetCategory Table once it is created.

create table #TempDatasetCategory ( Dataset_Id int, Category_Id int)

insert into #TempDatasetCategory (Dataset_Id, Category_Id)
Select Dataset_ID, Category_ID from Dataset
