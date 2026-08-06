namespace POS_System.Models.Domain
{
    public class Category
    {
        // Keep simple for EF and mapping compatibility but add basic domain behavior
        public string Id { get; private set; }
        public string Name { get; private set; }    

        public Category()
        {
        }

        private Category(string id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static Category Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name must be provided", nameof(name));
            }
            return new Category(Guid.NewGuid().ToString(), name.Trim());
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name must be provided", nameof(name));
            }
            Name = name.Trim();
        }

        public override string ToString() => $"Category: {Id} - {Name}";
    }
}
