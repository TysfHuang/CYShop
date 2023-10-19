using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CYShop.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CYShop.Data
{
    public class CYShopContext : IdentityDbContext<CYShopUser>
    {
        public CYShopContext(DbContextOptions<CYShopContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategorys { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductOrder> ProductOrders { get; set; }
        public DbSet<ProductSalesCount> ProductSalesCounts { get; set; }
        public DbSet<ProductHotSalesList> ProductHotSalesLists { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory");
            modelBuilder.Entity<ProductBrand>().ToTable("ProductBrand");
            modelBuilder.Entity<ProductOrder>().ToTable("ProductOrder");
            modelBuilder.Entity<ProductSalesCount>().ToTable("ProductSalesCount");
            modelBuilder.Entity<ProductHotSalesList>().ToTable("ProductHotSalesList");
        }
    }
}
