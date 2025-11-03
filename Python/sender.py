import cv2
import socket
import json

# Buka webcam
cap = cv2.VideoCapture(0)

# Setup UDP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ('127.0.0.1', 5052)  # alamat Unity listener

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Konversi ke HSV dan deteksi warna merah
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    lower_red = (0, 120, 70)
    upper_red = (10, 255, 255)
    mask = cv2.inRange(hsv, lower_red, upper_red)

    contours, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    if contours:
        c = max(contours, key=cv2.contourArea)
        x, y, w, h = cv2.boundingRect(c)

        # Kirim posisi (x,y) sebagai JSON
        data = {'x': x, 'y': y, 'w': w, 'h': h}
        sock.sendto(json.dumps(data).encode(), server_address)

    cv2.imshow("frame", frame)
    if cv2.waitKey(1) == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
