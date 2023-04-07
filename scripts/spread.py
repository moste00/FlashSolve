import json
import sys
import re
import math

# read the json files 
method_name_str = re.sub('.json', '',sys.argv[1])
with open(sys.argv[1], "r") as method_name:
    method_data = json.load(method_name)
    print(f'# {method_name_str} solutions: {len(method_data["values"])}')

def euclide_distance(point1, point2):
    sum_of_squares = 0
    for i in range(len(point1)-1):
        sum_of_squares += (point1[i] - point2[i])**2
    return math.sqrt(sum_of_squares)

num_points = len(method_data['values'])
dist = 0.0
for i in range(num_points):
    for j in range(i+1, num_points):
        dist += euclide_distance(method_data['values'][i], method_data['values'][j])

print(f'The total distance between points is: {dist}')
print(f'The average distance between points is: {dist/(num_points*(num_points-1)/2)}')

method_name.close()