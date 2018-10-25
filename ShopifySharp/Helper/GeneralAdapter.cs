using System;
using System.Collections.Generic;
using System.Text;

namespace ShopifySharp.Helper
{
    public static class GeneralAdapter
    {
        /// <summary>
        /// PLace your own string output method here
        /// </summary>
        public static Action<string> OutputString = Console.WriteLine;
        public static Action<string> OutputErrorString = Console.WriteLine;
        /// <summary>
        /// Send a ProductVariant that was searched by SKU to that function so it can be updated outside. (DI API TODO)
        /// </summary>
        public static Action<ProductVariant> OutputFoundVariantId = (ProductVariant pv) => { };




        public static bool FindIdFromSku(ProductVariant pv, List<ProductVariant> list)
        {
            ProductVariant vit = list.Find(e => e.SKU == pv.SKU);
            if (vit != null)
            {
                if (vit.Id == null || vit.InventoryItemId == null)
                {
                    OutputErrorString($"Couldn't find Id From Sku in Shopify. SKU in SAP: {pv.SKU} SKU in Shopify: {vit.SKU}");
                }
                OutputFoundVariantId(vit);
                pv.Id = vit.Id;
                pv.ProductId = vit.ProductId;
                pv.InventoryItemId = vit.InventoryItemId;
                return true;
            }
            return false;
        }

        public static bool FindIdFromSku(InventoryLevel il, List<ProductVariant> list)
        {
            ProductVariant vit = list.Find(e => e.SKU == il.SKU);
            if (vit != null)
            {
                if (vit.Id == null || vit.InventoryItemId == null)
                {
                    OutputErrorString($"Couldn't find Id From Sku in Shopify. SKU in SAP: {il.SKU} SKU in Shopify: {il.SKU}");
                }
                OutputFoundVariantId(vit);
                il.Id = vit.Id;
                il.InventoryItemId = vit.InventoryItemId;
                return true;
            }
            return false;
        }

        public static bool FindIdFromSku(InventoryLevel il)
        {
            return FindIdFromSku(il, ProductExport.variantList);
        }

        public static bool FindIdFromSku(ProductVariant pv)
        {
            return FindIdFromSku(pv, ProductExport.variantList);
        }

    }
}
