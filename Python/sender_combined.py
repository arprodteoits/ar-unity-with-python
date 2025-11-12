import cv2
import socket
import json
import numpy as np

# --- Socket untuk gambar (ke UDPReceiver Unity) ---
sock_img = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr_img = ('127.0.0.1', 5052)

# --- Socket untuk posisi (ke PythonReceiver Unity) ---
sock_pos = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr_pos = ('127.0.0.1', 5053)

cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    frame = cv2.resize(frame, (1280, 720))
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

    # --- deteksi warna merah ---
    lower_red1 = np.array([0, 120, 70])
    upper_red1 = np.array([10, 255, 255])
    lower_red2 = np.array([170, 120, 70])
    upper_red2 = np.array([180, 255, 255])
    mask1 = cv2.inRange(hsv, lower_red1, upper_red1)
    mask2 = cv2.inRange(hsv, lower_red2, upper_red2)
    mask = mask1 + mask2

    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    if contours:
        c = max(contours, key=cv2.contourArea)
        x, y, w, h = cv2.boundingRect(c)
        cv2.rectangle(frame, (x, y), (x+w, y+h), (0,255,0), 2)

        # --- kirim posisi ke Unity (port 5053) ---
        data = json.dumps({"x": x + w/2, "y": y + h/2, "w": w, "h": h}).encode('utf-8')
        sock_pos.sendto(data, addr_pos)

    # --- kirim gambar ke Unity (port 5052) ---
    _, img_encoded = cv2.imencode('.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), 50])
    data = img_encoded.tobytes()
    if len(data) < 60000:
        sock_img.sendto(data, addr_img)

    # --- tampilkan preview ---
    cv2.imshow("Camera", frame)
    if cv2.waitKey(1) == 27:
        break

cap.release()
sock_img.close()
sock_pos.close()
cv2.destroyAllWindows()
