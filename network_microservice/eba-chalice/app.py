from chalice import Chalice
import boto3
import sqlite3
import json

app = Chalice(app_name='eba-chalice')

# Establish a connection to the database and return cursor
def conn_db():
    """
    Establish connection with the EBA database
    :return: conn object
    """
    DB_NAME = "chalicelib/EBA.db"
    con = sqlite3.connect(DB_NAME)
    cur = con.cursor()
    return cur

@app.route('/', methods=['GET'],cors=True)
def index():
    return {'hello': 'world'}

@app.route('/entries/{entry_date}', methods=['GET'],cors=True)
def entry(entry_date):
    """
    Query EBA database for entry by date
    :return: entry_txt
    """
    sql_entry = """
            SELECT entry_date, entry_txt FROM diary_entry WHERE entry_date = :entry_date;
    """
    cur = conn_db()
    cur.execute(sql_entry, {"entry_date": entry_date})
    results = cur.fetchall()[0]
    json_results = json.dumps({
    'entry_date': results[0],
    'entry_txt': results[1]})

    return json_results

@app.route('/diary_entries', methods=['GET'],cors=True)
def diary_entries():
    """
    Query EBA database for all diary entries
    :return: [entry_sentiment]
    """
    sql_sentiments = """
            SELECT entry_date, entry_txt, entry_tei, entry_sentiment FROM diary_entry
    """
    cur = conn_db()
    cur.execute(sql_sentiments)
    results = cur.fetchall()

    json_results = {}
    for i in range(len(results)):
        record = results[i]
        json_results[record[0]] = {
        "entry_txt" : record[1],
        "entry_tei" : record[2],
        "entry_sentiment" : record[3]}

    return json.dumps(json_results)

@app.route('/bios/{persName}', methods=['GET'],cors=True)
def bio(persName):
    persName = "#" + persName
    """
    Query EBA database for bio by persName
    :return: [entry_sentiment]
    """
    sql_bio = """
            SELECT biography, birth_place, death_place, birth, death FROM biography WHERE persName = :persName;
    """
    cur = conn_db()
    cur.execute(sql_bio, {"persName": persName})
    results = cur.fetchall()[0]
    json_results = json.dumps({
    'biography': results[0],
    'birth_place': results[1],
    'death_place': results[2],
    'birth': str(results[3]),
    'death': str(results[4])})

    return json_results

print(bio("Acland_Lady"))
