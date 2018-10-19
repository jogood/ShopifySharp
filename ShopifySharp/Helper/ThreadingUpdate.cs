using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShopifySharp.Helper
{
    public class ThreadingUpdate
    {
        public static volatile bool AllQueueFilled = false;

        public static InventoryLevelService updateInventoryService;
        public static ProductVariantService updateVariantService;
        /// <summary>
        /// Maximum 250
        /// </summary>
        public static int limit = 250;

        public static void InitServices(string updateInventoryAPIFile = null, string updateVariantAPIfile = null)
        {
            if (updateInventoryAPIFile != null) updateInventoryService = ShopifyAPIConfig.MakeService<InventoryLevelService>(updateInventoryAPIFile);
            if (updateVariantAPIfile != null) updateVariantService = ShopifyAPIConfig.MakeService<ProductVariantService>(updateVariantAPIfile);
        }

        public static ConcurrentQueue<ProductVariant> variantUpdateQueue = new ConcurrentQueue<ProductVariant>();
        public static volatile bool variantUpdateDone = false;
        
        /// <summary>
        /// Update variant (price)
        /// </summary>
        static public void UpdateVariantThread()
        {
            int nulled = 0;
            while (!AllQueueFilled || variantUpdateQueue.Count > 0)
            {


                ProductVariant p = null;
                // SQLDone can become true whithout adding anything to Queue
                while (!variantUpdateQueue.TryDequeue(out p))
                {
                    if (AllQueueFilled && variantUpdateQueue.Count == 0) break;
                }
                if (p != null)
                {
                    if (p.Id == null)
                    {
                        GeneralAdapter.OuputString("Null variant. " + nulled++);
                    }
                    //var result = Task.Run(() => { return updateVariantService.UpdateAsync(p.Id, p); }).Result;
                }

            }
            variantUpdateDone = true;
            GeneralAdapter.OuputString("VariantUpdate Done.");
        }

        static ConcurrentQueue<InventoryLevel> inventoryUpdateQueue = new ConcurrentQueue<InventoryLevel>();
        static volatile bool inventoryUpdateDone = false;

        static public void UpdateInventoryThread()
        {
            int iter = 0;
            int nulled = 0;
            while (!AllQueueFilled || inventoryUpdateQueue.Count > 0)
            {
                iter++;
                InventoryLevel p;
                while (!inventoryUpdateQueue.TryDequeue(out p))
                {
                    if (inventoryUpdateQueue.Count == 0 && AllQueueFilled) break;
                }
                if (p != null)
                {
                    try
                    {
                        if (p.InventoryItemId == null)
                        {
                            GeneralAdapter.OuputString("Null inventory. " + nulled++);
                        }
                        var result = Task.Run(() => { return updateInventoryService.SetAsync(p); }).Result;
                    }
                    catch (Exception e)
                    {
                        GeneralAdapter.OuputString(e.Message + " Not updated inventory. Variantid: " + p.Id + " nulled: " + nulled++);
                    }
                }
            }
            if (inventoryUpdateQueue.Count > 0)
            {
                GeneralAdapter.OuputString("Variant update Queue Wasn't completed.");
                UpdateInventoryThread();
                return;
            }
            inventoryUpdateDone = true;
            GeneralAdapter.OuputString("inventoryUpdate Done. iter: " + iter);
        }

    }
}
