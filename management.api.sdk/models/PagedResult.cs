namespace agility.models
{
	/// <summary>
	/// A generic class for returning paginated results with total count.
	/// </summary>
	public class PagedResult<T>
	{
		public int TotalCount { get; set; }
		public List<T> Items { get; set; } = new List<T>();
	}
}
