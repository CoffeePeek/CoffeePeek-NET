namespace CoffeePeek.JobVacancies.Utils;

/// <summary>
/// Provides an asynchronous locking mechanism to ensure that only one asynchronous operation
/// accesses a critical section at a time. The lock is reentrant for separate tasks but requires
/// proper disposal to release the lock.
/// </summary>
public sealed class AsyncLock
{
    /// <summary>
    /// A private instance of <see cref="SemaphoreSlim"/> used to control asynchronous access
    /// to a resource in a safe, thread-constrained manner. Initialized with a maximum of 1
    /// concurrent access, ensuring mutual exclusion.
    /// </summary>
    private readonly SemaphoreSlim _mutex = new(1, 1);

    /// Acquires an asynchronous lock, ensuring that only one thread or task can access the protected code at a time.
    /// The lock is released automatically when the returned IDisposable is disposed. This method is particularly
    /// helpful for scenarios requiring synchronization in asynchronous code.
    /// <param name="ct">
    /// An optional cancellation token that can be used to cancel the asynchronous wait for the lock.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, which resolves to an IDisposable. Disposing of this object
    /// releases the lock.
    /// </returns>
    public async Task<IDisposable> LockAsync(CancellationToken ct = default)
    {
        await _mutex.WaitAsync(ct);
        return new Releaser(_mutex);
    }

    /// Executes the specified asynchronous code within a secure lock, ensuring that the provided
    /// operation runs exclusively without interference from other concurrent operations.
    /// This method acquires a lock using an asynchronous mechanism to ensure safe access to shared
    /// resources. Once the operation is completed, the lock is released.
    /// <param name="codeToRun">The asynchronous function to execute. This function contains the logic
    /// that should run exclusively within the acquired lock.</param>
    /// <param name="ct">The cancellation token to monitor for cancellation requests. Defaults to an empty cancellation token.</param>
    /// <returns>A task that represents the completion of the locked execution. The task will complete
    /// when the provided asynchronous function finishes execution.</returns>
    public async Task RunLocked(Func<Task> codeToRun, CancellationToken ct = default)
    {
        using (await LockAsync(ct))
        {
            await codeToRun();
        }
    }

    /// <summary>
    /// Represents a releaser for a semaphore offering a mechanism
    /// for automatically releasing the ownership of the lock when disposed.
    /// </summary>
    /// <remarks>
    /// This class is an internal part of the <see cref="AsyncLock"/> implementation.
    /// It is intended to be used implicitly by acquiring the lock via <see cref="AsyncLock.LockAsync(CancellationToken)"/>
    /// and disposing of it after the operation completes.
    /// </remarks>
    private class Releaser : IDisposable
    {
        /// <summary>
        /// SemaphoreSlim instance used to manage the asynchronous locking mechanism
        /// within the AsyncLock class. It ensures that only one task can access a
        /// protected resource at a time, providing thread-safe execution of critical sections.
        /// </summary>
        private readonly SemaphoreSlim _mutex;

        /// <summary>
        /// Represents a utility class that ensures proper releasing of a semaphore
        /// lock used within the <see cref="AsyncLock"/> class.
        /// </summary>
        public Releaser(SemaphoreSlim mutex)
        {
            _mutex = mutex;
        }

        /// <summary>
        /// Releases the semaphore that was previously acquired by this instance of the Releaser.
        /// </summary>
        /// <remarks>
        /// This method is responsible for unlocking the semaphore, allowing other tasks
        /// to acquire the semaphore and proceed. It should always be called after the
        /// critical section has been executed to ensure proper synchronization.
        /// </remarks>
        public void Dispose()
        {
            _mutex.Release();
        }
    }
}