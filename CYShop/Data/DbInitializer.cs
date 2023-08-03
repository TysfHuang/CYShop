using CYShop.Models;

namespace CYShop.Data
{
    public class DbInitializer
    {
        public static void Initialize(CYShopContext context)
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

            string t = "need to update";
            var products = new Product[]
            {
                new Product{Name="Intel Core i9-13900K",Description=t,CoverImagePath=t,Price=21999,ProductBrandID=1,ProductCategoryID=1},
                new Product{Name="Intel Core i7-13700K",Description=t,CoverImagePath=t,Price=15000,ProductBrandID=1,ProductCategoryID=1},
                new Product{Name="Intel Core i5-13600K",Description=t,CoverImagePath=t,Price=10600,ProductBrandID=1,ProductCategoryID=1},
                new Product{Name="AMD Ryzen 9-7950X",Description=t,CoverImagePath=t,Price=23250,ProductBrandID=2,ProductCategoryID=1},
                new Product{Name="AMD Ryzen 7-7700X",Description=t,CoverImagePath=t,Price=13150,ProductBrandID=2,ProductCategoryID=1},
                new Product{Name="AMD Ryzen 5-7600X",Description=t,CoverImagePath=t,Price=9950,ProductBrandID=2,ProductCategoryID=1},
                new Product{Name="ASUS ROG RTX4090",Description=t,CoverImagePath=t,Price=63000,ProductBrandID=3,ProductCategoryID=2},
                new Product{Name="ASUS ROG RTX4080",Description=t,CoverImagePath=t,Price=43000,ProductBrandID=3,ProductCategoryID=2},
                new Product{Name="MSI RTX4090",Description=t,CoverImagePath=t,Price=63990,ProductBrandID=4,ProductCategoryID=2},
                new Product{Name="MSI RTX4080",Description=t,CoverImagePath=t,Price=43990,ProductBrandID=4,ProductCategoryID=2},
                new Product{Name="SanDisk SSD Plus 2TB",Description=t,CoverImagePath=t,Price=4588,ProductBrandID=5,ProductCategoryID=3},
                new Product{Name="SanDisk SSD Plus 1TB",Description=t,CoverImagePath=t,Price=2588,ProductBrandID=5,ProductCategoryID=3},
                new Product{Name="Micron SSD 2TB",Description=t,CoverImagePath=t,Price=4488,ProductBrandID=6,ProductCategoryID=3},
                new Product{Name="Micron SSD 1TB",Description=t,CoverImagePath=t,Price=2488,ProductBrandID=6,ProductCategoryID=3},
                new Product{Name="Kingston SSD 2TB",Description=t,CoverImagePath=t,Price=4688,ProductBrandID=7,ProductCategoryID=3},
                new Product{Name="Kingston SSD 1TB",Description=t,CoverImagePath=t,Price=2688,ProductBrandID=7,ProductCategoryID=3},
                new Product{Name="Kingston DDR5 6000 16G",Description=t,CoverImagePath=t,Price=4000,ProductBrandID=7,ProductCategoryID=4},
                new Product{Name="Kingston DDR5 4800 16G",Description=t,CoverImagePath=t,Price=2500,ProductBrandID=7,ProductCategoryID=4},
                new Product{Name="Crucial DDR5 6000 16G",Description=t,CoverImagePath=t,Price=3900,ProductBrandID=8,ProductCategoryID=4},
                new Product{Name="Crucial DDR5 4800 16G",Description=t,CoverImagePath=t,Price=2200,ProductBrandID=8,ProductCategoryID=4},
                new Product{Name="Gigabyte X670",Description=t,CoverImagePath=t,Price=9990,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="Gigabyte B650",Description=t,CoverImagePath=t,Price=10990,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="Gigabyte Z690",Description=t,CoverImagePath=t,Price=6990,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="Gigabyte B660",Description=t,CoverImagePath=t,Price=3399,ProductBrandID=9,ProductCategoryID=5},
                new Product{Name="ASUS X670",Description=t,CoverImagePath=t,Price=10090,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="ASUS B650",Description=t,CoverImagePath=t,Price=11490,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="ASUS Z690",Description=t,CoverImagePath=t,Price=7099,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="ASUS B660",Description=t,CoverImagePath=t,Price=3500,ProductBrandID=3,ProductCategoryID=5},
                new Product{Name="Corsair RM1000X",Description=t,CoverImagePath=t,Price=6190,ProductBrandID=10,ProductCategoryID=6},
                new Product{Name="Corsair RM850X",Description=t,CoverImagePath=t,Price=4690,ProductBrandID=10,ProductCategoryID=6},
                new Product{Name="EVGA 750 GA",Description=t,CoverImagePath=t,Price=3150,ProductBrandID=11,ProductCategoryID=6},
                new Product{Name="EVGA 650 GA",Description=t,CoverImagePath=t,Price=2200,ProductBrandID=11,ProductCategoryID=6},
                new Product{Name="SeaSonic PRIME TX-850",Description=t,CoverImagePath=t,Price=8150,ProductBrandID=12,ProductCategoryID=6},
                new Product{Name="SeaSonic PRIME TX-750",Description=t,CoverImagePath=t,Price=7090,ProductBrandID=12,ProductCategoryID=6}
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
