import json
import sqlite3
import os

# authors.json contains authors
authors = '../data/authors.json'

# Establish Connection with DB
DB_NAME = 'EBA.db'
auth_id = 1
with sqlite3.connect(DB_NAME) as db:
    cur = db.cursor()
    with open(authors, 'r') as f:
        auth = json.load(f)
        for key, values in auth.items():
            if(isinstance(values, list)):
                for v in values:
                    cur.execute("INSERT INTO author (id, given_name, middle_name, family_name) VALUES(?, ?, ?, ?)", (auth_id, v['given_name'], v['middle_name'], v['family_name']))
                    auth_id += 1
            else:
                raise TypeError
    