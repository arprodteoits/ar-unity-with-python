import cv2
import socket
import json

# --- Socket setup ---
sock_video = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
sock_pos = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
unity_ip = '127.0.0.1'
video_port = 5052
pos_port = 5053

# --- Kamera ---
cap = cv2.VideoCapture(0)
lower_red = (0, 120, 70)
upper_red = (10, 255, 255)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Resize dan kirim gambar
    frame = cv2.resize(frame, (320, 240))
    _, img_encoded = cv2.imencode('.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), 50])
    data = img_encoded.tobytes()
    if len(data) < 60000:
        sock_video.sendto(data, (unity_ip, video_port))

    # Deteksi objek merah sederhana (contoh)
    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    mask = cv2.inRange(hsv, lower_red, upper_red)
    contours, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    if contours:
        c = max(contours, key=cv2.contourArea)
        x, y, w, h = cv2.boundingRect(c)
        data_dict = {"x": x, "y": y, "w": w, "h": h}
        json_str = json.dumps(data_dict)
        sock_pos.sendto(json_str.encode('utf-8'), (unity_ip, pos_port))

    # (opsional) tampilkan di jendela Python
    cv2.imshow('Camera', frame)
    if cv2.waitKey(1) == 27:
        break

cap.release()
cv2.destroyAllWindows()
sock_video.close()
sock_pos.close()
