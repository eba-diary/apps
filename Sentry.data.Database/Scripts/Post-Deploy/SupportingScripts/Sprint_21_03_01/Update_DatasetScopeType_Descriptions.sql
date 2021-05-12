/******************************************
	Update DatasetScopeType descriptions 
	for Point-in-Time and Appending types
*******************************************/

Update 
	DatasetScopeTypes
Set 
	Type_DSC = 'A copy of data at a given point in time.  Data consumption will focus on the latest file that has been uploaded.  Data may be repeated across files.'
where
	Name = 'Point-in-Time'


Update 
	DatasetScopeTypes
Set 
	Type_DSC = 'New data arrives in each file.  The new file can be appended to previous files for the full data picture.  Data will not be repeated across files.'
where
	Name = 'Appending'