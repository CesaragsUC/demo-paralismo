namespace demo_api.Entities
{
    public record Produto : BaseEntity
    {  
        public string Name { get; set; }
        public decimal Price { get; set; }
        
    }

    public abstract record BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public DateTime RegisterDate { get; set; }
    }

}