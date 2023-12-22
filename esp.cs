#include <DHT.h>
#include <FirebaseESP8266.h>
#include <ESP8266WiFi.h>

#define DHTPIN 2        // Pin where the DHT sensor is connected
#define DHTTYPE DHT22   // Change this if you are using a different DHT sensor model

DHT dht(DHTPIN, DHTTYPE);

#define FIREBASE_HOST "your-firebase-url.firebaseio.com"
#define FIREBASE_AUTH "your-firebase-auth-token"
#define WIFI_SSID "your-wifi-ssid"
#define WIFI_PASSWORD "your-wifi-password"

FirebaseData firebaseData;

void setup() {
  Serial.begin(115200);
  dht.begin();

  connectToWiFi();

  Firebase.begin(FIREBASE_HOST, FIREBASE_AUTH);
}

void loop() {
  float humidity = dht.readHumidity();
  float temperature = dht.readTemperature();

  if (isnan(humidity) || isnan(temperature)) {
    Serial.println("Failed to read from DHT sensor!");
    delay(2000);  // Wait for 2 seconds before retrying
    return;
  }

  Serial.print("Humidity: ");
  Serial.print(humidity);
  Serial.print("%\t");
  Serial.print("Temperature: ");
  Serial.print(temperature);
  Serial.println("°C");

  // Update Firebase
  if (updateFirebaseData("humidity", humidity) && updateFirebaseData("temperature", temperature)) {
    Serial.println("Firebase update successful");
  } else {
    Serial.println("Failed to update Firebase");
  }

  delay(5000);  // Delay for 5 seconds
}

void connectToWiFi() {
  Serial.println("\nConnecting to WiFi");
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  int attempts = 0;
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.print(".");
    attempts++;

    if (attempts > 20) {
      Serial.println("\nFailed to connect to WiFi. Please check your credentials or network.");
      delay(5000);  // Wait for 5 seconds before retrying
      ESP.restart();
    }
  }

  Serial.println("\nConnected to WiFi");
}

bool updateFirebaseData(const char* key, float value) {
  Serial.print("Updating Firebase with ");
  Serial.print(key);
  Serial.print(": ");
  Serial.print(value);

  if (Firebase.setFloat(firebaseData, key, value)) {
    return true;
  } else {
    Serial.println("\nFailed to update Firebase: " + firebaseData.errorReason());
    return false;
  }
}
