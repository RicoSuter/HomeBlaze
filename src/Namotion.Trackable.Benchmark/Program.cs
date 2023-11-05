using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Namotion.Trackable.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            Run();
#else
            BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).RunAllJoined();
#endif
        }

        private static void Run()
        {
            var benchmark = new TrackableBenchmark();
            benchmark.Setup();
            RunCode(benchmark);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RunCode(TrackableBenchmark benchmark)
        {
            var watch = Stopwatch.StartNew();

            var outer = 100;
            var inner = 100;

            var total = outer * inner;
            for (int i = 0; i < outer; ++i)
            {
                for (int j = 0; j < inner; ++j)
                {
                    benchmark.ChangeAllTires();
                }
                Console.WriteLine($"{i * inner}/{total} ({watch.ElapsedMilliseconds / (i + 1m) / inner} ms)");
            }
        }
    }
}