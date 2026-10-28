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

/// <summary>Filters and paging for <see cref="SearchClient.SearchAsync"/>.</summary>
public sealed class SearchOptions
{
    internal const int DefaultTake = 25;

    /// <summary>
    /// The kinds of result to return (<see cref="SearchKinds"/>). <see langword="null"/>: every kind you can see.
    /// <see cref="SearchResult.Facets"/> counts every kind you can see whatever this is.
    /// </summary>
    public IReadOnlyList<string>? Kinds { get; set; }

    /// <summary>Only content, components and pages in this locale, e.g. <c>en-us</c>. <see langword="null"/> or <c>*</c>: every locale.</summary>
    public string? Locale { get; set; }

    /// <summary>Only content in these containers, by reference name. Limits the search to content kinds.</summary>
    public IReadOnlyList<string>? Containers { get; set; }

    /// <summary>Only content and components of these models, by reference name. Limits the search to content kinds.</summary>
    public IReadOnlyList<string>? Models { get; set; }

    /// <summary>
    /// Only content in this state: <c>staging</c>, <c>published</c>, <c>unpublished</c> or <c>any</c>. Limits the
    /// search to content kinds.
    /// </summary>
    public string? State { get; set; }

    /// <summary>Only results modified on or after this time.</summary>
    public DateTime? ModifiedAfter { get; set; }

    /// <summary>How many results to skip, for paging. Default: 0.</summary>
    public int? Skip { get; set; }

    /// <summary>Results per page. Default: 25. The API returns at most 100.</summary>
    public int? Take { get; set; }
}

/// <summary>The kinds of result <see cref="SearchClient.SearchAsync"/> returns, for <see cref="SearchOptions.Kinds"/> and <see cref="SearchHit.Kind"/>.</summary>
public static class SearchKinds
{
    /// <summary><c>content</c>: content items.</summary>
    public const string Content = "content";
    /// <summary><c>component</c>: components on pages.</summary>
    public const string Component = "component";
    /// <summary><c>page</c></summary>
    public const string Page = "page";
    /// <summary><c>asset</c></summary>
    public const string Asset = "asset";
    /// <summary><c>model</c>: content and component models.</summary>
    public const string Model = "model";
    /// <summary><c>container</c></summary>
    public const string Container = "container";
}
