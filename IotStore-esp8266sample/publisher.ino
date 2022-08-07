#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>

const char *ssid = "Lan Solo";
const char *password = "Easy4You";

const char *address = "http://192.168.1.10:5092/api/iot/batteryVoltage";

void sendData(char *data)
{
    WiFiClient client;
    {
        HTTPClient http;
        //   WiFiClientSecure client;
        //   client.setInsecure();

        http.begin(client, address);
        http.addHeader("Content-Type", "text/plain");

        int httpCode = http.POST(data);
        Serial.print("Response code: ");
        Serial.println(httpCode);
        http.end();
    }
}

void setup()
{
    Serial.begin(115200);
    delay(1000);
    Serial.println();

    WiFi.begin(ssid, password);

    Serial.print("Connecting");
    while (WiFi.status() != WL_CONNECTED)
    {
        delay(200);
        Serial.print(".");
    }
    Serial.println();

    Serial.print("Connected, IP address: ");
    Serial.println(WiFi.localIP());
}

void loop()
{
    int rawLevel = analogRead(A0);

    // the 10kΩ/47kΩ voltage divider reduces the voltage, so the ADC Pin can handle it
    // According to Wolfram Alpha, this results in the following values:
    // 10kΩ/(47kΩ+10kΩ)*  5v = 0.8772v
    // 10kΩ/(47kΩ+10kΩ)*3.7v = 0.649v
    // 10kΩ/(47kΩ+10kΩ)*3.1v = 0.544
    // * i asumed 3.1v as minimum voltage => see LiPO discharge diagrams

    // convert battery level to percent
    int level = map(rawLevel, 500, 765, 0, 100);

    // i'd like to report back the real voltage, so apply some math to get it back
    // 1. convert the ADC level to a float
    // 2. divide by (R2[1] / R1 + R2)
    float realVoltage = (float)rawLevel / 1000 / (10000.f / (47000 + 10000));

    // build a nice string to send to influxdb or whatever you like
    char dataLine[64];
    // sprintf has no support for floats, but will be added later, so we need a String() for now
    sprintf(dataLine, "%d,%d,%s",
            rawLevel, // cap level to 100%, just for graphing, i don't want to see your lab, when the battery actually gets to that level
            level,
            String(realVoltage, 3).c_str());

    sendData(dataLine);
    Serial.println(dataLine);

    delay(60 * 1000);
}