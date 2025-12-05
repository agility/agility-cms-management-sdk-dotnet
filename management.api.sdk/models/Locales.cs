namespace agility.models
{
    /// <summary>
    /// Represents a collection of locale codes for an Agility CMS instance.
    /// </summary>
    public class Locales : List<string>
    {
        public Locales() : base()
        {
        }

        public Locales(IEnumerable<string> items) : base(items)
        {
        }
    }
}
