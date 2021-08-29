import sqlite3
import csv

# TODO #38 Hard code volume 17 for dev. Generalize for production
persName = '../data/networks/volume_17_people.csv'

def conn_db():
    """
    Establish connection with the EBA database
    :return: conn object
    """
    DB_NAME = "EBA.db"
    con = sqlite3.connect(DB_NAME)
    cur = con.cursor()
    return cur


def get_pers_id(person):
    """
    Check whether person exists in biography table.
    :person: a tei named entity in an entry
    :return: id of person from EBA.db
    """
    person_exists = """
        SELECT person_id FROM biography WHERE persName = :person;
    """
    cur = conn_db()
    cur.execute(person_exists, {"person": person})
    return cur.fetchall()

DB_NAME = 'EBA.db'

with sqlite3.connect(DB_NAME) as db:
    cur = db.cursor()
    with open (persName, 'r') as f:
        pers = csv.reader(f)
        for row in pers:
            pers_id = get_pers_id(row[3])
            if pers_id:
                pers_id = pers_id[0][0]
                cur.execute("INSERT INTO people (journal_id, diary_entry_id, person_id) VALUES(?, ?, ?)", (row[0], row[1], pers_id))
                
                