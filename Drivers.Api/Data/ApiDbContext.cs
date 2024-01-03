using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Drivers.Api.Data
{
    public class ApiDBContext : IdentityDbContext
    {
        public ApiDBContext(DbContextOptions<ApiDBContext> options) : base(options)
        {

        }
    }
}
