import sqlite3
import csv
import os
import nltk

DB_NAME = 'EBA.db'

# Process all CSV files in data
data = os.path.join("..", "data", "volumes")
contents = os.listdir(data)

with sqlite3.connect(DB_NAME) as db:
    cur = db.cursor()
    for f in contents:
        if f.endswith(".csv"):
            with open(os.path.join(data, f)) as csvfile:
                csv = csv.reader(csvfile)
                next(csv)
                for row in csv:
                    volume_id, entry_id, entry_date, plain_text, xml, sentiment = row[0], row[1], row[2], row[3], row[4], row[5]
                    cur.execute("INSERT INTO diary_entry (diary_entry_id, volume_id, entry_date, entry_txt, entry_tei, entry_afinn) VALUES(?, ?, ?, ?, ?, ?)", (entry_id, volume_id, entry_date, plain_text, xml, sentiment))
        else:
            next
