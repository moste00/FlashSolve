import subprocess

# Define the number of times you want to run the script
algos = ['NAIVE', 'MAXSMT', 'UNI_HASH_XOR']
    # ['NAIVE', '--limit'],
    # ['MAXSMT', '--limit'],
    # ['UNI_HASH_XOR', '--limit'],
    # ]

runs = ['100', '1000', '2000', '2500']

for algo in algos:
    print("-"*50  + "\nRunning " + algo + " runs:")
    for run in runs:
        timeout = 60*4 # in seconds

        try:
            # Use subprocess to run the script and capture its output
            result = subprocess.run(["dotnet", "run", "sample", algo, '--limit', run], capture_output=True, text=True, timeout=timeout, check=True)
        except subprocess.TimeoutExpired:
            print("Subprocess for this command [ "+f'{algo} --limit {run}'+" ] timed out after", timeout, "seconds.")
            break
        print("done with run = " + run)

