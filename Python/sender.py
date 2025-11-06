import cv2
import socket
import time

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ('127.0.0.1', 5052)

cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    frame = cv2.resize(frame, (320, 240))
    _, img_encoded = cv2.imencode('.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), 50])
    data = img_encoded.tobytes()

    if len(data) < 60000:
        sock.sendto(data, server_address)
    else:
        print(f"⚠️ Frame terlalu besar ({len(data)} bytes), dilewati.")

    # biar CPU nggak 100%
    time.sleep(0.03)
    if cv2.waitKey(1) == 27:  # tekan ESC untuk berhenti
        break

cap.release()
sock.close()
