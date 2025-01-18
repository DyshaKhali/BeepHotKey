using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

class Program
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    const int VK_F1 = 0x70; // Код клавиши F1

    static bool isBeeping = false;
    static Thread keyListenerThread;
    static WaveOutEvent waveOut;
    static SignalGenerator signalGenerator;

    static void Main(string[] args)
    {
        Console.WriteLine("Programm Start. Press F1 for 'beep'.");
        Console.WriteLine("Press Ctrl+C for exit.");

        ListInputDevices();

        signalGenerator = new SignalGenerator()
        {
            Gain = 1, 
            Frequency = 1000, 
            Type = SignalGeneratorType.Sin 
        };
        waveOut = new WaveOutEvent();

        // Поток для проверки клавиши
        keyListenerThread = new Thread(MonitorKey);
        keyListenerThread.Start();
    }

    static void ListInputDevices()
    {
        Console.WriteLine("\nList of devices:");
        var enumerator = new MMDeviceEnumerator();
        var endpoints = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
        foreach (var device in endpoints)
        {
            Console.WriteLine($"Device: {device.FriendlyName}, dataflow: {device.DataFlow.ToString()}");
        }
    }

    static void MonitorKey()
    {
        while (true)
        {
            Thread.Sleep(10);
            if ((GetAsyncKeyState(VK_F1) & 0x8000) != 0)
            {
                if (!isBeeping)
                {
                    isBeeping = true;
                    StartBeeping();
                }
            }
            else
            {
                if (isBeeping)
                {
                    isBeeping = false;
                    StopBeeping();
                }
            }
        }
    }

    static void StartBeeping()
    {
        waveOut.Init(signalGenerator);
        waveOut.Play();
    }

    static void StopBeeping()
    {
        waveOut.Stop();
    }
}
