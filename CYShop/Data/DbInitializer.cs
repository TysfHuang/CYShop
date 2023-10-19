using CYShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace CYShop.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new CYShopContext(
                serviceProvider.GetRequiredService<DbContextOptions<CYShopContext>>()))
            {
                var adminID = await EnsureUser(serviceProvider, testUserPw, "admin@cyshop.com");
                await EnsureRole(serviceProvider, adminID, "Administrator");

                SeedDB(context);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                            string testUserPw, string UserName)
        {
            var userManager = serviceProvider.GetService<UserManager<CYShopUser>>();

            var user = await userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                user = new CYShopUser
                {
                    UserName = UserName,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, testUserPw);
            }

            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            IdentityResult IR;
            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<CYShopUser>>();

            var user = await userManager.FindByIdAsync(uid);

            if (user == null)
            {
                throw new Exception("Error in DbInitializer.cs -> EnsureRole. Should create user first!");
            }

            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }

        private static void SeedDB(CYShopContext context)
        {
            if (context.Products.Any())
            {
                return;
            }

            var productCategorys = new ProductCategory[]
                {
                new ProductCategory{Name="CPU"},
                new ProductCategory{Name="GPU"},
                new ProductCategory{Name="SSD"},
                new ProductCategory{Name="RAM"},
                new ProductCategory{Name="MB"},
                new ProductCategory{Name="PSU"}
                };
            context.ProductCategorys.AddRange(productCategorys);
            context.SaveChanges();

            var productBrands = new ProductBrand[]
            {
                new ProductBrand{Name="Intel"},
                new ProductBrand{Name="Amd"},
                new ProductBrand{Name="Asus"},
                new ProductBrand{Name="Msi"},
                new ProductBrand{Name="Sandisk"},
                new ProductBrand{Name="Micron"},
                new ProductBrand{Name="Kingston"},
                new ProductBrand{Name="Crucial"},
                new ProductBrand{Name="Gigabyte"},
                new ProductBrand{Name="Corsair"},
                new ProductBrand{Name="EVGA"},
                new ProductBrand{Name="SeaSonic"}
            };
            context.ProductBrands.AddRange(productBrands);
            context.SaveChanges();

            string des = "need to update";
            string defaultCoverImagePath = "/img/example.jpg";
            var products = new Product[]
            {
                new Product{Name="Intel Core i9-13900K",Description=des,CoverImagePath=defaultCoverImagePath,Price=21999,ProductBrandID=1,ProductCategoryID=1},
                new Product{Name="Intel Core i7-13700K",Description=des,CoverImagePath=defaultCoverImagePath,Price=15000,ProductBrandID=1,ProductCategoryID=1},
                new Product{Name="Intel Core i5-13600K",Description=des,CoverImagePath=defaultCoverImagePath,Price=10600,ProductBrandID=1,ProductCategoryID=1},
                new Product{Name="AMD Ryzen 9-7950X",Description=des,CoverImagePath=defaultCoverImagePath,Price=23250,ProductBrandID=2,ProductCategoryID=1},
                new Product{Name="AMD Ryzen 7-7700X",Description=des,CoverImagePath=defaultCoverImagePath,Price=13150,ProductBrandID=2,ProductCategoryID=1},
                new Product{Name="AMD Ryzen 5-7600X",Description=des,CoverImagePath=defaultCoverImagePath,Price=9950,ProductBrandID=2,ProductCategoryID=1},
                new Product{Name="ASUS ROG RTX4090",Description=des,CoverImagePath=defaultCoverImagePath,Price=63000,ProductBrandID=3,ProductCategoryID=2},
                new Product{Name="ASUS ROG RTX4080",Description=des,CoverImagePath=defaultCoverImagePath,Price=43000,ProductBrandID=3,ProductCategoryID=2},
                new Product{Name="MSI RTX4090",Description=des,CoverImagePath=defaultCoverImagePath,Price=63990,ProductBrandID=4,ProductCategoryID=2},
                new Product{Name="MSI RTX4080",Description=des,CoverImagePath=defaultCoverImagePath,Price=43990,ProductBrandID=4,ProductCategoryID=2},
                new Product{Name="SanDisk SSD Plus 2TB",Description=des,CoverImagePath=defaultCoverImagePath,Price=4588,ProductBrandID=5,ProductCategoryID=3},
                new Product{Name="SanDisk SSD Plus 1TB",Description=des,CoverImagePath=defaultCoverImagePath,Price=2588,ProductBrandID=5,ProductCategoryID=3},
                new Product{Name="Micron SSD 2TB",Description=des,CoverImagePath=defaultCoverImagePath,Price=4488,ProductBrandID=6,ProductCategoryID=3},
                new Product{Name="Micron SSD 1TB",Description=des,CoverImagePath=defaultCoverImagePath,Price=2488,ProductBrandID=6,ProductCategoryID=3},
                new Product{Name="Kingston SSD 2TB",Description=des,CoverImagePath=defaultCoverImagePath,Price=4688,ProductBrandID=7,ProductCategoryID=3},
                new Product{Name="Kingston SSD 1TB",Description=des,CoverImagePath=defaultCoverImagePath,Price=2688,ProductBrandID=7,ProductCategoryID=3},
                new Product{Name="Kingston DDR5 6000 16G",Description=des,CoverImagePath=defaultCoverImagePath,Price=4000,ProductBrandID=7,ProductCategoryID=4},
                new Product{Name="Kingston DDR5 4800 16G",Description=des,CoverImagePath=defaultCoverImagePath,Price=2500,ProductBrandID=7,ProductCategoryID=4},
                new Product{Name="Crucial DDR5 6000 16G",Description=des,CoverImagePath=defaultCoverImagePath,Price=3900,ProductBrandID=8,ProductCategoryID=4},
                new Product{Name="Crucial DDR5 4800 16G",Description=des,CoverImagePath=defaultCoverImagePath,Price=2200,ProductBrandID=8,ProductCategoryID=4},
                new Product{Name="Gigabyte X670",Description=des,CoverImagePath=defaultCoverImagePath,Price=9990,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="Gigabyte B650",Description=des,CoverImagePath=defaultCoverImagePath,Price=10990,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="Gigabyte Z690",Description=des,CoverImagePath=defaultCoverImagePath,Price=6990,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="Gigabyte B660",Description=des,CoverImagePath=defaultCoverImagePath,Price=3399,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="ASUS X670",Description=des,CoverImagePath=defaultCoverImagePath,Price=10090,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="ASUS B650",Description=des,CoverImagePath=defaultCoverImagePath,Price=11490,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="ASUS Z690",Description=des,CoverImagePath=defaultCoverImagePath,Price=7099,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="ASUS B660",Description=des,CoverImagePath=defaultCoverImagePath,Price=3500,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="Corsair RM1000X",Description=des,CoverImagePath=defaultCoverImagePath,Price=6190,ProductBrandID=10,ProductCategoryID=6},
                new Product{Name="Corsair RM850X",Description=des,CoverImagePath=defaultCoverImagePath,Price=4690,ProductBrandID=10,ProductCategoryID=6},
                new Product{Name="EVGA 750 GA",Description=des,CoverImagePath=defaultCoverImagePath,Price=3150,ProductBrandID=11,ProductCategoryID=6},
                new Product{Name="EVGA 650 GA",Description=des,CoverImagePath=defaultCoverImagePath,Price=2200,ProductBrandID=11,ProductCategoryID=6},
                new Product{Name="SeaSonic PRIME TX-850",Description=des,CoverImagePath=defaultCoverImagePath,Price=8150,ProductBrandID=12,ProductCategoryID=6},
                new Product{Name="SeaSonic PRIME TX-750",Description=des,CoverImagePath=defaultCoverImagePath,Price=7090,ProductBrandID=12,ProductCategoryID=6}
            };
            context.Products.AddRange(products);
            context.SaveChanges();

            List<ProductSalesCount> salesList = new List<ProductSalesCount>();
            Random rnd = new Random();
            int days = 32;
            for (int i = 0; i < products.Length; i++)
            {
                byte[] buffer = new byte[days];
                rnd.NextBytes(buffer);
                for (int j = 0; j < buffer.Length; j++) //過去32天每日銷售數量
                {
                    if (Convert.ToInt32(buffer[j]) % 2 == 0)    //並不是每天都有銷售紀錄，因此用隨機數判斷
                    {
                        salesList.Add(new ProductSalesCount
                        {
                            OrderDate = (DateTime.Now - TimeSpan.FromDays(j)).Date,
                            Count = Convert.ToUInt32(rnd.Next(1, 20)),
                            ProductID = Convert.ToUInt32(i + 1)
                        });
                    }
                }
            }
            context.ProductSalesCounts.AddRange(salesList);
            context.SaveChanges();
        }
    }
}
