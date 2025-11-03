# About This Project

📋 Deskripsi Proyek
Proyek ini mengintegrasikan Unity (Visualisasi 3D) dengan Python (OpenCV) untuk membuat aplikasi Augmented Reality sederhana.
Python menangkap video dari webcam dan mengirimkan data posisi objek (hasil deteksi warna atau marker) ke Unity melalui UDP socket.
Unity menampilkan objek 3D yang bergerak mengikuti posisi hasil deteksi.

Struktur ini juga dapat diakses melalui Oculus Quest 1 menggunakan Chrome Remote Desktop.

🏗️ Arsitektur Sistem
Oculus Quest 1

     ↓

Chrome Remote Desktop

     ↓

PC / Laptop

     ↓
     
Python (OpenCV)  →  Unity (Visualisasi AR)

## Fitur Utama

* 🔴 Deteksi objek sederhana (warna, marker, atau bentuk)
* 🧩 Komunikasi real-time antara Python dan Unity
* 🎮 Visualisasi hasil deteksi ke objek 3D
* 🕶️ Dapat diakses lewat Oculus Quest 1 untuk simulasi AR

## Teknologi yang Digunakan

| Komponen                      | Deskripsi                                           |
| ----------------------------- | --------------------------------------------------- |
| **Unity**                     | Engine 3D untuk visualisasi                         |
| **Python 3.8+**               | Bahasa pemrosesan video & data                      |
| **OpenCV**                    | Library deteksi gambar/video                        |
| **Socket (UDP)**              | Komunikasi antar aplikasi                           |
| **Oculus Quest 1 (opsional)** | Akses tampilan jarak jauh via Chrome Remote Desktop |

# Instalasi dan Setup

1. Prasyarat
   * Unity 2021.3 LTS (disarankan)
   * Python 3.8–3.11
   * Paket Python:
  
  pip install opencv-python
