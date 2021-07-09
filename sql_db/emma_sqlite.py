import sqlite3

## Declare Database Variables
# We can call the database EBA for Emma B. Andrews. We might want to consider another name if we add more journals written by authors other than Emma. For now, we can use EBA.
DB_NAME = 'EBA.db'
TABLES = {} #create a Python dictionary for the tables
INDEX = {} #create a Python dictionary for the indices

## TABLES ##

### author - Our Author Table will record the author or believed author of a journal entry. We will want to use author_id as a Foreign Key in other tables.

TABLES['author'] = ("""CREATE TABLE IF NOT EXISTS author (
                                    id INT PRIMARY KEY, 
                                    author_id int NOT NULL,
                                    given_name text NOT NULL,
                                    middle_name text NULL,
                                    family_name text NOT NULL
                                    );""")

INDEX['author_author_id'] = (
    "CREATE INDEX IF NOT EXISTS author_author_id ON author (author_id)"
)


# I commented this out for now, but will return to it to complete it later.

# TABLES['journal_volume'] = ("""CREATE TABLE IF NOT EXISTS journal (
#                                     id INT PRIMARY KEY,
#                                     author_id ,
#                                     volume_id,
#                                     volume_
#                             );""")


## Create Database
con = sqlite3.connect(DB_NAME)
cur = con.cursor() #create a connection with the database.

# Now we want to iterate over the dictionary to create each table for the database
for t in TABLES:
    tableSQL = TABLES[t]
    cur.execute(tableSQL)
    for i in INDEX:
        indexSQL = INDEX[i]
        cur.execute(indexSQL)

    # We use con.commit to send to have SQL process our statements
    con.commit()
    con.close() # We close the connection with the database