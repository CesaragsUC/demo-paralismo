using demo_api.Context;
using demo_api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace demo_api.Repository;

public interface IProdutoRepository
{
    void BeginTransaction();
    void Commit();
    void Rollback();
    Task Add(Produto produto);
    Task Remove(Produto produto);
    Task Update(Produto produto);
}
