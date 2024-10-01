/*
  Код для пристроя передачі даних управління за допомогою радіомодуля nRF24L01
  та Arduino Pro Micro (або Leonardo)

  Розробив: Broniev (Максим Куліков) - 2024 р.
*/

// --- Визначення пінів підключення
// >== Радіомодуль
#define CE_PIN 9
#define CSN_PIN 10

// >== RGB світлодіод (використовується з спільним катодом)
#define RED_PIN 5
#define GREEN_PIN 3
#define BLUE_PIN 6
#define ISCommonAnode 0

// -- Режим роботи COM порта
#define DEBUG 0

// -- Бібліотеки
#include <avr/wdt.h>
#include <SPI.h>
#include <nRF24L01.h> 
#include <RF24.h>

// -- Визначення змін для роботи
struct Data {
  byte steer;
  byte throttle;
  byte brake;
  bool F1;
  bool F2;
};

RF24 radio(CE_PIN, CSN_PIN);

//uint8_t address[][6] = { "ARCATRANSMITTOR", "ARCARECEIVER" };
const byte address[8] = "1001001";
unsigned long lastSendTime = 0;
unsigned long lastSerialCommTime = 0;
unsigned long sendInterval = 15000;
unsigned long serialCommTimeout = 10000;
unsigned long currentTime;
int SerialComErrorCounter = 0;
bool IsCOMConnected = false;
Data data;

// -- Додаткові функції
void LED_setColor(int redValue, int greenValue,  int blueValue) {
  if (ISCommonAnode) {
    analogWrite(RED_PIN, redValue);
    analogWrite(GREEN_PIN,  greenValue);
    analogWrite(BLUE_PIN, blueValue);
  } else {
    analogWrite(RED_PIN, 255 - redValue);
    analogWrite(GREEN_PIN,  255 - greenValue);
    analogWrite(BLUE_PIN, 255 - blueValue);
  }
}

void LED_setBlink(int times, int redValue, int greenValue, int blueValue) {
  for (int i = 0; i < times; i++) {
    LED_setColor(redValue, greenValue, blueValue);
    delay(50);
    LED_setColor(0, 0, 0);
    delay(50);
  }
}

void resetFunc() {
  wdt_enable(WDTO_15MS);
  while (1) {}
}

void CheckSending()
{
  if (radio.write(&data, sizeof(data))) {
    Serial.println(F("DS"));
    LED_setBlink(1, 0, 255, 0);
  }
  else {
    Serial.println(F("DS:E"));
    LED_setBlink(1, 180, 45, 0);
  }
}

/// #-- Основний код
void setup() {
  Serial.begin(9600);
  pinMode(RED_PIN,  OUTPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(BLUE_PIN, OUTPUT);

  while (!Serial) {
    LED_setColor(50, 50, 50);
    delay(500);
  }

  radio.begin();
 
  IsCOMConnected = true;
  SerialComErrorCounter = 0;
  if (!radio.isChipConnected()) {
    Serial.println("nRFNC");
    LED_setColor(255, 0, 0);
    while (1) {}
  }
  else {
    Serial.println("nRFC");
    LED_setBlink(2, 255, 0, 0);
  }

  
  //radio.setPayloadSize(sizeof(data));
  radio.setPALevel(RF24_PA_MAX);
  radio.setDataRate(RF24_250KBPS);
  radio.setChannel(110);
  radio.openWritingPipe(address);
  radio.powerUp(); 
  radio.stopListening();

}

void loop() {

  if (radio.isChipConnected()) {
    unsigned long currentTime = millis();

    if (Serial.available() >= 5) {
      data.steer = Serial.read();
      data.throttle = Serial.read();
      data.brake = Serial.read();
      data.F1 = Serial.read();
      data.F2 = Serial.read();
      lastSerialCommTime = currentTime;
      IsCOMConnected = true;
      SerialComErrorCounter = 0;
    }

    if (IsCOMConnected && currentTime - lastSendTime >= sendInterval) {
    CheckSending();
    lastSendTime = currentTime;
  }

    if (currentTime - lastSerialCommTime >= serialCommTimeout) {
      IsCOMConnected = false;
      LED_setBlink(1, 255, 0, 0);
      SerialComErrorCounter++;
      delay(2500);
      if (SerialComErrorCounter == 3) {
        resetFunc();
      }
    }

    radio.write(&data, sizeof(data));
  }
}
