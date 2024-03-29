import json
import sys
import os
import re
import math
import numpy as np

# parse each file
def parse_file(file_path):
    variables = []
    with open(file_path, 'r') as file:
        for line in file:
            line = line.strip()
            values = line.split(',')
            # remove the last element in values
            if values[-1] == '':
                values.pop()
            # print(values)
            variables.append([int(value.split('=')[1]) for value in values])
    return variables

def euclide_distance(point1, point2):
    sum_of_squares = 0
    for i in range(len(point1)):
        sum_of_squares += (point1[i] - point2[i])**2
    return math.sqrt(sum_of_squares)

def cal_spread(method_data, file_name):
    num_points = len(method_data)

    dist = 0.0
    for i in range(num_points):
        for j in range(i+1, num_points):
            dist += euclide_distance(method_data[i], method_data[j])

    with open(f'out/spread_benchmark.txt', 'a') as file:
        file.write("\n" + "-"*20 + "spread analysis" + "-"*20 + "\n")
        file.write(f'The total distance for {file_name}: {dist}\n')
        file.write(f'The average distance for {file_name}: {dist/(num_points*(num_points-1)/2)}\n')


#------------------------------------------------------------------------------------------------------#
directory = sys.argv[1]
print(f'directory: {directory}')
if os.path.exists(f'out/spread_benchmark.txt'):
        os.remove(f'out/spread_benchmark.txt')

# loop over all the files in the directory
for file_name in os.listdir(directory):
    if file_name.endswith('.txt'):
        transcript_file_path = os.path.join(directory, file_name)
        # print(f'transcript_file_path: {file_name}')
        variables_values = parse_file(transcript_file_path)
        file_name = file_name.split('.')[0]
        print(variables_values)
        cal_spread(variables_values, file_name)

