from distutils.log import error
from re import T
import subprocess
import os
import json
from sys import stderr, stdin, stdout
import time

FNULL = open(os.path.join(os.getcwd(), "results.txt"), "a")
#path_to_dotnet = "/usr/local/share/dotnet/dotnet"
path_to_dotnet = "/mnt/c/Program Files/dotnet/dotnet.exe"
path_to_api_response = os.path.join(os.getcwd(), "apps", "SenderConsole", "SenderConsole.Core", "api_response.txt")

def read_api_response(context, expected_response):
    time.sleep(2)
    with open(path_to_api_response, "r") as f:
        result = json.loads(f.read())
        iters = 0
        while "StatusCode" not in result and iters < 5:
            time.sleep(0.5)
            iters += 1
        if iters >= 5:
            print('no output found')
            return
        response = result.get('StatusCode')
        if(response != expected_response):
            error(context + ': Failure. Expected value: ' + str(expected_response) + ' Actual value: ' + str(response))
        else:
            print(context + ': Pass')

    f = open(path_to_api_response, "w")
    f.close()
        
        

def get_challenge(proc):
    proc.stdin.write("1\n")

def send_document(proc):
    proc.stdin.write("2\n")

def clear_tokens(proc):
    proc.stdin.write("3\n")
    

def main():
    subprocess.Popen("date", stdout=FNULL, stderr=FNULL)

    print("generating new keys...")
    subprocess.Popen(
        ["python3", "generate_keys.py"],
        cwd = os.getcwd(),
        stdout=FNULL,
        stderr=FNULL
    )

    print('Starting API...')
    api = subprocess.Popen(
        [path_to_dotnet, "run", "--project ReceiverApi.csproj"],
        cwd = os.path.join(os.getcwd(), "apps", "ReceiverApi", "ReceiverApi"),
        stdout=FNULL,
        stderr=FNULL
    )
    time.sleep(4)
    print('Starting sender console...')

    proc = subprocess.Popen(
        [path_to_dotnet, "run", "--project SenderConsole.csproj"],
        cwd = os.path.join(os.getcwd(), "apps", "SenderConsole", "SenderConsole"),
        stdin=subprocess.PIPE,
        stdout=FNULL,
        stderr=FNULL,
        bufsize=1,
        universal_newlines=True
    )
    time.sleep(3)
    
    try:
        get_challenge(proc)
        read_api_response("get challenge", 200)

        send_document(proc)
        read_api_response("send document", 200)

        clear_tokens(proc)

        send_document(proc)
        read_api_response("send document", 401)

    finally:
        api.terminate()
        proc.terminate()
if __name__ == '__main__':
    main()