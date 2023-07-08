import numpy as np
import matplotlib.pyplot as plt
import csv 
import pandas as pd

# Read in the data
df_our = pd.read_csv('our_spread.csv')
df_siemens = pd.read_csv('siemens_spread.csv')

# store the data in a list
our_spread_50 = []
siemens_spread_50 = []
our_spread_100 = []
siemens_spread_100 = []
our_spread_1000 = []
siemens_spread_1000 = []
our_spread_10000 = []
siemens_spread_10000 = []

problem_name_50 = []
problem_name_100 = []
problem_name_1000 = []
problem_name_10000 = []

# add to the list if the key contains the value 50
for key, avg_spread in zip(df_our['file'], df_our['avg_spread']):
    if '50' in key:
        our_spread_50.append(avg_spread)
        problem_name_50.append(key)
    elif '10000' in key:
        our_spread_10000.append(avg_spread)
        problem_name_10000.append(key)
    elif '1000' in key:
        our_spread_1000.append(avg_spread)
        problem_name_1000.append(key)
    elif '100' in key:
        our_spread_100.append(avg_spread)
        problem_name_100.append(key)
    
    

# add to the list if the key contains the value 50
for key, avg_spread in zip(df_siemens['file'], df_siemens['avg_spread']):
    if '50' in key:
        siemens_spread_50.append(avg_spread)
    elif '10000' in key:
        siemens_spread_10000.append(avg_spread)
    elif '1000' in key:
        siemens_spread_1000.append(avg_spread)
    elif '100' in key:
        siemens_spread_100.append(avg_spread)
    
# take the log of the data
our_spread_50 = np.log(our_spread_50)
siemens_spread_50 = np.log(siemens_spread_50)
our_spread_100 = np.log(our_spread_100)
siemens_spread_100 = np.log(siemens_spread_100)
our_spread_1000 = np.log(our_spread_1000)
siemens_spread_1000 = np.log(siemens_spread_1000)
our_spread_10000 = np.log(our_spread_10000)
siemens_spread_10000 = np.log(siemens_spread_10000)

# plot a barplot for the data out_spread_50 and siemens_spread_50 the spread in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_50)), problem_name_50, rotation=90)
x = np.arange(len(problem_name_50))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_spread_50, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_spread_50, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Spread(log scale)')
# plt.ylim(0, 300)
plt.title('Spread of the problems with 50 solutions')
plt.legend()
plt.show()

# plot a barplot for the data out_spread_100 and siemens_spread_100 the spread in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_100)), problem_name_100, rotation=90)
x = np.arange(len(problem_name_100))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_spread_100, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_spread_100, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Spread(log scale)')
# plt.ylim(0, 300)
plt.title('Spread of the problems with 100 solutions')
plt.legend()
plt.show()

# plot a barplot for the data out_spread_100 and siemens_spread_1000 the spread in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_1000)), problem_name_1000, rotation=90)
x = np.arange(len(problem_name_1000))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_spread_1000, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_spread_1000, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Spread(log scale)')
# plt.ylim(0, 300)
plt.title('Spread of the problems with 1000 solutions')
plt.legend()
plt.show()

# plot a barplot for the data out_spread_100 and siemens_spread_1000 the spread in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_10000)), problem_name_10000, rotation=90)
x = np.arange(len(problem_name_10000))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_spread_10000, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_spread_10000, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Spread(log scale)')
# plt.ylim(0, 300)
plt.title('Spread of the problems with 10000 solutions')
plt.legend()
plt.show()