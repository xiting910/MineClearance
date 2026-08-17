using MineClearance.UI.Tests;
using Xunit.Sdk;
using Xunit.v3;

// 程序集级夹具: 在第一个测试运行前统一设置数据目录环境变量, 确保测试不触碰真实数据目录
[assembly: AssemblyFixture(typeof(TestEnvironmentFixture))]

// UI 常量的静态路径字段在首次访问时按环境变量求值, 禁用并行保证夹具先于所有测试生效
[assembly: Parallelization(Mode = ParallelMode.None)]
