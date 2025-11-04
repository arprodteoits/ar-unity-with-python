import cv2
import socket
import struct

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ('127.0.0.1', 5052)

cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # resize & kompres
    frame = cv2.resize(frame, (320, 240))
    _, img_encoded = cv2.imencode('.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), 50])
    data = img_encoded.tobytes()

    # kirim hanya jika ukuran aman
    if len(data) < 60000:  # UDP limit
        sock.sendto(data, server_address)
    else:
        print(f"⚠️ Frame terlalu besar ({len(data)} bytes), dilewati.")

cap.release()
sock.close()
