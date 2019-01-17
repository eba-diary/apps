--This script required a pre-deploy script. Insert all the categories ont he dataset into the datasetCategory table. 
--this table will now act like a linking table.

--This is post deploy because we needed to wait fot the new DataCategory table to be created.

INSERT INTO DatasetCategory (Dataset_Id, Category_Id)
SELECT Dataset_ID, Category_ID FROM #TempDatasetCategory

If(OBJECT_ID('tempdb..#TempDatasetCategory') Is Not Null)
Begin
    Drop Table #TempDatasetCategory
End