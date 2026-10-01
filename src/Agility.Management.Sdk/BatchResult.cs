using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk;

/// <summary>
/// The outcome of an operation that the API runs as a batch: saves, deletes, and workflow operations
/// such as publish.
/// </summary>
public sealed class BatchResult
{
    internal BatchResult(int batchId, Batch? batch)
    {
        BatchId = batchId;
        Batch = batch;
    }

    /// <summary>The batch ID. Use it with <see cref="Clients.BatchesClient"/> to check the batch later.</summary>
    public int BatchId { get; }

    /// <summary>
    /// The processed batch, or <see langword="null"/> when the call was made with <c>waitForBatch: false</c>.
    /// </summary>
    public Batch? Batch { get; }

    /// <summary>Whether the SDK waited for the batch and it finished.</summary>
    public bool IsProcessed => Batch?.BatchState == BatchState.Processed;

    /// <summary>The batch's items. Empty when the SDK didn't wait.</summary>
    public IReadOnlyList<BatchItem> Items => Batch?.Items ?? [];

    /// <summary>
    /// The IDs of the items the batch processed, in batch order. For a save of a new item, this is the
    /// ID the API assigned.
    /// </summary>
    public IReadOnlyList<int> ItemIds => Items.Select(i => i.ItemID).ToList();

    /// <summary>The first item's ID, or <see langword="null"/> when there are no items.</summary>
    public int? ItemId => Items.Count > 0 ? Items[0].ItemID : null;
}
