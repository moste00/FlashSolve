import json
import sys
import os
import re
import math
import numpy as np
import csv

times = dict()

# Match the desired pattern
file_pattern = r'_([^/]+)\.txt' 
total_time_pattern = r'tot_time= ([0-9]+\.[0-9]{6})'

file_name_new = ''
tot_time = 0.0
# parse each file
def parse_file(file_path):
    with open(file_path, 'r') as file:
        for line in file:
            line = line.strip()
            if re.search(file_pattern, line):
                file_name = re.search(file_pattern, line).group(1)
                file_name_new =  f"{file_name.split('_')[1]}_{file_name.split('_')[0]}"
            if re.search(total_time_pattern, line):
                tot_time = float(re.search(total_time_pattern, line).group(1))
                times[file_name_new] = tot_time
    
directory = sys.argv[1]


for file_name in os.listdir(directory):
    if file_name.find('max smt time till1000') != -1:
        time_file_path = os.path.join(directory, file_name)
        # print(f'transcript_file_path: {time_file_path}')
        parse_file(time_file_path)

        # save the times in a excel file
        filename = "our_time.csv"
        # Open the file in write mode and create a CSV writer object
        with open(filename, "w", newline="") as csvfile:
            writer = csv.writer(csvfile)
            # Write the headers
            writer.writerow(["file", "total time"])
            # Write the data
            for file_name, total_time in times.items():
                writer.writerow([file_name, "{:.6f}".format(total_time)])


