using demo_api.Entities;
using Microsoft.EntityFrameworkCore;

namespace demo_api.Context
{
    public class MeuDbContext : DbContext
    {
        public MeuDbContext(DbContextOptions<MeuDbContext> option) : base(option)
        {}

        public DbSet<Produto> Produtos { get; set; }   
    }
}
