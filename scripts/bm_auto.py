'''
    please make sure to run this file from the base directory 
    like that :
        python3 scripts/auto_bm.py

    and if u r using windows then make sure to 
    replace ( / ) with ( \ ) in the path strings witten below
'''
import subprocess
import os

# Get all files in the directory
directory_path = 'out/sample'
file_names = os.listdir(directory_path)
file_names = sorted(file_names)

# Define the number of times you want to run the script
num_runs = 5

# Open a file to write the output to
with open('out'+"/benchmark.md", "w") as f:
    # Loop through the desired number of runs
    for file_name in file_names:
        # Use subprocess to run the script and capture its output
        result = subprocess.run(["python3", "scripts/spread.py", f"{directory_path}/{file_name}"], capture_output=True, text=True)
        # Write the output to the file
        f.write(f"# Running file - {file_name}:\n")
        f.write("-"*55 + "\n")
        f.write(result.stdout)
        f.write("\n")
        f.write("-"*55 + "\n")
