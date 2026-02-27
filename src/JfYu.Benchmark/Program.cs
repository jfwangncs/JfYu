using BenchmarkDotNet.Running;
using JfYu.Benchmark.Benchmarks;

BenchmarkSwitcher.FromTypes([
    typeof(InsertBenchmark),
    typeof(UpdateBenchmark),
    typeof(SelectBenchmark)
]).Run(args);
