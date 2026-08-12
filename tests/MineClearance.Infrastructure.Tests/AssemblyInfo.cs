using MineClearance.Infrastructure.Tests;

// 程序集级夹具: 在第一个测试运行前统一设置数据目录环境变量, 确保测试不触碰真实数据目录
[assembly: AssemblyFixture(typeof(TestEnvironmentFixture))]

// 文件系统测试共享同一数据根目录, 禁用并行避免测试间文件冲突
[assembly: CollectionBehavior(DisableTestParallelization = true)]
