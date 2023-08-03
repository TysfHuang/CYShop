using CYShop.Data;
using CYShop.Models;
using CYShop.Controllers;
using CYShop.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Moq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Linq.Expressions;

namespace CYShopTests
{
    [TestFixture]
    public class ProductApiTest
    {
        private Mock<ICYShopRepository<Product, uint>> _repository;
        private ProductApiController _controller;
        private IQueryable<Product> _products;

        [SetUp]
        public void Setup()
        {
            _repository = new Mock<ICYShopRepository<Product, uint>>();
            _controller = new ProductApiController(_repository.Object);
            SetDefaultValue();
        }

        [TestCase((uint)1)]
        [TestCase((uint)2)]
        public void Get_JsonProductDTO_ById(uint id)
        {
            _repository.Setup(p => p.FindById(id)).Returns(_products.Where(p => p.ID == id).Single());
            string resultJson = JsonSerializer.Serialize(new ProductDTO(_products.Where(p => p.ID == id).Single()));

            string? result = _controller.Get(id).Result.Value;

            Assert.That(result, Is.EqualTo(resultJson));
        }

        [Test]
        public void Get_NotFound_ByWrongId()
        {
            _repository.Setup(p => p.FindById(1)).Returns(_products.Where(p => p.ID == 1).Single());

            var result = _controller.Get(2).Result.Result as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public void Get_AllJsonProductDTOs_NoneInput()
        {
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(_products);
            string resultJson = JsonSerializer.Serialize(_products.Select(p => new ProductDTO(p)).ToList());

            string? result = _controller.Get().Result.Value;

            Assert.That(result, Is.EqualTo(resultJson));
        }

        [TestCase("CPU")]
        [TestCase("GPU")]
        [TestCase("SSD")]
        public void Get_JsonProductDTOs_ByCategory(string categoryName)
        {
            var r = _products.Where(p => p.ProductCategory.Name == categoryName);
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(r);
            string resultJson = JsonSerializer.Serialize(_products
                .Where(p => p.ProductCategory.Name == categoryName)
                .Select(p => new ProductDTO(p))
                .ToList());

            var result = _controller.Get(categoryName, "").Result.Value;

            Assert.That(result, Is.EqualTo(resultJson));
        }

        [Test]
        public void Get_NoneProduct_ByNoMatchingCategory()
        {
            string categoryName = "Wrong";
            var r = _products.Where(p => p.ProductCategory.Name == categoryName);
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(r);

            var result = _controller.Get(categoryName, "").Result.Result;

            Assert.That(result, Is.EqualTo(null));
        }

        [TestCase("")]
        [TestCase("core")]
        [TestCase("asus")]
        public void Get_JsonProductDTOs_BySearch(string searchString)
        {
            var r = _products.Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(r);
            string resultJson = JsonSerializer.Serialize(_products
                .Where(p => p.Name.ToLower().Contains(searchString.ToLower()))
                .Select(p => new ProductDTO(p))
                .ToList());

            var result = _controller.Get("All", searchString).Result.Value;

            Assert.That(result, Is.EqualTo(resultJson));
        }

        [TestCase("Wrong")]
        public void Get_NoneProduct_ByNoMatchingSearchString(string searchString)
        {
            var r = _products.Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(r);

            var result = _controller.Get("All", searchString).Result.Result;

            Assert.That(result, Is.EqualTo(null));
        }

        [TestCase("CPU", "core")]
        [TestCase("CPU", "1tb")]
        [TestCase("GPU", "4090")]
        [TestCase("GPU", "core")]
        [TestCase("SSD", "1t")]
        [TestCase("SSD", "3t")]
        [TestCase("SSD", "sand")]
        public void Get_JsonProductDTOs_ByCategoryAndSearch(string categoryName, string searchString)
        {
            var r = _products.Where(
                p => p.ProductCategory.Name == categoryName &&
                p.Name.ToLower().Contains(searchString.ToLower()));
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(r);
            string resultJson = JsonSerializer.Serialize(_products
                .Where(p => p.ProductCategory.Name == categoryName &&
                       p.Name.ToLower().Contains(searchString.ToLower()))
                .Select(p => new ProductDTO(p))
                .ToList());

            var result = _controller.Get(categoryName, searchString).Result.Value;

            Assert.That(result, Is.EqualTo(resultJson));
        }

        [Test]
        public void Get_NoneProduct_ByNoMatchingCategoryAndSearchString()
        {
            string categoryName = "Wrong";
            string searchString = "Wrong";
            var r = _products.Where(
                p => p.ProductCategory.Name == categoryName &&
                p.Name.ToLower().Contains(searchString.ToLower()));
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(r);

            var result = _controller.Get(categoryName, searchString).Result.Result;

            Assert.That(result, Is.EqualTo(null));
        }

        private void SetDefaultValue()
        {
            string t = "Need To Update";
            List<Product> temp = new List<Product>()
            {
                new Product {
                    ID = 1, Name = "Intel Core i9-13900K", Description = t, CoverImagePath = t,
                    Price = 21999, ProductBrandID = 1, ProductCategoryID = 1, ProductCategory = new ProductCategory { ID = 1, Name = "CPU" }
                },
                new Product {
                    ID = 2, Name = "Intel Core i7-13700K", Description = t, CoverImagePath = t,
                    Price = 15000, ProductBrandID = 1, ProductCategoryID = 1, ProductCategory = new ProductCategory { ID = 1, Name = "CPU" }
                },
                new Product {
                    ID = 3, Name = "AMD Ryzen 9-7950X", Description = t, CoverImagePath = t,
                    Price = 23250, ProductBrandID = 2, ProductCategoryID = 1, ProductCategory = new ProductCategory { ID = 1, Name = "CPU" }
                },
                new Product {
                    ID = 4, Name = "AMD Ryzen 7-7700X", Description = t, CoverImagePath = t,
                    Price = 13150, ProductBrandID = 2, ProductCategoryID = 1, ProductCategory = new ProductCategory { ID = 1, Name = "CPU" }
                },
                new Product {
                    ID = 5, Name = "ASUS ROG RTX4090", Description = t, CoverImagePath = t,
                    Price = 63000, ProductBrandID = 3, ProductCategoryID = 2, ProductCategory = new ProductCategory { ID = 2, Name = "GPU" }
                },
                new Product {
                    ID = 6, Name = "ASUS ROG RTX4090", Description = t, CoverImagePath = t,
                    Price = 40000, ProductBrandID = 3, ProductCategoryID = 2, ProductCategory = new ProductCategory { ID = 2, Name = "GPU" }
                },
                new Product {
                    ID = 7, Name = "MSI RTX4090", Description = t, CoverImagePath = t,
                    Price = 62000, ProductBrandID = 4, ProductCategoryID = 2, ProductCategory = new ProductCategory { ID = 2, Name = "GPU" }
                },
                new Product {
                    ID = 8, Name = "MSI RTX4080", Description = t, CoverImagePath = t,
                    Price = 39000, ProductBrandID = 4, ProductCategoryID = 2, ProductCategory = new ProductCategory { ID = 2, Name = "GPU" }
                },
                new Product {
                    ID = 9, Name = "SanDisk SSD Plus 2TB", Description = t, CoverImagePath = t,
                    Price = 4588, ProductBrandID = 5, ProductCategoryID = 3, ProductCategory = new ProductCategory { ID = 3, Name = "SSD" }
                },
                new Product {
                    ID = 10, Name = "SanDisk SSD Plus 1TB", Description = t, CoverImagePath = t,
                    Price = 2588, ProductBrandID = 5, ProductCategoryID = 3, ProductCategory = new ProductCategory { ID = 3, Name = "SSD" }
                },
                new Product {
                    ID = 11, Name = "Micron SSD 2TB", Description = t, CoverImagePath = t,
                    Price = 4488, ProductBrandID = 6, ProductCategoryID = 3, ProductCategory = new ProductCategory { ID = 3, Name = "SSD" }
                },
                new Product {
                    ID = 12, Name = "Micron SSD 1TB", Description = t, CoverImagePath = t,
                    Price = 2488, ProductBrandID = 6, ProductCategoryID = 3, ProductCategory = new ProductCategory { ID = 3, Name = "SSD" }
                },
            };
            _products = temp.AsQueryable();
        }
    }
}
