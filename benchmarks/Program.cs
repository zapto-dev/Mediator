using BenchmarkDotNet.Running;

BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
