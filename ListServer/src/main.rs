use std::net::{TcpListener, TcpStream};
use std::io::Read;
use std::collections::HashMap;
use std::vec::Vec;

#[repr(packed)]
struct RoomInfo {
    connections_count: u16,
    max_connections: u16,
    password: [u8; 16],
    title: String,
}

unsafe fn handle_client(mut stream: TcpStream, clients_map : &mut HashMap<u128, RoomInfo>) {
    let mut packet_size = [0 as u8; 1];
    loop {
        match stream.read(&mut packet_size) {
            Ok(_) => {
                let mut room_info_raw = Vec::new();
                match stream.read(&mut room_info_raw) {
                    Ok(_) => {
                        let room_info : RoomInfo = { std::ptr::read(room_info_raw.as_ptr() as * const _)};
                        let mut guid_raw = [0 as u8; 8];
                        match stream.read(&mut guid_raw) {
                            Ok(_) => {
                                let guid : u128 =  { std::ptr::read(guid_raw.as_ptr() as * const _)};
                                clients_map.insert(guid, room_info);
                            }
                            _ => {}
                        }
                    }
                    _ => {}
                }
            }
            _ => {}
        }
    }
}

fn main() {
    let mut clients_map : HashMap<u128, RoomInfo> = HashMap::new();
    let listener = TcpListener::bind("127.0.0.1:8887").unwrap();
    println!("Server listening on port 8887");
    for stream in listener.incoming() {
        match stream {
            Ok(stream) => unsafe {
                handle_client(stream, &mut clients_map);
            }
            _ => {}
        }
    }
}