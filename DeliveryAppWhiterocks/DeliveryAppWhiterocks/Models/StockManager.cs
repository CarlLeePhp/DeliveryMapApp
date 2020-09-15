using DeliveryAppWhiterocks.Models.Database.SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models
{
    public class StockManager
    {
        public static Dictionary<string,Stock> GetStock(List<InvoiceSQLite> invoiceSQLite)
        {
            Dictionary<string,Stock> itemDictionary =  new Dictionary<string, Stock>();

            foreach (InvoiceSQLite invoice in invoiceSQLite)
            {
                List<LineItemSQLite> lineItemSQLite = App.LineItemDatabase.GetLineItemByInvoiceID(invoice.InvoiceID);
                foreach (LineItemSQLite lineItem in lineItemSQLite)
                {
                    string codeX = lineItem.ItemCode;
                    ItemSQLite itemSQLite = App.ItemDatabase.GetItemByID(lineItem.ItemCode);
                    if (!itemDictionary.ContainsKey(codeX))
                    {
                        Stock stock = new Stock(codeX, itemSQLite.Description, itemSQLite.Weight * lineItem.Quantity, lineItem.Quantity);
                        itemDictionary.Add(codeX, stock);
                    }
                    else
                    {
                        itemDictionary[codeX].AddStockQuantity(Convert.ToInt32(lineItem.Quantity));
                        itemDictionary[codeX].Weight += itemSQLite.Weight * lineItem.Quantity; 
                    }
                }
            }

            return itemDictionary;
        }

        public static double CalculateStockWeight(Dictionary<string,Stock> itemDictionary)
        {
            double totalWeight = 0;
            foreach (KeyValuePair<string, Stock> stock in itemDictionary)
            {
                Stock stockX = stock.Value;
                totalWeight += stockX.Weight;
            }

            return totalWeight;
        }
    }
}
