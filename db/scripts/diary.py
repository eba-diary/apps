import json
import sqlite3
from queries import author

diaries = '../data/diaries.json'
DB_NAME = 'EBA.db'

author = author("Andrews")
author_id = author[0][0]

with sqlite3.connect(DB_NAME) as db:
    cur = db.cursor()
    with open(diaries, 'r') as f:
        diary_vol = json.load(f)
        collection = diary_vol['bibliography']
        # for c in collection:
        #     for key, value in c.items():
                # print(key)
                