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
using Microsoft.Extensions.Options;
using MockQueryable.Moq;

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
            _repository.Setup(p => p.FindByIdAsync(id)).Returns(Task.FromResult(_products.Where(p => p.ID == id).Single()));
            string expectValue = JsonSerializer.Serialize(new ProductDTO(_products.Where(p => p.ID == id).Single()));

            var result = ((OkObjectResult)_controller.Get(id).Result.Result).Value;
            string resultJson = JsonSerializer.Serialize(result);

            Assert.That(resultJson, Is.EqualTo(expectValue));
        }

        [Test]
        public void Get_NotFound_ByWrongId()
        {
            _repository.Setup(p => p.FindByIdAsync(1)).Returns(Task.FromResult(_products.Where(p => p.ID == 1).Single()));

            var result = _controller.Get(2).Result.Result as NotFoundResult;

            Assert.IsNotNull(result);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public void Get_AllJsonProductDTOs_NoneInput()
        {
            var testDataList = GetTestDataList().Select(p => p);
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);
            int maxPageNum = _products.Count();
            List<ProductDTO> tempList = _products
                .Take(ProductApiController.GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToList();
            var obj = new
            {
                data = tempList,
                maxPageNum = (int)Math.Ceiling(maxPageNum / (double)ProductApiController.GetPageSize())
            };
            var expectValue = JsonSerializer.Serialize(obj);

            var result = ((OkObjectResult)_controller.Get().Result.Result).Value;
            string resultJson = JsonSerializer.Serialize(result);

            Assert.That(resultJson, Is.EqualTo(expectValue));
        }

        [TestCase("CPU")]
        [TestCase("GPU")]
        [TestCase("SSD")]
        public void Get_JsonProductDTOs_ByCategory(string categoryName)
        {
            var testDataList = GetTestDataList().Where(p => p.ProductCategory.Name == categoryName);
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);
            var products = _products
                .Where(p => p.ProductCategory.Name == categoryName);
            int maxPageNum = products.Count();
            List<ProductDTO> tempList = products
                .Take(ProductApiController.GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToList();
            var obj = new
            {
                data = tempList,
                maxPageNum = (int)Math.Ceiling(maxPageNum / (double)ProductApiController.GetPageSize())
            };
            var expectValue = JsonSerializer.Serialize(obj);

            var result = ((OkObjectResult)_controller.Get(categoryName, "", null, null).Result.Result).Value;
            string resultJson = JsonSerializer.Serialize(result);

            Assert.That(resultJson, Is.EqualTo(expectValue));
        }

        [Test]
        public void Get_NoneProduct_ByNoMatchingCategory()
        {
            string categoryName = "Wrong";
            var testDataList = GetTestDataList().Where(p => p.ProductCategory.Name == categoryName);
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);

            var result = _controller.Get(categoryName, "", null, null).Result.Result;

            Assert.That(result, Is.TypeOf(typeof(NotFoundResult)));
        }

        [TestCase("")]
        [TestCase("core")]
        [TestCase("asus")]
        public void Get_JsonProductDTOs_ByStringSearch(string searchString)
        {
            var testDataList = GetTestDataList().Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);
            var products = _products
                .Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            int maxPageNum = products.Count();
            List<ProductDTO> tempList = products
                .Take(ProductApiController.GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToList();
            var obj = new
            {
                data = tempList,
                maxPageNum = (int)Math.Ceiling(maxPageNum / (double)ProductApiController.GetPageSize())
            };
            var expectValue = JsonSerializer.Serialize(obj);

            var result = ((OkObjectResult)_controller.Get("ALL", searchString, null, null).Result.Result).Value;
            string resultJson = JsonSerializer.Serialize(result);

            Assert.That(resultJson, Is.EqualTo(expectValue));
        }

        /// <summary>
        /// 要確保測試用的資料數量大於_controller.GetPageSize()
        /// </summary>
        [Test]
        public void Get_JsonProductDTOs_OnPageTwo()
        {
            var testDataList = GetTestDataList().Where(p => true);
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);
            var products = _products.Where(p => true);
            int maxPageNum = products.Count();
            List<ProductDTO> tempList = products
                .Skip(ProductApiController.GetPageSize())
                .Take(ProductApiController.GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToList();
            var obj = new
            {
                data = tempList,
                maxPageNum = (int)Math.Ceiling(maxPageNum / (double)ProductApiController.GetPageSize())
            };
            var expectValue = JsonSerializer.Serialize(obj);

            var result = ((OkObjectResult)_controller.Get("ALL", "", null, 2).Result.Result).Value;
            string resultJson = JsonSerializer.Serialize(result);

            Assert.That(resultJson, Is.EqualTo(expectValue));
        }

        [TestCase("priceHighToLow")]
        [TestCase("priceLowToHigh")]
        public void Get_JsonProductDTOs_SortedByPrice(string currentSortOrder)
        {
            var testDataList = GetTestDataList().Select(p => p);
            var products = _products;
            switch (currentSortOrder)
            {
                case "priceHighToLow":
                    testDataList = testDataList.OrderByDescending(p => p.Price);
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "priceLowToHigh":
                    testDataList = testDataList.OrderBy(p => p.Price);
                    products = products.OrderBy(p => p.Price);
                    break;
            }
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);
            int maxPageNum = products.Count();
            List<ProductDTO> tempList = products
                .Take(ProductApiController.GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToList();
            var obj = new
            {
                data = tempList,
                maxPageNum = (int)Math.Ceiling(maxPageNum / (double)ProductApiController.GetPageSize())
            };
            var expectValue = JsonSerializer.Serialize(obj);

            var result = ((OkObjectResult)_controller.Get("ALL", "", null, null).Result.Result).Value;
            string resultJson = JsonSerializer.Serialize(result);

            Assert.That(resultJson, Is.EqualTo(expectValue));
        }

        [TestCase("Wrong")]
        public void Get_NoneProduct_ByNoMatchingSearchString(string searchString)
        {
            var testDataList = GetTestDataList().Where(p => p.Name.ToLower().Contains(searchString.ToLower()));
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);

            var result = _controller.Get("All", searchString, null, null).Result.Result;

            Assert.That(result, Is.TypeOf(typeof(NotFoundResult)));
        }

        [TestCase("CPU", "core")]
        [TestCase("GPU", "4090")]
        [TestCase("SSD", "1t")]
        [TestCase("SSD", "sand")]
        public void Get_JsonProductDTOs_ByCategoryAndSearch(string categoryName, string searchString)
        {
            var testDataList = GetTestDataList().Where(
                p => p.ProductCategory.Name == categoryName &&
                p.Name.ToLower().Contains(searchString.ToLower())); ;
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);
            var products = _products
                .Where(p => p.ProductCategory.Name == categoryName &&
                       p.Name.ToLower().Contains(searchString.ToLower()));
            int maxPageNum = products.Count();
            List<ProductDTO> tempList = products
                .Take(ProductApiController.GetPageSize())
                .Select(p => new ProductDTO(p))
                .ToList();
            var obj = new
            {
                data = tempList,
                maxPageNum = (int)Math.Ceiling(maxPageNum / (double)ProductApiController.GetPageSize())
            };
            var expectValue = JsonSerializer.Serialize(obj);

            var result = ((OkObjectResult)_controller.Get(categoryName, searchString, null, null).Result.Result).Value;
            string resultJson = JsonSerializer.Serialize(result);

            Assert.That(resultJson, Is.EqualTo(expectValue));
        }

        [TestCase("CPU", "1tb")]
        [TestCase("GPU", "core")]
        [TestCase("SSD", "3t")]
        public void Get_NotFoundResult_WhenNoMatchingCondition(string categoryName, string searchString)
        {
            var testDataList = GetTestDataList().Where(
                p => p.ProductCategory.Name == categoryName &&
                p.Name.ToLower().Contains(searchString.ToLower()));
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);

            var result = _controller.Get(categoryName, searchString, null, null).Result.Result;

            Assert.That(result, Is.TypeOf(typeof(NotFoundResult)));
        }

        [Test]
        public void Get_NoneProduct_ByNoMatchingCategoryAndSearchString()
        {
            string categoryName = "Wrong";
            string searchString = "Wrong";
            var testDataList = GetTestDataList().Where(
                p => p.ProductCategory.Name == categoryName &&
                p.Name.ToLower().Contains(searchString.ToLower()));
            var dataMock = testDataList.BuildMock();
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<Product, bool>>>())).Returns(dataMock);

            var result = _controller.Get(categoryName, searchString, null, null).Result.Result;

            Assert.That(result, Is.TypeOf(typeof(NotFoundResult)));
        }

        private void SetDefaultValue()
        {
            string t = "Need To Update";
            List<Product> temp = GetTestDataList();
            _products = temp.AsQueryable();
        }

        private List<Product> GetTestDataList()
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
            return temp;
        }
    }
}
