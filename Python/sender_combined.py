import cv2
import socket
import json
import numpy as np
import mediapipe as mp

# --- Socket untuk gambar (ke UDPReceiver Unity) ---
sock_img = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr_img = ('127.0.0.1', 5052)

# --- Socket untuk posisi (ke PythonReceiver Unity) ---
sock_pos = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr_pos = ('127.0.0.1', 5053)

# MediaPipe hands
mp_hands = mp.solutions.hands
mp_draw = mp.solutions.drawing_utils
hands = mp_hands.Hands(max_num_hands=1, min_detection_confidence=0.6)

cap = cv2.VideoCapture(0)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    frame = cv2.resize(frame, (1280, 720))
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)

    results = hands.process(rgb)

    if results.multi_hand_landmarks:
        hand_landmarks = results.multi_hand_landmarks[0]

        # Ambil bounding box dari semua titik tangan
        h, w, c = frame.shape
        xs = []
        ys = []

        for lm in hand_landmarks.landmark:
            xs.append(int(lm.x * w))
            ys.append(int(lm.y * h))

        x_min, x_max = min(xs), max(xs)
        y_min, y_max = min(ys), max(ys)
        w_box = x_max - x_min
        h_box = y_max - y_min

        cx = x_min + w_box / 2
        cy = y_min + h_box / 2

        # Draw bounding box di preview
        cv2.rectangle(frame, (x_min, y_min), (x_max, y_max), (0,255,0), 2)

        # Kirim posisi ke Unity
        data = json.dumps({
            "x": cx,
            "y": cy,
            "w": w_box,
            "h": h_box
        }).encode('utf-8')

        sock_pos.sendto(data, addr_pos)

    else:
            # --- TIDAK ADA tangan ---
        data = json.dumps({"x": -1, "y": -1, "w": 0, "h": 0}).encode("utf-8")
        sock_pos.sendto(data, addr_pos)

        # (opsional) gambar skeleton
        mp_draw.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    # --- kirim gambar ke Unity ---
    _, img_encoded = cv2.imencode('.jpg', frame, [int(cv2.IMWRITE_JPEG_QUALITY), 50])
    data = img_encoded.tobytes()
    
    if len(data) < 60000:
        sock_img.sendto(data, addr_img)

    # Preview
    cv2.imshow("Hand Tracker", frame)
    if cv2.waitKey(1) == 27:
        break

cap.release()
sock_img.close()
sock_pos.close()
cv2.destroyAllWindows()
