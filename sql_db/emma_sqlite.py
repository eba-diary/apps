import sqlite3

## Declare Database Variables
# We can call the database EBA for Emma B. Andrews. We might want to consider another name if we add more journals written by authors other than Emma. For now, we can use EBA.
DB_NAME = 'EBA.db'
TABLES = {} #create a Python dictionary for the tables
INDEX = {} #create a Python dictionary for the indices

## TABLES ##

### author - Our Author Table will record the author or believed author of a journal entry. We will use author_id as a Foreign Key from the diary.author_id.

TABLES['author'] = ("""CREATE TABLE IF NOT EXISTS author (
                                    id INT PRIMARY KEY NOT NULL, 
                                    author_id INT NOT NULL,
                                    given_name TEXT NOT NULL,
                                    middle_name TEXT NULL,
                                    family_name TEXT NOT NULL
                                    );""")

INDEX['author_author_id'] = (
    "CREATE INDEX IF NOT EXISTS author_author_id ON author (author_id);"
)

# diary - an author writes a diary. We will use a diary table to hold meta-data about the diary. Diaries have entries so the foreign key entry.entry_id references entries.

TABLES['diary'] = ("""CREATE TABLE IF NOT EXISTS diary (
                                    id INT PRIMARY KEY NOT NULL,
                                    author_id INT NOT NULL,
                                    image TEXT NULL,
                                    url TEXT NULL,
                                    iiif_manifest TEXT NULL,
                                    entry_id INT NOT NULL,
                                    date_first INT NULL,
                                    date_last INT NULL,
                                    CONSTRAINT fk_diary
                                        FOREIGN KEY (author_id)
                                        REFERENCES author (author_id)
                                        ON DELETE CASCADE 
                            );""")

INDEX['diary_author_id'] = (
    "CREATE INDEX IF NOT EXISTS diary_author_id ON diary (author_id);"
)
INDEX['diary_entry_id'] = (
    "CREATE INDEX IF NOT EXISTS diary_entry_id ON diary (entry_id);"
)

TABLES['diary_entry'] = ("""CREATE TABLE IF NOT EXISTS diary_entry (
                                    id INT PRIMARY KEY NOT NULL,
                                    entry_id INT NOT NULL,
                                    entry_txt TEXT NOT NULL,
                                    entry_tei TEXT NOT NULL,
                                    entry_setiment INT,
                                    CONSTRAINT fk_diary_entry
                                        FOREIGN KEY (entry_id)
                                        REFERENCES diary (entry_id)
                                        ON DELETE CASCADE
                            );""")



## Create Database
con = sqlite3.connect(DB_NAME)
cur = con.cursor() #create a connection with the database.

# Now we want to iterate over the dictionary to create each table for the database
for t in TABLES:
    tableSQL = TABLES[t]
    cur.execute(tableSQL)
    con.commit()

for i in INDEX:
    indexSQL = INDEX[i]
    cur.execute(indexSQL)
    con.commit()

con.close()