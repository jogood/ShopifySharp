using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShopifySharp.Helper
{
    public class ProductUpdate
    {
        /// <summary>
        /// When SQL is done
        /// </summary>
        public static volatile bool AllGetQueueFilled = false;

        public static InventoryLevelService updateInventoryService;
        public static ProductVariantService updateVariantService;



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
            while (!AllGetQueueFilled || variantUpdateQueue.Count > 0)
            {


                ProductVariant p = null;
                // SQLDone can become true whithout adding anything to Queue
                if (variantUpdateQueue.TryDequeue(out p))
                {

                    try
                    {
                        if (p.Id == null)
                        {
                            NotFoundIdVariantUpdateQueue.Enqueue(p);
                        }
                        else
                        {
                            //var result = Task.Run(() => { return updateVariantService.UpdateAsync((long)p.Id, p); }).Result;
                        }
                    }
                    catch
                    {
                        GeneralAdapter.OutputString($"This product was sent to check its Id {p.SKU}");
                        NotFoundIdVariantUpdateQueue.Enqueue(p);
                    }
                }

            }
            variantUpdateDone = true;
            GeneralAdapter.OutputString("VariantUpdate Done.");
        }

        /// <summary>
        /// Update variant (price)
        /// </summary>
        static public async Task UpdateVariantAsync()
        {
            while (!AllGetQueueFilled || variantUpdateQueue.Count > 0)
            {

                if (variantUpdateQueue.Count == 0) await Task.Delay(500);
                
                ProductVariant p = null;
                // SQLDone can become true whithout adding anything to Queue
                if (variantUpdateQueue.TryDequeue(out p))
                {

                    try
                    {
                        if (p.Id == null)
                        {
                            NotFoundIdVariantUpdateQueue.Enqueue(p);
                        }
                        else
                        {
                            //var result = await updateVariantService.UpdateAsync((long)p.Id, p));
                        }
                    }
                    catch
                    {
                        GeneralAdapter.OutputString($"This product was sent to check its Id {p.SKU}");
                        NotFoundIdVariantUpdateQueue.Enqueue(p);
                    }
                }

            }
            variantUpdateDone = true;
            GeneralAdapter.OutputString("VariantUpdate Done.");
        }

        public static ConcurrentQueue<InventoryLevel> inventoryUpdateQueue = new ConcurrentQueue<InventoryLevel>();
        public static volatile bool inventoryUpdateDone = false;
        

        static public void UpdateInventoryThread()
        {
            while (!AllGetQueueFilled || inventoryUpdateQueue.Count > 0)
            {
                InventoryLevel p;
                if (inventoryUpdateQueue.TryDequeue(out p))
                {
                    try
                    {
                        if (p.InventoryItemId == null)
                        {
                            NotFoundIdInventoryUpdateQueue.Enqueue(p);
                        }
                        else
                        {
                            var result = Task.Run(() => { return updateInventoryService.SetAsync(p); }).Result;
                        }
                    }
                    catch
                    {
                        GeneralAdapter.OutputString($"This product was sent to check its Id {p.SKU}");
                        NotFoundIdInventoryUpdateQueue.Enqueue(p);
                    }
                }

            }
            inventoryUpdateDone = true;
            GeneralAdapter.OutputString("inventoryUpdate Done.");
        }
        static public async Task UpdateInventoryAsync()
        {
            while (!AllGetQueueFilled || inventoryUpdateQueue.Count > 0)
            {
                if (inventoryUpdateQueue.Count == 0) await Task.Delay(500);
                InventoryLevel p;
                if (inventoryUpdateQueue.TryDequeue(out p))
                {
                    try
                    {
                        if (p.InventoryItemId == null)
                        {
                            NotFoundIdInventoryUpdateQueue.Enqueue(p);
                        }
                        else
                        {
                            var result = await updateInventoryService.SetAsync(p);
                        }
                    }
                    catch
                    {
                        GeneralAdapter.OutputString($"This product was sent to check its Id {p.SKU}");
                        NotFoundIdInventoryUpdateQueue.Enqueue(p);
                    }
                }

            }
            inventoryUpdateDone = true;
            GeneralAdapter.OutputString("inventoryUpdate Done.");
        }

        public static ConcurrentQueue<ProductVariant> NotFoundIdVariantUpdateQueue = new ConcurrentQueue<ProductVariant>();
        public static volatile bool checkIdVariantDone = false;

        /// <summary>
        /// Update variant (price) and check Ids (update them in SAP). Use checkIdInventoryUpdateQueue, should check <see cref=ProductExport.variantDone></cref> before running
        /// </summary>
        static public void CheckIdUpdateVariantThread()
        {
            while (!variantUpdateDone || NotFoundIdVariantUpdateQueue.Count > 0)
            {
                ProductVariant p = null;
                // SQLDone can become true whithout adding anything to Queue
                if (NotFoundIdVariantUpdateQueue.TryDequeue(out p))
                {
                    try
                    {
                        if (GeneralAdapter.FindIdFromSku(p))
                        {
                            //TODO DI API
                            //var result = Task.Run(() => { return updateVariantService.UpdateAsync((long)p.Id, p); }).Result;
                        }
                        else
                        {
                            GeneralAdapter.OutputString($"Couldn't find the SKU: {p.SKU}");
                        }
                    }
                    catch
                    {

                        GeneralAdapter.OutputErrorString($"Couldn't resolve the variant {p.Id} SKU: {p.SKU}");
                    }
                }

            }
            checkIdVariantDone = true;
            GeneralAdapter.OutputString("checkIdVariantDone Done.");
        }

        static public async Task NotFoundIdUpdateVariantAsync()
        {
            while (!variantUpdateDone || NotFoundIdVariantUpdateQueue.Count > 0)
            {
                if (NotFoundIdVariantUpdateQueue.Count == 0) await Task.Delay(500);
                ProductVariant p = null;
                // SQLDone can become true whithout adding anything to Queue
                if (NotFoundIdVariantUpdateQueue.TryDequeue(out p))
                {
                    try
                    {
                        if (GeneralAdapter.FindIdFromSku(p))
                        {
                            //TODO DI API
                            //await updateVariantService.UpdateAsync((long)p.Id, p);
                        }
                        else
                        {
                            GeneralAdapter.OutputString($"Couldn't find the SKU: {p.SKU}");
                        }
                    }
                    catch
                    {

                        GeneralAdapter.OutputErrorString($"Couldn't resolve the variant {p.Id} SKU: {p.SKU}");
                    }
                }

            }
            checkIdVariantDone = true;
            GeneralAdapter.OutputString("checkIdVariantDone Done.");
        }



        public static ConcurrentQueue<InventoryLevel> NotFoundIdInventoryUpdateQueue = new ConcurrentQueue<InventoryLevel>();
        public static volatile bool checkIdInventoryDone = false;

        /// <summary>
        /// Update variant (price) and check Ids (update them in SAP). Use checkIdInventoryUpdateQueue, should check <see cref=ProductExport.variantDone></cref> before running
        /// </summary>
        static public void CheckIdInventoryUpdateThread()
        {
            while (!inventoryUpdateDone || NotFoundIdInventoryUpdateQueue.Count > 0)
            {
                InventoryLevel p = null;
                // SQLDone can become true whithout adding anything to Queue
                if (NotFoundIdInventoryUpdateQueue.TryDequeue(out p))
                {

                    try
                    {
                        if (GeneralAdapter.FindIdFromSku(p))
                        {
                            var result = Task.Run(() => { return updateInventoryService.SetAsync(p); }).Result;
                        }
                        else
                        {
                            GeneralAdapter.OutputString($"Couldn't find the SKU: {p.SKU}");
                        }
                    }
                    catch
                    {
                        GeneralAdapter.OutputErrorString($"Couldn't resolve the inventory {p.Id} SKU: {p.SKU}");
                    }
                }

            }
            checkIdInventoryDone = true;
            GeneralAdapter.OutputString("checkIdInventoryDone Done.");
        }

        static public async Task NotFoundIdInventoryUpdateAsync()
        {
            while (!inventoryUpdateDone || NotFoundIdInventoryUpdateQueue.Count > 0)
            {
                if (NotFoundIdVariantUpdateQueue.Count == 0) await Task.Delay(500);
                InventoryLevel p = null;
                // SQLDone can become true whithout adding anything to Queue
                if (NotFoundIdInventoryUpdateQueue.TryDequeue(out p))
                {

                    try
                    {
                        if (GeneralAdapter.FindIdFromSku(p))
                        {
                            await updateInventoryService.SetAsync(p);
                        }
                        else
                        {
                            GeneralAdapter.OutputString($"Couldn't find the SKU: {p.SKU}");
                        }
                    }
                    catch
                    {
                        GeneralAdapter.OutputString($"Couldn't resolve the inventory {p.Id} SKU: {p.SKU}");
                    }
                }

            }
            checkIdInventoryDone = true;
            GeneralAdapter.OutputString("checkIdInventoryDone Done.");
        }
    }
}
