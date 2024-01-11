using KonsiApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KonsiApi.Data
{
    public class DesafioKonsiContext : DbContext
    {
        public DesafioKonsiContext(DbContextOptions<DesafioKonsiContext> options)
        : base(options)
        {
        }

        public DbSet<Cpf> Cpfs { get; set; }

    }
}
