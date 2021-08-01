import json
import sqlite3
from queries import author

# Static Files
diaries = '../data/diaries.json'
DB_NAME = 'EBA.db'

# Specify the Author
author = author("Andrews")
# Get db author_id for foreign key
author_id = author[0][0]
volume_id = 1

with sqlite3.connect(DB_NAME) as db:
    cur = db.cursor()
    with open(diaries, 'r') as f:
        diary_vol = json.load(f)
        collection = diary_vol['bibliography']
        for c in collection:
            for key, value in c.items():
                cur.execute("INSERT INTO diary (id, author_id, volume_id, date_first, date_last) VALUES (?, ?, ?, ?, ?)", (volume_id, author_id, volume_id, value['date_first'], value['date_last']))
                volume_id += 1
                