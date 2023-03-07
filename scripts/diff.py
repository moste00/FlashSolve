import json
import sys
import re

# read the json files 
for i in range(1, len(sys.argv)):
    with open(sys.argv[i], "r") as globals()[f"method_{i}"]:
        globals()[f"method_{i}_data"] = json.load(globals()[f"method_{i}"])
    globals()[f"method_{i}_str"] = re.sub('.json', '',sys.argv[i])
    print(f'# {globals()[f"method_{i}_str"]} solutions: {len(globals()[f"method_{i}_data"]["values"])}')

def diff(method1sols, method2sols):
    return max(len(method1sols.difference(method2sols)), len(method2sols.difference(method1sols)))
def intersection(method1sols, method2sols):
    return len(method1sols.intersection(method2sols))

# putting the solution of each method in a set and printing the length of each set (the number of unique solutions)
for i in range(1, len(sys.argv)):
    globals()[f"method_{i}_set"] = set(tuple(x) for x in globals()[f"method_{i}_data"]['values'])
    print(f'The number of unique solutions of the {globals()[f"method_{i}_str"]} method: {len(globals()[f"method_{i}_set"])}')


for i in range(1, len(sys.argv)):
    for j in range(i+1, len(sys.argv)):
        d = diff(globals()[f"method_{i}_set"], globals()[f"method_{j}_set"])
        print(f'The number of unique solutions that are different in the {globals()[f"method_{i}_str"]} and {globals()[f"method_{j}_str"]} methods: {d}')
        inter = intersection(globals()[f"method_{i}_set"], globals()[f"method_{j}_set"])
        print(f'The number of unique solutions that are the same in the {globals()[f"method_{i}_str"]} and {globals()[f"method_{j}_str"]} methods: {inter}')


for i in range(1, len(sys.argv)):
    globals()[f"method_{i}"].close()