use std::{env, fs, io, path::Path, process::Command};

fn main() {
    let receiver_api_keys_path_str = "./apps/ReceiverApi/keys";
    let sender_console_keys_path_str = "./apps/SenderConsole/keys";

    let receiver_api_keys_path = Path::new(receiver_api_keys_path_str);
    let sender_console_keys_path = Path::new(sender_console_keys_path_str);

    if !receiver_api_keys_path.is_dir() {
        fs::create_dir_all(receiver_api_keys_path_str);
    }

    if !sender_console_keys_path.is_dir() {
        fs::create_dir_all(sender_console_keys_path_str);
    }

    Command::new("sh")
        .arg("-c")
        .arg(format!("openssl genrsa -out {}/{} 2048", sender_console_keys_path_str, "client.key"))
        .output()
        .expect("failed to execute");

    Command::new("sh")
        .arg("-c")
        .arg(format!("openssl rsa -in {}/{} -pubout > {}/{}", sender_console_keys_path_str, "client.key", receiver_api_keys_path_str, "client.pub"))
        .output()
        .expect("failed to execute");

    Command::new("sh")
        .arg("-c")
        .arg(format!("openssl rand -base64 32 > {}/{}", receiver_api_keys_path_str, "receiver.secret.key"))
        .output()
        .expect("failed to execute");
}
