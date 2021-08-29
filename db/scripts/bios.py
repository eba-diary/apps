import gspread
import pandas as pd
import re
from oauth2client.service_account import ServiceAccountCredentials
import sqlite3

scopes = ['https://spreadsheets.google.com/feeds','https://www.googleapis.com/auth/drive']
creds = ServiceAccountCredentials.from_json_keyfile_name('../data/biographies.json', scopes)

client = gspread.authorize(creds)
sheet = client.open("MASTER INDICES 2021")

# Get the persName Sheet
sheet_persName = sheet.get_worksheet(0)
records_data = sheet_persName.get_all_records()

# Convert to DF and process into Database
# persName=@REF TAG TEI, biography=XML, occupation=OCCUPATION, birth_place=BIRTHPLACE, death_place=PLACE OF DEATH, birth=DATE OF BIRTH, death=DATE OF DEATH
bios = pd.DataFrame.from_dict(records_data)
bios.drop(["Complete SK",
            "Display_Name",
            "Variants",
            "AUTHORITY_FILE",
            "RESEARCH",
            "VOLUME(S)",
            "BIO ON EBA WEBSITE (Y/N)",
            "RESEARCH FOLDER IN GOOGLE DRIVE",
            "IMAGE SOURCE",
            "IMAGE",
            "REFERENCE RESOURCES USED (eg books, websites etc)",
            "OCCUPATION",
            "BIOGRAPHICAL NOTES",
            "STATIC IMAGE URL",
            "INTERN INITIALS"], axis=1, inplace=True)

DB_NAME = "EBA.db"
with sqlite3.connect(DB_NAME) as db:
    cur = db.cursor()
    bioIdx = 1
    for row in bios.iterrows():
        persName = re.sub("#", '', row[1][1])
        biography, birth_place, death_place, birth, death = str(row[1][6]), str(row[1][3]), str(row[1][5]), str(row[1][2]), str(row[1][4])
        cur.execute("INSERT INTO biography (id, persName, person_id, biography, birth_place, death_place, birth, death) VALUES (?, ?, ?, ?, ?, ?, ?, ?)", (bioIdx, persName, bioIdx, biography, birth_place, death_place, birth, death))
        bioIdx += 1