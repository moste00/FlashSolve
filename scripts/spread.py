import json
import sys
import re
import math
import numpy as np

# constants
INFO_VALUES_LENGTH = 2
DURATION_MILLLIS_POSITION = -1
HASH_POSITION = -2

# read the json files 
method_name_str = re.sub('.json', '',sys.argv[1])
method_name_str = re.sub('out/sample/', '',method_name_str)
with open(sys.argv[1], "r") as method_name:
    method_data = json.load(method_name)
    print(f'#### {method_name_str} solutions: {len(method_data["values"])}')

def get_N_avg(idxs):
    N = len(idxs) #math.ceil((N*(N-1)/2.0))

    dist = 0.0
    for i in range(N):
        for j in range(i+1, N):
            dist += euclide_distance(method_data['values'][int(idxs[i])], method_data['values'][int(idxs[j])])
    return dist/(N*(N-1)/2)


def calc_all_dists():
    num_points = len(method_data['values'])
    all_distanses = []
    for i in range(num_points):
        dist = 0.0
        for j in range(num_points):
            dist += euclide_distance(method_data['values'][i], method_data['values'][j])
        all_distanses.append( [dist, i ] )
    all_distanses.sort(reverse=True)
    return np.array(all_distanses)

def euclide_distance(point1, point2):
    sum_of_squares = 0
    for i in range(len(point1)-INFO_VALUES_LENGTH):
        sum_of_squares += (point1[i] - point2[i])**2
    return math.sqrt(sum_of_squares)

def calc_time_per_solution():
    num_points = len(method_data['values'])
    acc_time = 0.0
    max_time = 0.0
    for i in range(num_points):
        current_time = ( method_data['values'][i][DURATION_MILLLIS_POSITION] ) / 1000.0
        acc_time += current_time
        if current_time > max_time:
            max_time = current_time
    print(f'Found {num_points} Solutions in: {acc_time} seconds')
    print(f'    - With an average time per solution =  {acc_time/num_points} seconds')
    print(f'    - With a worst time per solution =  {max_time} seconds')

#------------------------------------------------------------------------------------------------------#
num_points = len(method_data['values'])

# print reported spreads
print("\n"+"-"*20 + "spread analysis" + "-"*20)
if(num_points <= 4000):
    dist = 0.0
    for i in range(num_points):
        for j in range(i+1, num_points):
            dist += euclide_distance(method_data['values'][i], method_data['values'][j])

    print(f'The total distance between points is: {dist}')
    print(f'The average distance between points is: {dist/(num_points*(num_points-1)/2)}')

if(num_points > 4000):
    all_dists = calc_all_dists()
    print(f'best possible average distance for 100 points is: {get_N_avg(all_dists[0:100][:,1])}')
    print(f'best possible average distance for 1000 points is: {get_N_avg(all_dists[0:1000][:,1])}')
    print(f'best possible average distance for 2000 points is: {get_N_avg(all_dists[0:2000][:,1])}')
    print(f'best possible average distance for 2500 points is: {get_N_avg(all_dists[0:2500][:,1])}')
    print(f'best possible average distance for all points is: {get_N_avg(all_dists[:,1])}')

# print reported times
print("\n"+"-"*20 + "time analysis" + "-"*20)
calc_time_per_solution()

method_name.close()