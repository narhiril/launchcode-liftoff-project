﻿using Microsoft.EntityFrameworkCore;
using HouseholdManager.Models;

namespace HouseholdManager.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options):base(options)
        {
                
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<HouseholdManager.Models.User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);
            mb.Entity("HouseholdManager.Models.Room", b => {
                b.Property<string>("Icon").IsUnicode(true);
            });
        }

    }
}
