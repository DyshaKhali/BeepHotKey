using System;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.Wave;
using NAudio.Mixer;

class Program
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    const int VK_F1 = 0x70; // Код клавиши F1

    static bool isBeeping = false;
    static WasapiLoopbackCapture micCapture;
    static BufferedWaveProvider waveProvider;
    static WaveOutEvent waveOut;
    static Thread keyListenerThread;

    static void Main(string[] args)
    {
        Console.WriteLine("Запуск программы. Удерживайте F1 для 'пиканья'.");
        Console.WriteLine("Нажмите Ctrl+C для выхода.");

        // Инициализация реального микрофона
        micCapture = new WasapiLoopbackCapture();
        waveProvider = new BufferedWaveProvider(micCapture.WaveFormat);

        micCapture.DataAvailable += (s, e) =>
        {
            if (!isBeeping)
            {
                waveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
            }
        };
        micCapture.StartRecording();

        // Настройка выхода (виртуальный микрофон)
        waveOut = new WaveOutEvent();
        waveOut.Init(waveProvider);
        waveOut.Play();

        // Поток для проверки клавиши
        keyListenerThread = new Thread(MonitorKey);
        keyListenerThread.Start();
    }

    static void MonitorKey()
    {
        while (true)
        {
            Thread.Sleep(10); // Проверяем клавишу каждые 10 мс
            if ((GetAsyncKeyState(VK_F1) & 0x8000) != 0) // F1 нажата
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
        Console.WriteLine("Начало пиканья");
        var beepWave = GenerateBeep(micCapture.WaveFormat.SampleRate, 1000); // Генерация 1000 Гц
        waveProvider.ClearBuffer();
        waveProvider.AddSamples(beepWave, 0, beepWave.Length);
    }

    static void StopBeeping()
    {
        Console.WriteLine("Остановка пиканья");
        waveProvider.ClearBuffer(); // Возврат к реальному звуку
    }

    static byte[] GenerateBeep(int sampleRate, int frequency)
    {
        int durationMs = 200; // Длина "пика"
        int sampleCount = (sampleRate * durationMs) / 1000;
        byte[] buffer = new byte[sampleCount * 2];

        for (int i = 0; i < sampleCount; i++)
        {
            short amplitude = (short)(Math.Sin(2 * Math.PI * frequency * i / sampleRate) * short.MaxValue);
            buffer[i * 2] = (byte)(amplitude & 0xFF);
            buffer[i * 2 + 1] = (byte)((amplitude >> 8) & 0xFF);
        }

        return buffer;
    }
}
