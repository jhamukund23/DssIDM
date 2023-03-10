using Dss.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Dss.Infrastructure.Persistence;
public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
    { }
    public DbSet<AddDocument>? adddocument { get; set; }


}
