using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpendSmart_Backend.infrastructure.Data
{
    public class SpendSmartDbContext: DbContext
    {
        public SpendSmartDbContext(DbContextOptions dbContextOptions): base(dbContextOptions)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Category> Categories { get; set; }


    }
}
