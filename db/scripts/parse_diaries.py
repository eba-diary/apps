#! -*- coding: UTF-8 -*-

import csv
from bs4 import BeautifulSoup
import lxml
import re
import argparse
import sys
import os
from afinn import Afinn
from config import volumes

def get_loc(diary):
    """
    Return path to diary volume
    :return: volume formatted according to location as specified in database.ini
    """
    if diary.isdigit():
        return "volume_" + str(diary)
    else:
        num = ''.join([c for c in diary if c.isdigit()])
        return "volume_" + str(num)

def process_diary(args):
    """
    Here we preprocess a journal to a csv output
    """
    diary = args.volume_num
    # Sanitize the diary
    diary = get_loc(diary)
    print(f'Processing {diary}…')

    afinn = Afinn(language='en')
    diary_volumes = volumes()
    diary_path = diary_volumes[diary]
    
    journal_id = 17
    entry_id = 1

    with open(diary_path) as xml:
        soup = BeautifulSoup(xml, 'lxml-xml')
        with open(os.path.join('../data/networks', diary + '_people' + '.csv'), 'w', newline='') as f:
            print('Creating CSV file for networks …')
            ppl = csv.writer(f)
            with open(os.path.join('../data/volumes', diary + '_entry' + '.csv'), 'w', newline='') as e:
                print('Creating CSV file for diary entries …')
                entry = csv.writer(e)
                # create headers for entry csv
                entry.writerow(["journal_id", "entry_id", "date", "entry", "XML", "sentiment"])
                # create headers for people csv
                ppl.writerow(["journal_id", "entry_id", "date", "TEI_name", "emma_name", "relation"])
                
                print('Processing each entry …')
                for i in soup.find_all("div", {"type": "entry"}):
                    txt_string = str(i)
                    xml = "".join(line.strip() for line in txt_string.split("\n"))
                
                    #Extract the Date for the Graph Model
                    match = re.search('EBA-([0-9-–]+)', i.attrs['xml:id'])
                    date = match.group(1) if match else None

                    #Clean the Entry and Prepare for Post-Processing
                    remove_newlines = re.sub("\n+", " ", i.text.strip())
                    plain_text = re.sub(" +", " ", remove_newlines)

                    #Extract all the PersName and Score Entry in which PersName appears
                    people = i.find_all('persName')
                    if not people:
                        afinn_scr = afinn.score(plain_text)
                        entry.writerow([journal_id, entry_id, date, plain_text, xml, afinn_scr])
                        ppl.writerow([journal_id, entry_id, date, "None", "None", "None"])
                        entry_id += 1
                    else:
                        entry.writerow([journal_id, entry_id, date, plain_text, xml, afinn_scr])
                        for p in people:
                            names_unclean = re.sub("\n+", "  ", p.text.strip())
                            emma_name = re.sub(" +", " ", names_unclean)
                            if ' ' in p['ref']:
                                more_than_one = p['ref'].split(' ')
                                for ind in more_than_one:
                                    ind = re.sub("#", '', ind)
                                    ppl.writerow([journal_id, entry_id, date, ind, emma_name, "undef"])
                            else:
                                p['ref'] = re.sub("#", '', p['ref'])
                                ppl.writerow([journal_id, entry_id,date, p['ref'], emma_name, "undef"])
                        entry_id += 1
                print(f'{diary} is processed.')


def main(argv):
    """
    Preprocess the diaries by selected volume. Enter a volume number for
    a valid argument.

    :volume_num: volumes 1–19
    """
    parser = argparse.ArgumentParser()
    parser.add_argument('volume_num', help='Enter a volume number: 1–19')
    args = parser.parse_args()
    process_diary(args)


if __name__ == '__main__':
        main(sys.argv[:1])
