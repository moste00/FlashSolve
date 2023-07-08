import json
import sys
import os
import re
import math
import numpy as np
import csv

spreads = dict()


file_pattern = r"The total distance for (.*):"
average_pattern = r"The average distance for (.*): (\d+\.\d{6})"

file_name_new = ''
avg = 0.0
# parse each file
def parse_file(file_path):
    with open(file_path, 'r') as file:
        for line in file:
            line = line.strip()
            if re.search(file_pattern, line):
                file_name = re.search(file_pattern, line).group(1)
                file_name_new =  f"{file_name.split('_')[2]}_{file_name.split('_')[1]}"
            if re.search(average_pattern, line):
                avg = float(re.search(average_pattern, line).group(2))
                spreads[file_name_new] = avg
    
directory = sys.argv[1]



parse_file(directory)
print(spreads)

# save the times in a excel file
filename = "our_spread.csv"
# Open the file in write mode and create a CSV writer object
with open(filename, "w", newline="") as csvfile:
    writer = csv.writer(csvfile)
    # Write the headers
    writer.writerow(["file", "avg_spread"])
    # Write the data
    for file_name, avg_spread in spreads.items():
        writer.writerow([file_name, "{:.6f}".format(avg_spread)])