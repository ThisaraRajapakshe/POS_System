namespace POS_System.Models.Domain
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }



        //Navigation Properties

        public Category Category { get; set; }
    }
}
