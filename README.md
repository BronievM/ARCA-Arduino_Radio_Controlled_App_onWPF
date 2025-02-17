# Arduino-Based Remote Camera Access (ARCA)

This is my team’s diploma project, which enables the connection of Arduino-based remote devices with cameras, such as the ESP32 or other IP cameras.

👨‍💻 The code for the Arduino sending device is included. However, the receiver code will be published later.

---

## Screenshots

![ARCA Main Window](/images/mainwindow-controllerConnected.jpg "ARCA Main Window")  
![ARCA Connected to a Public Camera](/images/mainwindow-connectedToSomeCamera.jpg "Connected to a Public Camera")  

---

## IP Cameras for Testing

You can find some IPs for testing here:  
[GitHub - Public IP Cameras](https://github.com/fury999io/public-ip-cams?tab=readme-ov-file)

---

## Known Issues

- The program may freeze when Bluetooth is turned on.  
  This happens because it tries to retrieve names of unknown COM ports via Bluetooth.
