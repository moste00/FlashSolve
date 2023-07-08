import numpy as np
import matplotlib.pyplot as plt
import csv 
import pandas as pd

# Read in the data
df_our = pd.read_csv('our_time.csv')
df_siemens = pd.read_csv('siemens_time.csv')

# store the data in a list
our_time_50 = []
siemens_time_50 = []
our_time_100 = []
siemens_time_100 = []
our_time_1000 = []
siemens_time_1000 = []
our_time_10000 = []
siemens_time_10000 = []

problem_name_50 = []
problem_name_100 = []
problem_name_1000 = []
problem_name_10000 = []

# add to the list if the key contains the value 50
for key, total_time in zip(df_our['file'], df_our['total time']):
    if '50' in key:
        our_time_50.append(total_time)
        problem_name_50.append(key)
    elif '10000' in key:
        our_time_10000.append(total_time)
        problem_name_10000.append(key)
    elif '1000' in key:
        our_time_1000.append(total_time)
        problem_name_1000.append(key)
    elif '100' in key:
        our_time_100.append(total_time)
        problem_name_100.append(key)
    
    

# add to the list if the key contains the value 50
for key, total_time in zip(df_siemens['file'], df_siemens['total time']):
    if '50' in key:
        siemens_time_50.append(total_time)
    elif '10000' in key:
        siemens_time_10000.append(total_time)
    elif '1000' in key:
        siemens_time_1000.append(total_time)
    elif '100' in key:
        siemens_time_100.append(total_time)
    
    


# plot a barplot for the data out_time_50 and siemens_time_50 the time in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_50)), problem_name_50, rotation=90)
x = np.arange(len(problem_name_50))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_time_50, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_time_50, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Time (s)')
plt.ylim(0, 0.04)
plt.title('Time taken to solve the problem with 50 solutions')
plt.legend()
plt.show()

# plot a barplot for the data out_time_100 and siemens_time_100 the time in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_100)), problem_name_100, rotation=90)
x = np.arange(len(problem_name_100))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_time_100, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_time_100, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Time (s)')
plt.ylim(0, 0.08)
plt.title('Time taken to solve the problem with 100 solutions')
plt.legend()
plt.show()

# plot a barplot for the data out_time_100 and siemens_time_1000 the time in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_1000)), problem_name_1000, rotation=90)
x = np.arange(len(problem_name_1000))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_time_1000, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_time_1000, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Time (s)')
plt.ylim(0, 0.35)
plt.title('Time taken to solve the problem with 1000 solutions')
plt.legend()
plt.show()

# plot a barplot for the data out_time_100 and siemens_time_1000 the time in the y axis and the problem name in the x axis
# add this information to the plot to the x axis
# make them side by side
plt.figure(figsize=(20, 10))
plt.xticks(range(len(problem_name_10000)), problem_name_10000, rotation=90)
x = np.arange(len(problem_name_10000))
bar_width = 0.35
# Plot the bars
plt.bar(x - bar_width/2, our_time_10000, width=bar_width, label='FlashSolve')
plt.bar(x + bar_width/2, siemens_time_10000, width=bar_width, label='QuestaSim')
plt.xlabel('Problem Name')
plt.ylabel('Time (s)')
plt.ylim(0, 2.5)
plt.title('Time taken to solve the problem with 10000 solutions')
plt.legend()
plt.show()




