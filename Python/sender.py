import cv2
import socket
import json
import base64

cap = cv2.VideoCapture(0)

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
addr = ('127.0.0.1', 5052)

while True:
    ret, frame = cap.read()
    if not ret:
        break

    hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)
    lower_red = (0, 120, 70)
    upper_red = (10, 255, 255)
    mask = cv2.inRange(hsv, lower_red, upper_red)
    contours, _ = cv2.findContours(mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)

    obj_data = {'x': 0, 'y': 0, 'frame': ''}

    if contours:
        c = max(contours, key=cv2.contourArea)
        x, y, w, h = cv2.boundingRect(c)
        obj_data['x'], obj_data['y'] = x, y
        cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)

    # Encode frame ke JPEG lalu base64
    _, buffer = cv2.imencode('.jpg', frame)
    jpg_as_text = base64.b64encode(buffer).decode('utf-8')
    obj_data['frame'] = jpg_as_text

    # Kirim JSON berisi gambar + posisi
    message = json.dumps(obj_data).encode()
    sock.sendto(message, addr)

    if cv2.waitKey(1) == ord('q'):
        break

cap.release()
cv2.destroyAllWindows()
