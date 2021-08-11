default: remove_db make_db populate_db get_biographies

remove_db:
	@ echo 'Moving EBA.db to bin ...'
	@ rm -rf EBA.db
	@ echo 'Database moved to bin'

make_db:
	@ echo 'Recreating EBA.db from emma_sqlite.py ...'
	@ python3 emma_sqlite.py
	@ echo 'EBA.db is recreated'

populate_db:
	@ echo 'Processing authors.json ...'
	@ python3 authors.py
	@ echo 'Authors were added to EBA.db'
	@ echo 'Processing diaries.json ...'
	@ python3 diary.py
	@ echo 'Diaries were added to EBA.db'
	@ echo 'Processing entries ...'
	@ python3 entries.py
	@ echo 'Entries were added to EBA.db'	

get_biographies:
	@ echo 'Fetching biographies from gsheets ...'
