import json
import sys
import re
import math
import numpy as np
import matplotlib.pyplot as plt

# read the json files 
method_name_str = re.sub('.json', '',sys.argv[1])
with open(sys.argv[1], "r") as method_name:
    method_data = json.load(method_name)
    print(f'# {method_name_str} solutions: {len(method_data["values"])}')

# calculate the euclidean distance between points in any dimension
def euclide_distance(point1, point2):
    sum_of_squares = 0
    for i in range(len(point1)):
        sum_of_squares += (point1[i] - point2[i])**2
    return math.sqrt(sum_of_squares)

# get the number of points (i.e. solutions)
num_points = len(method_data['values'])
# array to store the distances
a = []
# calculate the total distance between points
dist = 0.0
for i in range(num_points):
    for j in range(i+1, num_points):
        temp = euclide_distance(method_data['values'][i], method_data['values'][j])
        dist += temp
        a.append(temp)

# print the total distance, average distance, maximum distance, minimum distance
print(f'The total distance between points is: {dist}')
avg = dist/(num_points*(num_points-1)/2)
print(f'The average distance between points is: {avg}')
print(f'The maximum distance between points is: {max(a)}')
print(f'The minimum distance between points is: {min(a)}')

# count how many distances are below the average
a = np.array(a)
count = np.count_nonzero(a < avg)
print(f'The number of distances below the average is: {count} out of {len(a)} distances')
print(f'The ratio of number of distances below the average is: {count*100/len(a)}%')

# plot the histogram
plt.hist(a, bins=100)
plt.title(f'{method_name_str} distance histogram')
plt.xlabel('Distance')
plt.ylabel('Frequency')
plt.savefig(f'{method_name_str}_histogram.png')
plt.show()

method_name.close()