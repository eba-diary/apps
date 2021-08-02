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

@app.route('/authors/{family_name}', methods=['GET'],cors=True)
def author(family_name):
    """
    Query EBA database for specific author
    :return: author_id, family name
    """
    sql_author = """
            SELECT author_id, given_name, middle_name, family_name FROM author WHERE family_name = :family_name;
    """
    cur = conn_db()
    cur.execute(sql_author, {"family_name": family_name})
    results = cur.fetchall()[0]
    json_results = json.dumps({
    'author_id': results[0],
    'given_name': results[1],
    'middle_name': results[2],
    'family_name': results[3]})

    return json_results

print(author("Andrews"))
