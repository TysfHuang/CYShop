using CYShop.Models;
using CYShop.Repositories;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CYShop.Helper
{
    public interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }

    public class ScopedProcessingService : IScopedProcessingService
    {
        private readonly ILogger _logger;
        private readonly ICYShopRepository<ProductSalesCount, uint> _repository_sales;
        private readonly ICYShopRepository<ProductHotSalesList, uint> _repository_list;
        private const int MaxNumOfItemsInList = 30;
        private struct TmpData
        {
            public uint ProductID;
            public uint SalesCount;
        }

        public ScopedProcessingService(ILogger<ScopedProcessingService> logger,
            ICYShopRepository<ProductSalesCount, uint> repository_sales,
            ICYShopRepository<ProductHotSalesList, uint> repository_list)
        {
            _logger = logger;
            _repository_sales = repository_sales;
            _repository_list = repository_list;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            await WorkForUpdate();
            /*while (!stoppingToken.IsCancellationRequested)
            {
                executionCount++;

                _logger.LogInformation(
                    "Scoped Processing Service is working. Count: {Count}", executionCount);

                await Task.Delay(10000, stoppingToken);
            }
            */
        }

        private async Task WorkForUpdate()
        {
            await UpdateHotSalesList(ProductHotSalesListPeriodType.Week);
        }

        private async Task UpdateHotSalesList(ProductHotSalesListPeriodType period)
        {
            int days = 7;
            switch (period)
            {
                case ProductHotSalesListPeriodType.Week: days = 7; break;
            }
            DateTime specDate = (DateTime.Now - TimeSpan.FromDays(Convert.ToDouble(days))).Date;
            Expression<Func<ProductSalesCount, bool>> query = p => p.OrderDate >= specDate;
            var salesList = await _repository_sales
                .Find(query)
                .Include(s => s.Product)
                .AsNoTracking()
                .ToListAsync();
            var productIdList = salesList.Select(s => s.ProductID).Distinct();
            List<TmpData> totalSalesCountsList = new List<TmpData>();
            foreach (var productId in productIdList)    //計算每個產品的總銷售數
            {
                uint totalCount = Convert.ToUInt32(salesList.Where(s => s.ProductID == productId).Sum(s => s.Count));
                Product product = salesList.Where(s => s.ProductID == productId).First().Product;
                totalSalesCountsList.Add(new TmpData
                {
                    ProductID = product.ID,
                    SalesCount = totalCount
                });
            }
            totalSalesCountsList.Sort((x, y) => x.SalesCount.CompareTo(y.SalesCount) * -1);

            string convertedList = "";
            for (int i = 0; i < totalSalesCountsList.Count && i < MaxNumOfItemsInList; i++)
            {
                convertedList += totalSalesCountsList[i].ProductID.ToString() +
                    "," +
                    totalSalesCountsList[i].SalesCount.ToString() +
                    ".";
            }
            await _repository_list.CreateAsync(new ProductHotSalesList
            {
                Itemslist = convertedList,
                RecordDate = DateTime.Now,
                Period = period
            });
        }
    }

    public class HotSalesUpdateScheduler : BackgroundService
    {
        private readonly ILogger<HotSalesUpdateScheduler> _logger;
        private IServiceProvider _services { get; }
        private int _executionCount = 0;

        public HotSalesUpdateScheduler(
            IServiceProvider services,
            ILogger<HotSalesUpdateScheduler> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service for update hot sales list is running.");

            var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
            try
            {
                do
                {
                    using (var scope = _services.CreateScope())
                    {
                        var myService = scope.ServiceProvider.GetRequiredService<IScopedProcessingService>();
                        await myService.DoWork(stoppingToken);
                        int count = Interlocked.Increment(ref _executionCount);
                        _logger.LogInformation("Timed Hosted Service for update hot sales list is working. Count: {Count}", count);
                    }
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                } while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Timed Hosted Service for update hot sales list is stopping.");
            }
        }
    }
}
