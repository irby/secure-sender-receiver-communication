import os
import base64

client_private_key_file_name = "client.key"
client_public_key_name = "client.pub"
receiver_secret_key_name = "receiver.secret.key"

def make_dir_if_not_exist(path_to_dir):
    if(os.path.exists(path_to_dir)):
        return
    os.mkdir(path_to_dir)

def main():
    receiver_api_keys_path = os.path.join(os.getcwd(), "apps", "ReceiverApi", "keys")
    sender_console_keys_path = os.path.join(os.getcwd(), "apps", "SenderConsole", "keys")
    
    make_dir_if_not_exist(receiver_api_keys_path)
    make_dir_if_not_exist(sender_console_keys_path)

    os.system("openssl genrsa -out %s/%s 2048" % (sender_console_keys_path, client_private_key_file_name))
    os.system("openssl rsa -in %s/%s -pubout > %s/%s" % (sender_console_keys_path, client_private_key_file_name, receiver_api_keys_path, client_public_key_name))
    
    random_key = os.urandom(64)

    os.system("echo '%s' > %s/%s" % (base64.b32encode(random_key).decode('utf-8'), receiver_api_keys_path, receiver_secret_key_name))

if __name__ == '__main__':
    main()