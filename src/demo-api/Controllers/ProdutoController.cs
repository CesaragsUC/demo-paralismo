using Bogus;
using demo_api.Context;
using demo_api.Entities;
using demo_api.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace demo_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly MeuDbContext _db;

        private readonly ILogger<ProdutoController> _logger;
        private readonly IProdutoRepository _produtoRepository;

        //para usar o Parallel precisa ter instancias diferente do context, EF nao permite  usar Parallel na mesma instancia
        //por isso criei otras 2 como demo
        private readonly DbContextOptions<MeuDbContext> _context1;
        private readonly DbContextOptions<MeuDbContext> _context2;

        public ProdutoController(MeuDbContext db,
            DbContextOptions<MeuDbContext> context1,
            DbContextOptions<MeuDbContext> context2,
            ILogger<ProdutoController> logger,
            IProdutoRepository produtoRepository)
        {
            _db = db;
            _context1 = context1;
            _context2 = context2;
            _logger = logger;
            _produtoRepository = produtoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var produtos = _db.Produtos.AsNoTracking().ToList();

            return Ok(produtos);
        }


        [HttpPost("add")]
        public async Task<IActionResult> Create()
        {
            var produto = await CreateProduct();

            try
            {
                await SaveAsync(produto, _produtoRepository.Add);
                return Ok("produto cadastrado com sucesso");
            }
            catch (Exception)
            {
                return BadRequest("houve um erro");
                throw;
            }
        }


        [HttpPost("add-list-parallel")]
        public async Task<IActionResult> CreateByListParallel()
        {
            var produtos = await CreateProductList(10);// 100k

            _logger.LogInformation("cadastrando com Parallel");

            var watch = Stopwatch.StartNew();

            Parallel.ForEach(produtos, produto =>
            {
                using (var context1 = new MeuDbContext(_context1))
                {
                    using (var scope = context1.Database.BeginTransaction())
                    {
                        try
                        {
                            context1.Produtos.Add(produto);
                            context1.SaveChanges();
                            scope.Commit();
                        }
                        catch (Exception)
                        {
                            scope.Rollback();
                            throw;
                        }

                    }
                }
            });

            watch.Stop();

            _logger.LogInformation($"Total com Parallel: {watch.ElapsedMilliseconds}");
            _logger.LogInformation("\n\n");

            return Ok("produtos foram cadastrados com sucesso");
        }


        [HttpPost("add-list-padrao")]
        public async Task<IActionResult> CreateByList()
        {
            var produtos = await CreateProductList(100); // 100k

            _logger.LogInformation("cadastrando sem Parallel");

            var watch = Stopwatch.StartNew();

            await SaveAsync(produtos, _produtoRepository.Add);

            watch.Stop();

            _logger.LogInformation($"Total sem Parallel: {watch.ElapsedMilliseconds}");
            _logger.LogInformation("\n\n");

            return Ok("produtos foram cadastrados com sucesso");
        }


        private async Task<Produto> CreateProduct()
        {
            return new Faker<Produto>("pt_BR")
                .RuleFor(x => x.Name, f => f.Commerce.Product())
                .RuleFor(x => x.Price, f => f.Random.Decimal(200, 1000))
                .RuleFor(x => x.RegisterDate, f => f.Date.Future());

        }

        private async Task<List<Produto>> CreateProductList(int number)
        {
            return new Faker<Produto>("pt_BR")
                .RuleFor(x => x.Name, f => f.Commerce.Product())
                .RuleFor(x => x.Price, f => f.Random.Decimal(200, 1000))
                .RuleFor(x => x.RegisterDate, f => f.Date.Future())
                .Generate(number);

        }

        private async Task<bool> SaveAsync<T>(IEnumerable<T> dataList, Func<T, Task> repository)
        where T : BaseEntity, new()
        {
            var isValid = true;
            foreach (var data in dataList)
            {
                try
                {
                    await repository(data);
                }
                catch (Exception ex)
                {

                    _logger.LogError(
                        exception: ex,
                        message: "Houve um erro ao salvar na tabela {Tabela} com {Id}",
                        typeof(T).Name,
                        data?.Id);

                    isValid = false;
                }
            }
            return isValid;
        }

        private async Task<bool> SaveAsync<T>(T data, Func<T, Task> repository)
        where T : class, new()
        {
            var isValid = false;

            try
            {
                await repository(data);
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    exception: ex,
                    message: "Houve um erro ao salvar na tabela {Tabela}",
                    typeof(T).Name);

                isValid = false;
            }

            return isValid;
        }
    }
}
