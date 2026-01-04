using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Api.Infrastructure;

/// <summary>
///     Helpers để chạy một đoạn code trong transaction một cách gọn và tái sử dụng.
///     Mục tiêu:
///     - Tập trung boilerplate Begin/Commit/Rollback vào 1 chỗ.
///     - Cho phép caller quyết định có Commit hay không (ví dụ: validation fail).
///     Lưu ý:
///     - Không dùng ExecutionStrategy retry ở đây để tránh chạy lại các thao tác không idempotent.
///     - Nếu muốn retry cho các thao tác idempotent, có thể tạo overload riêng sau.
/// </summary>
public static class DbContextTransactionExtensions
{
    /// <summary>
    ///     Chạy <paramref name="action" /> trong một DB transaction.
    /// </summary>
    /// <typeparam name="TResult">Kiểu kết quả trả về.</typeparam>
    /// <param name="db">DbContext đang dùng.</param>
    /// <param name="action">
    ///     Delegate trả về (commit, result):
    ///     - commit=true  => Commit transaction
    ///     - commit=false => Rollback transaction
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task<TResult> RunInTransactionAsync<TResult>(
        this DbContext db,
        Func<Task<(bool commit, TResult result)>> action,
        CancellationToken cancellationToken = default)
    {
        // EF Core execution strategy (ví dụ SqlServerRetryingExecutionStrategy khi EnableRetryOnFailure)
        // không hỗ trợ user-initiated transactions trừ khi toàn bộ unit-of-work được chạy bên trong
        // CreateExecutionStrategy().Execute/ExecuteAsync.
        IExecutionStrategy? strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Nếu caller đã có transaction thì không mở thêm transaction (tránh nested tx).
            if (db.Database.CurrentTransaction is not null)
            {
                (_, TResult? existingTxResult) = await action();
                return existingTxResult;
            }

            await using IDbContextTransaction tx = await db.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                (bool commit, TResult? result) = await action();

                if (commit)
                {
                    await tx.CommitAsync(cancellationToken);
                }
                else
                {
                    await tx.RollbackAsync(cancellationToken);
                }

                return result;
            }
            catch
            {
                // Best-effort rollback; đừng che lỗi gốc nếu rollback cũng fail.
                try
                {
                    await tx.RollbackAsync(cancellationToken);
                }
                catch
                {
                    // ignored
                }

                throw;
            }
        });
    }
}
