using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MesaSitec.Infraestructura.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MesaSitecDbContext>
{
    public MesaSitecDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MesaSitecDbContext>()
            .UseSqlite("Data Source=mesasitec.db")
            .Options;

        return new MesaSitecDbContext(options);
    }
}
