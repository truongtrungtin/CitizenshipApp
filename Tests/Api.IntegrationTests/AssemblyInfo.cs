// Integration tests share a single in-memory SQLite connection via TestWebApplicationFactory.
// Running tests in parallel may cause SQLite lock/transaction conflicts -> flaky 500s.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
