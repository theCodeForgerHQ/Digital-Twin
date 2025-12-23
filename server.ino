#include <WiFi.h>
#include <WiFiUdp.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>

const char* ssid     = "protosem";
const char* password = "proto123";

WiFiUDP udp;
const int udpPort = 4210;
const int pins[9] = {16, 17, 18, 19, 33, 34, 23, 25, 26};

char incomingPacket[32];
bool lastButtonState = LOW;

void setup() {
  Serial.begin(115200);

  for (int i = 0; i < 9; i++) {
    pinMode(pins[i], OUTPUT);
    digitalWrite(pins[i], LOW);
  }

  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nConnected!");
  Serial.print("ESP32 IP: ");
  Serial.println(WiFi.localIP());

  udp.begin(udpPort);
  Serial.print("Listening on UDP port ");
  Serial.println(udpPort);
}

void loop() {
  int packetSize = udp.parsePacket();
  if (packetSize) {

    int len = udp.read(incomingPacket, sizeof(incomingPacket) - 1);
    if (len > 0) incomingPacket[len] = '\0';

    Serial.print("Received: ");
    Serial.println(incomingPacket);

    for (int i = 0; i < 9; i++) {
      digitalWrite(pins[i], LOW);
    }

    if      (strcmp(incomingPacket, "Stop") == 0)           digitalWrite(pins[1], HIGH);
    else if (strcmp(incomingPacket, "Reset") == 0)          digitalWrite(pins[2], HIGH);
    else if (strcmp(incomingPacket, "Forward") == 0)        digitalWrite(pins[3], HIGH);
    else if (strcmp(incomingPacket, "Reverse") == 0)        digitalWrite(pins[4], HIGH);
    else if (strcmp(incomingPacket, "Up") == 0)             digitalWrite(pins[5], HIGH);
    else if (strcmp(incomingPacket, "Down") == 0)           digitalWrite(pins[6], HIGH);
    else if (strcmp(incomingPacket, "Clockwise") == 0)      digitalWrite(pins[7], HIGH);
    else if (strcmp(incomingPacket, "Anticlockwise") == 0) digitalWrite(pins[8], HIGH);
    else if (strcmp(incomingPacket, "Start") == 0)          digitalWrite(pins[0], HIGH);
  }

}
