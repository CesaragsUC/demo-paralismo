using demo_api.Context;
using demo_api.Controllers;
using demo_api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace demo_api.Repository;

public class ProdutoRepository : IProdutoRepository
{
    private readonly MeuDbContext _Db;

    private readonly ILogger<ProdutoRepository> _logger;

    public  DbContext context => _Db;


    public ProdutoRepository(MeuDbContext db, ILogger<ProdutoRepository> logger)
    {
        _Db = db;
        _logger = logger;
    }

    public async Task Add(Produto produto)
    {
        _Db.Produtos.Add(produto);
        _Db.SaveChanges();
    }

    public async Task Remove(Produto produto)
    {
        _Db.Produtos.Remove(produto);
        _Db.SaveChanges();
    }


    public async Task Update(Produto produto)
    {
        _Db.Produtos.Update(produto);
        _Db.SaveChanges();
    }

    public void BeginTransaction()
    {
        _Db.Database.BeginTransaction();
    }

    public void Commit()
    {
       _Db.Database.CommitTransaction();
    }

    public void Rollback()
    {
        _Db.Database.RollbackTransaction();
    }
}
