using Agility.Management.Sdk.Models;

namespace Agility.Management.Sdk.Clients;

// Methods take required values as parameters and optional ones as named parameters. The few methods with many
// optional settings take one of these instead, so a call names only what it sets.

/// <summary>Paging, sorting and filtering for <see cref="ContentClient.GetContentListAsync"/>.</summary>
public sealed class ContentListOptions
{
    /// <summary>Filters: states, dates, field values, free text. <see langword="null"/> lists everything.</summary>
    public ContentListFilterModel? Filter { get; set; }

    /// <summary>Items per page. Default: the API's (50).</summary>
    public int? Take { get; set; }

    /// <summary>How many items to skip.</summary>
    public int? Skip { get; set; }

    /// <summary>Include deleted items.</summary>
    public bool? ShowDeleted { get; set; }

    /// <summary>Comma-separated field names to return, to keep responses small.</summary>
    public string? Fields { get; set; }

    /// <summary>The field to sort by.</summary>
    public string? SortField { get; set; }

    /// <summary><c>asc</c> or <c>desc</c>.</summary>
    public string? SortDirection { get; set; }
}

/// <summary>Where to put a page, and how to link it, for <see cref="PagesClient.SavePageAsync"/>.</summary>
public sealed class SavePageOptions
{
    /// <summary>The parent page, for a new page. Omitted: the API's default (the root).</summary>
    public int? ParentPageId { get; set; }

    /// <summary>Put the page before this sibling. Omitted: last.</summary>
    public int? PlaceBeforePageId { get; set; }

    /// <summary>With <see cref="PageIdInOtherLocale"/>: the locale of the page this one translates.</summary>
    public string? OtherLocale { get; set; }

    /// <summary>The ID of the same page in <see cref="OtherLocale"/>.</summary>
    public int? PageIdInOtherLocale { get; set; }

    /// <summary>Link the page's components to existing content instead of copying it.</summary>
    public bool? LinkExistingComponents { get; set; }
}

/// <summary>Paging and filtering for <see cref="ContainersClient.GetContainerListPagedAsync"/>.</summary>
public sealed class ContainerListOptions
{
    /// <summary>Containers per page.</summary>
    public int? PageSize { get; set; }

    /// <summary>How many containers to skip.</summary>
    public int? RecordOffset { get; set; }

    /// <summary>Which kind of container to list.</summary>
    public ContentViewType? ContentType { get; set; }

    /// <summary>Include component (module) containers.</summary>
    public bool? IncludeModules { get; set; }

    /// <summary>Only containers changed after this time.</summary>
    public DateTime? UpdatedSince { get; set; }
}
