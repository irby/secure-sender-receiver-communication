import subprocess
import os
import json
from sys import stdout
import time

FNULL = open(os.path.join(os.getcwd(), "results.txt"), "a")
path_to_dotnet = "/mnt/c/Program Files/dotnet/dotnet.exe"


def main():
    # api = subprocess.Popen(
    #     [path_to_dotnet, "run", "--project ReceiverApi.csproj"],
    #     cwd = os.path.join(os.getcwd(), "apps", "SenderReceiver", "ReceiverApi"),
    #     stdout=FNULL,
    #     stderr=FNULL
    # )
    proc = subprocess.Popen(
        [path_to_dotnet, "run", "--project SenderConsole.csproj"],
        cwd = os.path.join(os.getcwd(), "apps", "SenderConsole", "SenderConsole"),
        stdout=FNULL,
        stderr=FNULL,
        bufsize=1,
        universal_newlines=True
    )
if __name__ == '__main__':
    main()