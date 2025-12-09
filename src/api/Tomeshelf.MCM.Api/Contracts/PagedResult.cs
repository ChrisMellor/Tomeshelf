namespace Tomeshelf.Mcm.Api.Contracts;

/// <summary>
///     Represents a single page of results from a larger collection, including paging metadata and the items for the
///     current page.
/// </summary>
/// <remarks>
///     Use this type to return paginated data along with metadata needed for client-side paging, such as the
///     current page index and total item count. The number of items in <paramref name="Items" /> may be less than
///     <paramref
///         name="PageSize" />
///     on the final page or if the result set is smaller than a full page.
/// </remarks>
/// <typeparam name="T">The type of items contained in the paged result.</typeparam>
/// <param name="Page">
///     The zero-based index of the current page within the overall result set. Must be greater than or
///     equal to 0.
/// </param>
/// <param name="PageSize">The maximum number of items included in each page. Must be greater than 0.</param>
/// <param name="Total">The total number of items available across all pages.</param>
/// <param name="Items">The collection of items contained in the current page. Cannot be null.</param>
public sealed record PagedResult<T>(int Page, int PageSize, int Total, IReadOnlyList<T> Items);