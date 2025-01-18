using System;
using System.Runtime.InteropServices;
using System.Threading;
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
        Console.WriteLine("Запуск программы. Удерживайте F1 для 'пиканья'.");
        Console.WriteLine("Нажмите Ctrl+C для выхода.");

        // Инициализация звукового генератора
        signalGenerator = new SignalGenerator()
        {
            Gain = 1, // Громкость
            Frequency = 1000, // Частота (Гц)
            Type = SignalGeneratorType.Sin // Тип сигнала (синусоидальный)
        };
        waveOut = new WaveOutEvent();

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
        waveOut.Init(signalGenerator);
        waveOut.Play();
    }

    static void StopBeeping()
    {
        Console.WriteLine("Остановка пиканья");
        waveOut.Stop();
    }
}
