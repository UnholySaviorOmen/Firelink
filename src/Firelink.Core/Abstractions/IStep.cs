namespace Firelink.Core.Abstractions;

public interface IStep<in TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct);
}