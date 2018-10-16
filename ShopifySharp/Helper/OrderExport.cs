using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopifySharp;

namespace ShopifySharp
{


    static public class OrderExport
    {
        public static OrderService orderService;
        public static OrderRiskService orderRiskService;

        static public void initOrderService(string APIfile)
        {
            orderService = ShopifyAPIConfig.MakeService<OrderService>(APIfile); 
        }

        static public void initOrderRiskService(string APIfile)
        {
            orderRiskService = ShopifyAPIConfig.MakeService<OrderRiskService>(APIfile);
        }

        static public void initServices(string APIfile)
        {
            initOrderRiskService(APIfile);
            initOrderService(APIfile);
        }

        /// <summary>
        /// Return orders
        /// </summary>
        /// <param name="APIfile">File containing API information if not already initialized</param>
        /// <param name="SinceID">Orders after this Id</param>
        /// <returns></returns>
        public static List<Order> GetOrders(long SinceID, string APIfile = null)
        {
            if (orderService == null)
            {
                //ShopifyAPIConfig sac = new ShopifyAPIConfig(APIfile);
                //orderService = sac.MakeService<OrderService>();
                initOrderService(APIfile);
            }
            List<Order> listOrder = new List<Order>();
            
            ShopifySharp.Filters.OrderFilter filter = new ShopifySharp.Filters.OrderFilter()
            {
                SinceId = SinceID,
                Limit = 250
            };
            int orderCount = Task.Run(() => { return orderService.CountAsync(filter); }).Result;
            int nPages = (int)Math.Ceiling((double)orderCount / (double)filter.Limit);
            if (orderCount > 0)
            {
                for (int page = 1; page <= nPages; page++)
                {
                    filter.Page = page;
                    IEnumerable<Order> orders = Task.Run(() => { return orderService.ListAsync(filter); }).Result;
                    listOrder.AddRange(orders);
                }
            }
            return listOrder;
        }

        public static List<OrderRisk> GetRisks(long OrderID, string APIfile = null)
        {
            if (orderRiskService == null)
            {
                initOrderRiskService(APIfile);
            }
            List<OrderRisk> riskList = new List<OrderRisk>();
            riskList.AddRange(Task.Run(() => { return orderRiskService.ListAsync(OrderID); }).Result);
            return riskList;
        }

        /// <summary>
        /// Return the risk list of an Order.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="APIfile"></param>
        /// <returns></returns>
        public static List<OrderRisk> GetRisks(this Order order, string APIfile = null)
        {
            return GetRisks((long)order.Id, APIfile);
        }

        

    }
}
