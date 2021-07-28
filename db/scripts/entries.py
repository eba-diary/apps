import sqlite3
import csv
import os
import nltk

# Process all CSV files in data
data = os.path.join("..", "data")
contents = os.listdir(data)

for f in contents:
    if f.endswith(".csv"):
        with open(os.path.join(data, f)) as csvfile:
            csv = csv.reader(csvfile)
            for row in csv:
                volume_id   = row[0]
                entry_id    = row[1]
                entry_date  = row[2]
                print(volume_id, entry_id, entry_date)
    else:
        next
