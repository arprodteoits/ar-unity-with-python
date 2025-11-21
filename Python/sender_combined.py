import cv2
import socket
import json
import numpy as np
import mediapipe as mp
from scipy.spatial.transform import Rotation as R

sock_img = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr_img = ('127.0.0.1', 5052)

sock_pos = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr_pos = ('127.0.0.1', 5053)

mp_hands = mp.solutions.hands
mp_draw = mp.solutions.drawing_utils
hands = mp_hands.Hands(max_num_hands=1, min_detection_confidence=0.6)

cap = cv2.VideoCapture(0)

def normalize(v):
    return v / np.linalg.norm(v)

while True:
    ret, frame = cap.read()
    if not ret: break

    frame = cv2.resize(frame, (1280, 720))
    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(rgb)

    if results.multi_hand_landmarks:
        lm = results.multi_hand_landmarks[0]
        h, w, _ = frame.shape

        def LM(id):
            p = lm.landmark[id]
            return np.array([p.x * w, p.y * h, p.z])

        wrist = LM(0)
        index = LM(5)
        pinky = LM(17)

        right = normalize(index - pinky)
        forward = normalize(np.cross(right, wrist - index))
        up = normalize(np.cross(forward, right))

        rot_matrix = np.array([right, up, forward]).T
        quat = R.from_matrix(rot_matrix).as_quat()  # x, y, z, w

        xs = [int(p.x * w) for p in lm.landmark]
        ys = [int(p.y * h) for p in lm.landmark]
        x_min, x_max = min(xs), max(xs)
        y_min, y_max = min(ys), max(ys)
        w_box = x_max - x_min
        h_box = y_max - y_min
        cx = x_min + w_box / 2
        cy = y_min + h_box / 2

        # KIRIM JSON
        data = json.dumps({
            "x": cx,
            "y": cy,
            "w": w_box,
            "h": h_box,
            "qx": float(quat[0]),
            "qy": float(quat[1]),
            "qz": float(quat[2]),
            "qw": float(quat[3])
        }).encode()

        sock_pos.sendto(data, addr_pos)

    else:
        sock_pos.sendto(json.dumps({"x": -1}).encode(), addr_pos)

    cv2.imshow("Hand", frame)
    if cv2.waitKey(1) == 27: break
