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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using System.Security.Claims;
using Newtonsoft.Json;

namespace CYShopTests
{
    [TestFixture]
    public class OrderTest
    {
        private Mock<ICYShopRepository<ProductOrder, uint>> _repository;
        private Mock<UserManager<CYShopUser>> _userManager;
        private OrderController _controller;
        private IQueryable<ProductOrder> _productOrders;
        private Mock<HttpContext> _context;
        private Mock<ISession> _session;

        [SetUp]
        public void Setup()
        {
            _repository = new Mock<ICYShopRepository<ProductOrder, uint>>();
            var UserStoreMock = Mock.Of<IUserStore<CYShopUser>>();
            _userManager = new Mock<UserManager<CYShopUser>>(UserStoreMock, null, null, null, null, null, null, null, null);
            _controller = new OrderController(_repository.Object, _userManager.Object);
            _context = new Mock<HttpContext>();
            _session = new Mock<ISession>();
            SetValue();
        }

        [Test]
        public void Index_GetOrders()
        {
            CYShopUser user = GetUser("2");
            _repository.Setup(p => p.Find(It.IsAny<Expression<Func<ProductOrder, bool>>>()))
                .Returns(_productOrders.Where(p => p.UserID == user.Id));
            _userManager.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id);

            var result = _controller.Index(1);

            Assert.That(result, Is.TypeOf<ViewResult>());
            var model = (((ViewResult)result).ViewData.Model) as List<ProductOrderViewModel>;
            Assert.That(model, Has.Exactly(1).Items);
            Assert.That(model[0].TotalPrice, Is.EqualTo(10000));
        }

        [Test]
        public void Checkout_Get_GetCartItems()
        {
            int maxCartItemCount = 3;
            List<CartItem> cartItems = GetOrderItems(maxCartItemCount);
            var sessionValue = JsonConvert.SerializeObject(cartItems);
            byte[] dummy = System.Text.Encoding.UTF8.GetBytes(sessionValue);
            _session.Setup(x => x.TryGetValue(It.IsAny<string>(), out dummy)).Returns(true); //the out dummy does the trick
            _context.Setup(s => s.Session).Returns(_session.Object);
            _userManager.Setup(u => u.GetUserName(It.IsAny<ClaimsPrincipal>())).Returns(GetUser("1").UserName);
            _controller.ControllerContext.HttpContext = _context.Object;

            var result = _controller.Checkout();

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(((ViewResult)result).ViewData, Is.Not.Null);
            var viewData = ((ViewResult)result).ViewData;
            Assert.That(viewData["Cartlist"], Has.Exactly(maxCartItemCount).Items);
            Assert.That(viewData["TotalPrice"], Is.GreaterThan(0));
        }

        [Test]
        public void Checkout_Post_RedirectToAction_WhenAllValid()
        {
            int maxCartItemCount = 3;
            uint orderId = 5;
            CYShopUser user = GetUser("2");
            List<CartItem> cartItems = GetOrderItems(maxCartItemCount);
            var sessionValue = JsonConvert.SerializeObject(cartItems);
            byte[] dummy = System.Text.Encoding.UTF8.GetBytes(sessionValue);
            _session.Setup(x => x.TryGetValue(It.IsAny<string>(), out dummy)).Returns(true); //the out dummy does the trick
            _context.Setup(s => s.Session).Returns(_session.Object);
            _repository.Setup(r => r.CreateAsync(It.IsAny<ProductOrder>())).Returns(Task.FromResult(orderId));
            _userManager.Setup(s => s.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id);
            OrderViewModel input = new OrderViewModel() { ReceiverName = "Re1", ReceiverAddress = "Taiwan", 
                                                          ReceiverPhone = "0988999888", UserName = user.UserName };
            _controller.ControllerContext.HttpContext = _context.Object;

            var result = Task.FromResult(_controller.Checkout(input)).Result.Result;
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Result"));
        }

        [Test]
        public void Checkout_Post_AddErrorMsg_WhenInvalidCart()
        {
            byte[] dummy = null;
            _session.Setup(x => x.TryGetValue(It.IsAny<string>(), out dummy)).Returns(true); //the out dummy does the trick
            _context.Setup(s => s.Session).Returns(_session.Object);
            OrderViewModel input = new OrderViewModel()
            {
                ReceiverName = "Re1",
                ReceiverAddress = "Taiwan",
                ReceiverPhone = "0988999888",
                UserName = "Wrong"
            };
            _controller.ControllerContext.HttpContext = _context.Object;

            var result = Task.FromResult(_controller.Checkout(input)).Result.Result;

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewResult>());
            var modelState = ((ViewResult)result).ViewData.ModelState;
            Assert.That(modelState.IsValid, Is.False);
        }

        [Test]
        public void Checkout_Post_CheckIfInvalid_WhenInvalidModelState()
        {
            int maxCartItemCount = 2;
            List<CartItem> cartItems = GetOrderItems(maxCartItemCount);
            var sessionValue = JsonConvert.SerializeObject(cartItems);
            byte[] dummy = System.Text.Encoding.UTF8.GetBytes(sessionValue);
            _session.Setup(x => x.TryGetValue(It.IsAny<string>(), out dummy)).Returns(true); //the out dummy does the trick
            _context.Setup(s => s.Session).Returns(_session.Object);
            OrderViewModel input = new OrderViewModel()
            {
                ReceiverName = "Re1",
                ReceiverAddress = "Taiwan",
                ReceiverPhone = "0988999888",
                UserName = "Wrong"
            };
            _controller.ControllerContext.HttpContext = _context.Object;
            _controller.ModelState.AddModelError("", "Wrong");

            var result = Task.FromResult(_controller.Checkout(input)).Result.Result;

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<ViewResult>());
            var modelState = ((ViewResult)result).ViewData.ModelState;
            Assert.That(modelState.IsValid, Is.False);
        }

        private CYShopUser GetUser(string id)
        {
            return new CYShopUser() { Id = id, UserName = "test" + id.ToString(), Email = "test@example.com" };
        }

        private List<CartItem> GetOrderItems(int maxCount)
        {
            List<CartItem> cartItems = new List<CartItem>();
            for (uint i = 1; i <= maxCount; i++)
            {
                cartItems.Add(new CartItem
                {
                    ProductID = i,
                    Price = i * 1000,
                    ProductName = "Intel Core I" + i.ToString(),
                    Quantity = i
                });
            }
            return cartItems;
        }

        private void SetValue()
        {
            List<ProductOrder> order = new List<ProductOrder>();
            ProductOrder p1 = new ProductOrder
            {
                ID = 1,
                UserID = "1",
                ReceiverName = "Re1",
                ReceiverAddress = "Taiwen1",
                ReceiverPhone = "0988777555",
                TotalPrice = 3000,
                OrderDate = DateTime.Now
            };
            p1.SetOrderItems(GetOrderItems(2));
            order.Add(p1);

            ProductOrder p2 = new ProductOrder
            {
                ID = 2,
                UserID = "1",
                ReceiverName = "Re2",
                ReceiverAddress = "Taiwen2",
                ReceiverPhone = "0988777666",
                TotalPrice = 6000,
                OrderDate = DateTime.Now
            };
            p2.SetOrderItems(GetOrderItems(3));
            order.Add(p2);

            ProductOrder p3 = new ProductOrder
            {
                ID = 3,
                UserID = "2",
                ReceiverName = "Re3",
                ReceiverAddress = "Taiwen3",
                ReceiverPhone = "0988777777",
                TotalPrice = 10000,
                OrderDate = DateTime.Now
            };
            p3.SetOrderItems(GetOrderItems(4));
            order.Add(p3);

            _productOrders = order.AsQueryable();
        }
    }
}
