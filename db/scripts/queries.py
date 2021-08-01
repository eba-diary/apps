import sqlite3

# Establish a connection to the database and return cursor
def conn_db():
    """
    Establish connection with the EBA database
    :return: conn object
    """
    DB_NAME = "EBA.db"
    con = sqlite3.connect(DB_NAME)
    cur = con.cursor()
    return cur

# Search the database for author and return author_id, family_name
def author(family_name):
    """
    Query EBA database for specific author
    :return: author_id, family name
    """
    sql_author = """
            SELECT author_id, family_name FROM author WHERE family_name = :family_name;
    """
    cur = conn_db()
    cur.execute(sql_author, {"family_name": family_name})
    return cur.fetchall()
    

