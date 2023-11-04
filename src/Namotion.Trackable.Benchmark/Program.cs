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
            for (int i = 0; i < 10000000; ++i)
            {
                //benchmark.ChangeAllTiresAndCheckPressure();
            }
        }
    }
}