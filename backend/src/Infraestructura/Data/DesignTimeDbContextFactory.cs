using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HelpDesk.Infraestructura.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<HelpDeskDbContext>
{
    public HelpDeskDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HelpDeskDbContext>()
            .UseSqlite("Data Source=helpdesk.db")
            .Options;

        return new HelpDeskDbContext(options);
    }
}
