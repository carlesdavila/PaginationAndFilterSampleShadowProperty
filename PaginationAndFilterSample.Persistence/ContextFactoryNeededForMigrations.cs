using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PaginationAndFilterSample.Persistence
{
    /// <summary>
    /// This class is needed to allow Add-Migrations command to be run. 
    /// It is not a good implmentation as it has to have a constant connection sting in it
    /// but it is Ok on a local machine, which is where you want to run the command
    /// see https://docs.microsoft.com/en-us/ef/core/miscellaneous/configuring-dbcontext#using-idesigntimedbcontextfactorytcontext
    /// </summary>
    public class ContextFactoryNeededForMigrations : IDesignTimeDbContextFactory<CustomerDbContext>
    {
            public CustomerDbContext CreateDbContext(string[] args)
        {
            return new CustomerDbContext();
        }
    }
}