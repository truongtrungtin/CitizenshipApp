/// <summary>
/// This partial Program class exists ONLY to support integration testing.
///
/// WebApplicationFactory<TEntryPoint> requires a public type from the entry assembly.
/// In minimal hosting, Program is generated implicitly and is internal by default.
///
/// By adding this file, we make Program visible to the test project without
/// changing the runtime behavior of the Api.
/// </summary>
public partial class Program { }
