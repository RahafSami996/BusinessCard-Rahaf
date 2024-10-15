using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;

namespace BusinessCard_Rahaf.Modals
{
    public class DbCardContext:DbContext
    {
        public DbSet<BusinessCardInf> BusinessCards { get; set; }

        public DbCardContext(DbContextOptions<DbCardContext> options)
            : base(options)
        {
        }
    }
}
