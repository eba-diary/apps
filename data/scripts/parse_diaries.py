#! -*- coding: UTF-8 -*-

import csv
from bs4 import BeautifulSoup
import lxml
import re
from afinn import Afinn
import argparse
import sys


def process_diary(args):
    """
    Here we preprocess a journal to a csv output 
    """
    pass


def main(argv):
    """
    Preprocess the diaries by selected volume
    :volume_num: volumes 1–19
    """
    parser = argparse.ArgumentParser()

    parser.add_argument('volume_num', help='Enter a volume number: 1–19')
    args = parser.parse_args()

    process_diary(args)


if __name__ == '__main__':
        main(sys.argv[:1])
