using ShopifySharp.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShopifySharp.Helper
{

    static public class QueueExt
    {
        /// <summary>
        /// AddRange for concurentQueue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <param name="enu"></param>
        public static void AddRange<T>(this ConcurrentQueue<T> queue, IEnumerable<T> enu)
        {
            foreach (T obj in enu)
                queue.Enqueue(obj);
        }
    }

    public static class ThreadingGet
    {
        public static ProductService getProductService;
        public static ProductVariantService getVariantService;
        /// <summary>
        /// Maximum 250
        /// </summary>
        public static int limit = 250;

        public static void InitServices(string getProductAPIFile = null, string getVariantsAPIfile = null)
        {
            if(getProductAPIFile != null) getProductService = ShopifyAPIConfig.MakeService<ProductService>(getProductAPIFile);
            if(getVariantsAPIfile != null) getVariantService = ShopifyAPIConfig.MakeService<ProductVariantService>(getVariantsAPIfile);
        }


        
        static ConcurrentQueue<Product> productQueue = new ConcurrentQueue<Product>();
        static volatile bool productDone = false;
        public static void GetProductThread()
        {
            int total = Task.Run(() => { return getProductService.CountAsync(); }).Result;
            int pages = (int)Math.Ceiling(total / (decimal)limit);
            ProductFilter pf = new ProductFilter()
            {
                Limit = limit
            };
            for (int i = 1; i <= pages; i++)
            {
                pf.Page = i;
                productQueue.AddRange(Task.Run(() => { return getProductService.ListAsync(pf); }).Result);
            }
            productDone = true;
        }

    }
}
