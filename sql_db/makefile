default: remove_db make_db

remove_db:
	@ echo 'Moving EBA.db to bin ...'
	@ rm -rf EBA.db
	@ echo 'Database moved to bin'

make_db:
	@ echo 'Recreating EBA.db from emma_sqlite.py ...'
	@ python3 emma_sqlite.py
	@ echo 'EBA.db is recreated'

# Populate database
# populate_db:
