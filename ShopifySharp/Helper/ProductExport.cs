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


    public static class ProductExport
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


        
        public static ConcurrentQueue<Product> productQueue = new ConcurrentQueue<Product>();
        public static volatile bool productDone = false;
        /// <summary>
        /// Thread versuib of Get product. Get ll products in Shopify
        /// </summary>
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
            GeneralAdapter.OutputString("GetProductThread done.");
        }
        /// <summary>
        /// Async Version of GetProduct. Get all products in Shopify
        /// </summary>
        public static async Task GetProductAsync()
        {
            int total = await getProductService.CountAsync();
            int pages = (int)Math.Ceiling(total / (decimal)limit);
            ProductFilter pf = new ProductFilter()
            {
                Limit = limit
            };
            for (int i = 1; i <= pages; i++)
            {
                pf.Page = i;
                var result = await getProductService.ListAsync(pf);
                productQueue.AddRange(result);
            }
            productDone = true;
            GeneralAdapter.OutputString("GetProductThread done.");
        }


        public static volatile bool variantDone = false;
        public static List<ProductVariant> variantList;
        /// <summary>
        /// Get Variants from productQueue
        /// </summary>
        public static void GetVariantThread()
        {
            variantList = new List<ProductVariant>();
            int iter = 0;
            while (!productDone || productQueue.Count > 0)
            {

                Product p = null;
                if (productQueue.TryDequeue(out p))
                {

                    if (p != null)
                    {
                        iter++;
                        variantList.AddRange(Task.Run(() => { return getVariantService.ListAsync((long)p.Id); }).Result);
                    }
                }
            }
            variantDone = true;
            GeneralAdapter.OutputString("GetVariantThread Done.");
        }
        /// <summary>
        /// Get Variants from productQueue
        /// </summary>
        public static async Task GetVariantAsync()
        {
            variantList = new List<ProductVariant>();

            while (!productDone || productQueue.Count > 0)
            {
                if(productQueue.Count == 0)
                {
                    await Task.Delay(500);
                }
                Product p = null;
                if (productQueue.TryDequeue(out p))
                {

                    if (p != null)
                    {
                        variantList.AddRange(await getVariantService.ListAsync((long) p.Id));
                    }
                }
            }
            variantDone = true;
            GeneralAdapter.OutputString("GetVariantThread Done.");
        }

    }
}
