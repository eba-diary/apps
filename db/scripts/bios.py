import gspread
import pandas as pd
from oauth2client.service_account import ServiceAccountCredentials

scopes = ['https://spreadsheets.google.com/feeds','https://www.googleapis.com/auth/drive']
creds = ServiceAccountCredentials.from_json_keyfile_name('../data/biographies.json', scopes)

client = gspread.authorize(creds)
sheet = client.open("MASTER INDICES 2021")

# Get the persName Sheet
sheet_persName = sheet.get_worksheet(0)
records_data = sheet_persName.get_all_records()

# Convert to DF and process into Database
bios_df = pd.DataFrame.from_dict(records_data)