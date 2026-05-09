#include <Servo.h>
#include <RF24.h>

// --- Визначення пінів підключення
// >- Сервопривід
#define SERVO_PIN 4
#define ESC1_PIN 5  // +-- Для кермування (вперед)
#define ESC2_PIN 6  // +-- Для підняття вверх
// >- Для піключення радіомодуля
#define CE_PIN 7
#define CSN_PIN 8

// --- Ліміти сервоприводів й моторів
// >- Ліміти сервоприводів
#define SERVO_MIN 10
#define SERVO_MAX 200
// >- Ліміти моторів
#define ESC1_MIN 1000
#define ESC1_MAX 2000
#define ESC2_MIN 1000
#define ESC2_MAX 2000

// >- Ініціалізація радіомодуля
RF24 radio(CE_PIN, CSN_PIN);
const byte address[8] = "1001001";
unsigned long lastReceiveTime = 0;
unsigned long currentTime = 0;

// >- Ініціалізація сервоприводів (та моторів)
Servo ESC1;
Servo ESC2;
Servo servo1;

bool IsRaised = false;
int ESC1_Value, ESC2_Value, servo1Value;

// >- Створення структури
struct Data {
  byte steer;
  byte throttle;
  byte brake;
  bool IsRaised;
};

// >- і ініціалізація
Data data;

void resetData() {
  data.steer = 130;
  data.throttle = 0;
  data.brake = 0;
  data.IsRaised = false;
}

void setup() {

  Serial.begin(9600);
  radio.begin();

  radio.setPALevel(RF24_PA_MAX);
  radio.setChannel(108);
  radio.openReadingPipe(0, address);
  radio.setAutoAck(false);
  radio.setDataRate(RF24_250KBPS);
  radio.setPALevel(RF24_PA_LOW);
  radio.startListening();  //  Встановлення модуля як слухача
  resetData();

  servo1.attach(SERVO_PIN);
  ESC1.attach(5, ESC1_MIN, ESC1_MAX);
  ESC1.write(ESC1_MAX);
  ESC1.write(ESC1_MIN);
  ESC2.attach(6, ESC2_MIN, ESC2_MAX);

}

void loop() {
  currentTime = millis();


  if (radio.available()) {
    radio.read(&data, sizeof(Data));
    //Serial.print(String(data.steer) + ", ");
    //Serial.print(String(data.throttle) + ", ");
    //Serial.print(String(data.brake) + ", UP:");
    //Serial.print(String(data.IsRaised) + ", B:");
    //Serial.println();
    lastReceiveTime = millis();
  }

  // -- Підняття й утримання подушки
  if (data.IsRaised) ESC2.write(ESC2_MAX);
  else ESC2.write(ESC2_MIN);

  // -- Керування сервоприводами
  servo1Value = map(data.steer, 0, 255, SERVO_MIN, SERVO_MAX);
  Serial.println(servo1Value);
  servo1.write(servo1Value);

  // -- Керування ESC (моторами)
  ESC1_Value = constrain((data.throttle - data.brake), 0, 255);
  ESC1_Value = map(ESC1_Value, 0, 255, ESC1_MIN, ESC1_MAX);
  ESC1.write(ESC1_Value);
}

void Raising() {
   ESC2.write(ESC2_MAX);
