import sqlite3

## Declare Database Variables
DB_NAME = 'EBA.db'
TABLES = {}
INDEX = {}

TABLES['author'] = ("""CREATE TABLE IF NOT EXISTS author (
                            id INTEGER NOT NULL,
                            author_id INTEGER PRIMARY KEY AUTOINCREMENT,
                            given_name TEXT NOT NULL,
                            middle_name TEXT NULL,
                            family_name TEXT NOT NULL
                    );""")

INDEX['author_author_id'] = (
    "CREATE INDEX IF NOT EXISTS author_author_id ON author (author_id);"
)

TABLES['diary'] = ("""CREATE TABLE IF NOT EXISTS diary (
                            id INTEGER NOT NULL,
                            author_id INTEGER NOT NULL,
                            volume_id INTEGER PRIMARY KEY AUTOINCREMENT,
                            image TEXT NULL,
                            url TEXT NULL,
                            iiif_manifest TEXT NULL,
                            date_first INTEGER NULL,
                            date_last INTEGER NULL,
                            CONSTRAINT fk_diary
                                FOREIGN KEY (author_id)
                                REFERENCES author (author_id)
                                ON DELETE CASCADE 
                    );""")

INDEX['diary_author_id'] = (
    "CREATE INDEX IF NOT EXISTS diary_author_id ON diary (author_id);"
)
INDEX['diary_entry_id'] = (
    "CREATE INDEX IF NOT EXISTS diary_entry_id ON diary (volume_id);"
)

TABLES['editor'] = ("""CREATE TABLE IF NOT EXISTS editor (
                            id INTEGER NOT NULL,
                            editor_id INTEGER PRIMARY KEY AUTOINCREMENT,
                            given_name TEXT,
                            family_name TEXT,
                            birth INT,
                            death INT
                        );""")

INDEX['editor_editor_id'] = (
    "CREATE INDEX IF NOT EXISTS editor_id ON editor (editor_id);"
)

TABLES['diary_entry'] = ("""CREATE TABLE IF NOT EXISTS diary_entry (
                                id INTEGER NOT NULL,
                                diary_entry_id INTEGER PRIMARY KEY AUTOINCREMENT,
                                editor_id INTEGER NULL,
                                volume_id INTEGER NOT NULL,
                                entry_date INTEGER NOT NULL,
                                entry_txt TEXT NOT NULL,
                                entry_tei TEXT NOT NULL,
                                entry_setiment INTEGER,
                                CONSTRAINT fk_volume_id
                                    FOREIGN KEY (volume_id)
                                    REFERENCES diary (volume_id)
                                    ON DELETE CASCADE,
                                CONSTRAINT fk_editor_id
                                    FOREIGN KEY (editor_id)
                                    REFERENCES editor (editor_id)
                                    ON DELETE CASCADE
                        );""")

TABLES['sentences'] = ("""CREATE TABLE IF NOT EXISTS sentences (
                            id INTEGER NOT NULL,
                            sent_id INTEGER PRIMARY KEY AUTOINCREMENT,
                            entry_id INTEGER NOT NULL,
                            sent_text TEXT,
                            sent_sentiment INTEGER,
                            CONSTRAINT fk_entry_id
                                FOREIGN KEY (entry_id)
                                REFERENCES diary_entry (entry_id)
                                ON DELETE CASCADE
                        );""")

INDEX['sentences'] = (
    "CREATE INDEX IF NOT EXISTS sentences_ling ON sentences (sent_text);"
)

TABLES['tokens'] = ("""CREATE TABLE IF NOT EXISTS tokens (
                            id INTEGER NOT NULL,
                            lemma_id INTEGER PRIMARY KEY NOT NULL,
                            sent_id INTEGER NOT NULL,
                            token TEXT,
                            lemma TEXT,
                            POS TEXT,
                            wordNet TEXT,
                            CONSTRAINT fk_sent_id
                                FOREIGN KEY (sent_id)
                                REFERENCES sentences (sent_id)
                                ON DELETE CASCADE
                            );""")

INDEX['tokens'] = (
    "CREATE INDEX IF NOT EXISTS tokens_ling ON tokens (token, lemma, POS, wordNet);"
)

TABLES['biography'] = ("""CREATE TABLE IF NOT EXISTS biography (
                            id INTEGER NOT NULL,
                            bio_id INTEGER PRIMARY KEY AUTOINCREMENT,
                            person_id INTEGER NOT NULL,
                            persName TEXT NULL,
                            biography TEXT NULL,
                            image TEXT NULL,
                            occupation TEXT NULL,
                            birth_place TEXT NULL,
                            death_place TEXT NULL,
                            birth INTEGER NULL,
                            death INTEGER NULL
                        );""")

INDEX['biography'] = (
    "CREATE INDEX IF NOT EXISTS biography_text ON biography (biography);"
)

TABLES['people'] = ("""CREATE TABLE IF NOT EXISTS people (
                            id INTEGER PRIMARY KEY NOT NULL,
                            person_id INTEGER NULL,
                            lemma_id INTEGER NOT NULL,
                            social_struct TEXT,
                            CONSTRAINT fk_person_id
                                FOREIGN KEY (person_id)
                                REFERENCES biography (person_id)
                                ON DELETE CASCADE,
                            CONSTRAINT fk_lemma_id
                                FOREIGN KEY (lemma_id)
                                REFERENCES tokens (lemma_id)
                                ON DELETE CASCADE
                        );""")



## Create Database
con = sqlite3.connect(DB_NAME)
cur = con.cursor() #create a connection with the database.

# Create Tables and Indices
for t in TABLES:
    tableSQL = TABLES[t]
    cur.execute(tableSQL)
    con.commit()

for i in INDEX:
    indexSQL = INDEX[i]
    cur.execute(indexSQL)
    con.commit()

con.close()