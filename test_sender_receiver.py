from re import T
import subprocess
import os
import json
from sys import stderr, stdout
import time

FNULL = open(os.path.join(os.getcwd(), "results.txt"), "a")
path_to_dotnet = "/usr/local/share/dotnet/dotnet"


def main():
    subprocess.Popen("date", stdout=FNULL, stderr=FNULL)
    print('Starting API...')
    api = subprocess.Popen(
        [path_to_dotnet, "run", "--project ReceiverApi.csproj"],
        cwd = os.path.join(os.getcwd(), "apps", "ReceiverApi", "ReceiverApi"),
        stdout=FNULL,
        stderr=FNULL
    )
    time.sleep(5)
    print('Starting sender console...')

    proc = subprocess.Popen(
        ["dotnet", "run", "--project SenderConsole.csproj"],
        cwd = os.path.join(os.getcwd(), "apps", "SenderConsole", "SenderConsole"),
        stdout=FNULL,
        stderr=FNULL,
        bufsize=1,
        universal_newlines=True
    )
    
    try:
        print('in here')

    finally:
        api.kill()
if __name__ == '__main__':
    main()