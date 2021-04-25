using System;
using System.Diagnostics;
using System.Threading;

namespace FindTimeQuant
{
    class Program
    {
        private static Stopwatch _stopwatch;
        private static long _elapsedMilliseconds;
        private static AutoResetEvent wh;

        static void Main(string[] args)
        {
            var processorNum = args.Length > 0 ? int.Parse(args[0]) - 1 : 1;
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr) (1 << processorNum);

            _stopwatch = new Stopwatch();
            var thread = new Thread(DoWork) {IsBackground = true};
            wh = new AutoResetEvent(false);

            thread.Start();
            wh.WaitOne();
            thread.Interrupt(); // TODO normal kill thread
            _stopwatch.Stop();
            Console.WriteLine($"Quant ticks: {_elapsedMilliseconds}");
        }

        static void DoWork()
        {
            wh.Set();
            _stopwatch.Start();
            while (true)
            {
                _elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
            }
        }
    }
}