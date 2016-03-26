#include <Adafruit_NeoPixel.h>

#include <SoftwareSerial.h>   // We need this even if we're not using a SoftwareSerial object Due to the way the Arduino IDE compiles
#include <SerialCommand.h>

#define PIN            6
#define NUMPIXELS      8
#define ANIMDURATION   300

Adafruit_NeoPixel pixels = Adafruit_NeoPixel(NUMPIXELS, PIN, NEO_GRB + NEO_KHZ800);
SerialCommand SCmd;

// Primary State
bool IsRed = false;
bool IsBlue = false;
bool IsLocked = false;
byte Power = 0;

// State Helpers
int FlashAnim = 0;

void setup()
{
  Serial.begin(9600);

  // Serial Commands
  SCmd.addCommand("connect",  SetConnected);
  SCmd.addCommand("red",      SetRed);
  SCmd.addCommand("blu",      SetBlue);
  SCmd.addCommand("lck",      SetLocked);
  SCmd.addCommand("pwr",      SetPower);
  SCmd.addCommand("clr",      Clear);
  SCmd.addCommand("bright",   SetBrightness);
  SCmd.addDefaultHandler(unrecognized);

  pixels.begin();
  pixels.show();
}

void unrecognized()
{
  SetColor(255, 255, 0);
  delay(1000);
}

boolean IsReadyToShoot()
{
  return IsLocked && (Power >= 254);
}

void loop()
{
  SCmd.readSerial();
  
  if (IsReadyToShoot())
  {
    byte val = map(FlashAnim, 0, ANIMDURATION, 255, 0);
    SetColor(0, max(val, 64), 0);
    
    pixels.show();

    FlashAnim += 1;
    if (FlashAnim > ANIMDURATION)
    {
      FlashAnim = 0;
    }
  }

  delay(1);
}

void UpdateColor()
{
  if (IsReadyToShoot())
  {
    FlashAnim = 0;
  }
  else
  {
    if (IsRed)
        SetColor(255, 0, 0);
    
    if (IsBlue)
      SetColor(0, 0, 255);
    
    if (!IsRed && !IsBlue)
      SetColor(0, 0, 0);
    

    if (IsLocked)
    {
      int count = map(Power, 0, 255, NUMPIXELS / 2, NUMPIXELS);
      for (int i = 0; i < count; i++)
      {
        pixels.setPixelColor(i, 0, 255, 0);
      }
    }

    pixels.show();
  }
}

void SetColor(byte r, byte g, byte b)
{
  for(int i = 0; i < NUMPIXELS; i++)
  {
    pixels.setPixelColor(i, r, g, b);
  }
}

void SetConnected()
{
  //Serial.println("Set Connected");
  boolean isConnected = atoi(SCmd.next()) > 0;
  //Serial.println(isConnected);
  
  Clear();
  if (!isConnected)
  {
    for (int i = 0; i < NUMPIXELS; i += 2)
      {
        pixels.setPixelColor(i, 255, 255, 0);
      }
  }
  pixels.show();
}

void SetRed()
{
  //Serial.println("Set Red");
  IsRed = atoi(SCmd.next()) > 0;
  //Serial.println(IsRed);
  UpdateColor();
}

void SetBlue()
{
  //Serial.println("Set Blue");
  IsBlue = atoi(SCmd.next()) > 0;
  UpdateColor();
}

void SetLocked()
{
  //Serial.println("Set Locked");
  IsLocked = atoi(SCmd.next()) > 0;
  UpdateColor();
}

void SetPower()
{
  //Serial.println("Set Power");
  int powerRaw = atoi(SCmd.next());
  //Serial.println(powerRaw);
  Power = max(0, min(powerRaw, 255));
  UpdateColor();
}

void Clear()
{
  //Serial.println("Clear All");
  IsRed = false;
  IsBlue = false;
  IsLocked = false;
  Power = 0;
  UpdateColor();
}

void SetBrightness()
{
  //Serial.println("Set Brightness");
  byte val = atoi(SCmd.next());
  pixels.setBrightness(map(val, 0, 255, 0, 128));
  pixels.show();
}


