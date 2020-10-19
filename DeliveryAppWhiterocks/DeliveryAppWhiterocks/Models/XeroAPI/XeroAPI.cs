using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using UnixTimeStamp;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using System.Text.RegularExpressions;
using System.Linq;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class XeroAPI
    {
        public static InvoiceResponse _InvoiceResponse;
        public static AccessToken _accessToken;
        
        public static async Task<bool> GetToken()
        {
            var formVariables = new List<KeyValuePair<string, string>>();
            formVariables.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            formVariables.Add(new KeyValuePair<string, string>("client_id", XeroSettings.clientID));
            formVariables.Add(new KeyValuePair<string, string>("code", Preferences.Get("Code", string.Empty)));
            formVariables.Add(new KeyValuePair<string, string>("redirect_uri", XeroSettings.redirectURI));
            formVariables.Add(new KeyValuePair<string, string>("code_verifier", Preferences.Get("CodeVerifier", string.Empty)));

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            var request = new HttpRequestMessage(HttpMethod.Post, @"https://identity.xero.com/connect/token");
            request.Content = new FormUrlEncodedContent(formVariables);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var token = JsonConvert.DeserializeObject<Token>(await response.Content.ReadAsStringAsync());
            Preferences.Set("AccessToken", token.access_token);
            Preferences.Set("ExpiresIn", token.expires_in);
            Preferences.Set("CurrentTime", UnixTime.GetCurrentTime());
            Preferences.Set("RefreshToken", token.refresh_token);
            
            DecodeAccessToken();
            return true;
        }
        public static void DecodeAccessToken()
        {
            string accessTokenString = Preferences.Get("AccessToken", "");
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(accessTokenString))
            {
                JwtSecurityToken accessToken = handler.ReadJwtToken(accessTokenString);
                JwtPayload myPayload = accessToken.Payload;
                string myPayloadJSON = myPayload.SerializeToJson();
                _accessToken = JsonConvert.DeserializeObject<AccessToken>(myPayloadJSON);

            }
            else
            {
                _accessToken = null;
            }

        }
        public static async Task<bool> GetTenantID()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get("AccessToken", string.Empty));

            HttpResponseMessage response = await client.GetAsync(@"https://api.xero.com/connections");

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            var tenant = JsonConvert.DeserializeObject<List<Tenant>>(responseBody);

            
            string tenantId = Preferences.Get("TenantID", string.Empty);
            bool isExist = false;
            foreach (Tenant t in tenant)
            {
                if (tenantId == t.tenantId) isExist = true;
            }
            if (!isExist)
            {
                
                Preferences.Set("TenantID", tenant[0].tenantId);
                
            }

            App.TenantDatabase.DeleteAllTenants();
            for (int i = 0; i < tenant.Count; i++)
            {
                TenantSQLite tenantSQLite = new TenantSQLite();
                tenantSQLite.TenantID = tenant[i].tenantId;
                tenantSQLite.TenantName = tenant[i].tenantName;
                tenantSQLite.TenantIndex = i;
                App.TenantDatabase.InsertTenant(tenantSQLite);
            }

            return true;
        }

        public static async Task<bool> GetInvoices()
        {
            try { 
                var tenantID = Preferences.Get("TenantID", string.Empty);
                if (tenantID == string.Empty)
                {
                    return false;
                }
            
                HttpResponseMessage response = await HttpClientBuilder(RequestType.Invoices);

                if (!response.IsSuccessStatusCode) return false;

                string responseBody = await response.Content.ReadAsStringAsync();
                _InvoiceResponse = JsonConvert.DeserializeObject<InvoiceResponse>(responseBody);
                // filter invoices by start date
                DateTime defaultDate = DateTime.Now.AddMonths(-3);
                DateTime startDate = Preferences.Get("StartDate", defaultDate);
                List<Invoice> filterInvoices = new List<Invoice>();
                for(int i=0; i < _InvoiceResponse.Invoices.Count; i++)
                {
                    if(_InvoiceResponse.Invoices[i].UpdatedDateUTC > startDate)
                    {
                        filterInvoices.Add(_InvoiceResponse.Invoices[i]);
                    }
                }
                _InvoiceResponse.Invoices = filterInvoices;
            } catch
            {
                return false;
            }
            return true;
        }

        public static async Task<bool> FillData()
        {

            var tenantID = Preferences.Get("TenantID", string.Empty);
            Dictionary<string,InvoiceSQLite> allInvoicesInDatabase = App.InvoiceDatabase.GetAllInvoices().ToDictionary(invX => invX.InvoiceID, invX => invX);
            for (int i = 0; i < _InvoiceResponse.Invoices.Count; i++)
            {
                InvoiceSQLite invoiceInDatabase = null;

                if (allInvoicesInDatabase.ContainsKey(_InvoiceResponse.Invoices[i].InvoiceID)) { 
                    invoiceInDatabase = allInvoicesInDatabase[_InvoiceResponse.Invoices[i].InvoiceID];
                    allInvoicesInDatabase.Remove(_InvoiceResponse.Invoices[i].InvoiceID);
                }

                if (invoiceInDatabase != null && invoiceInDatabase.CompletedDeliveryStatus) continue;
                
                if (_InvoiceResponse.Invoices[i].Status == "AUTHORISED" || _InvoiceResponse.Invoices[i].Status == "PAID")
                {
                    await FillItems(_InvoiceResponse.Invoices[i], i);
                    await FillContactAddress(_InvoiceResponse.Invoices[i].Contact, i);

                    //Insert data normally if the data doesnt exist else check for update
                    if(invoiceInDatabase == null) {
                        InvoiceSQLite invoiceSqlite = new InvoiceSQLite()
                        {
                            InvoiceType = _InvoiceResponse.Invoices[i].Type,
                            InvoiceID = _InvoiceResponse.Invoices[i].InvoiceID,
                            TenantID = tenantID,
                            InvoiceNumber = _InvoiceResponse.Invoices[i].InvoiceNumber,
                            CompletedDeliveryStatus = false,
                            ContactID = _InvoiceResponse.Invoices[i].Contact.ContactID,
                            Subtotal = _InvoiceResponse.Invoices[i].SubTotal,
                            UpdateTimeTicksXERO = _InvoiceResponse.Invoices[i].UpdatedDateUTC.Ticks,
                            UpdateTimeTicksApp = _InvoiceResponse.Invoices[i].UpdatedDateUTC.Ticks,
                        };
                        App.InvoiceDatabase.InsertInvoice(invoiceSqlite, _InvoiceResponse.Invoices[i].LineItems, _InvoiceResponse.Invoices[i].Contact);
                    }
                    else
                    {
                        ContactSQLite contactInDatabase = App.ContactDatabase.GetContactByID(invoiceInDatabase.ContactID);
                        ContactSQLite newContact = App.ContactDatabase.PrepareContactSQLite(_InvoiceResponse.Invoices[i].Contact);
                        
                        if(contactInDatabase.Address != newContact.Address || contactInDatabase.Fullname != newContact.Fullname || 
                            contactInDatabase.City != newContact.City || contactInDatabase.PhoneNumber != newContact.PhoneNumber)
                        {
                            App.ContactDatabase.UpdateContactPosition(newContact);
                        }

                        
                        if (_InvoiceResponse.Invoices[i].UpdatedDateUTC.Ticks == invoiceInDatabase.UpdateTimeTicksXERO) continue;

                        if (invoiceInDatabase.InvoiceNumber != _InvoiceResponse.Invoices[i].InvoiceNumber)
                        {
                            invoiceInDatabase.InvoiceNumber = _InvoiceResponse.Invoices[i].InvoiceNumber;
                        }
                        invoiceInDatabase.UpdateTimeTicksXERO = _InvoiceResponse.Invoices[i].UpdatedDateUTC.Ticks;
                        App.InvoiceDatabase.UpdateInvoiceNumber(invoiceInDatabase);

                        List<LineItemSQLite> lineItemSQLiteList = App.LineItemDatabase.GetLineItemByInvoiceID(_InvoiceResponse.Invoices[i].InvoiceID);

                        var maxItemLineID = App.LineItemDatabase.GetLastLineItem();
                        int itemLineID = maxItemLineID == null ? 1 : maxItemLineID.ItemLineID;

                        foreach (LineItem lineItem in _InvoiceResponse.Invoices[i].LineItems)
                        {
                            
                            ItemSQLite itemSQLite = App.ItemDatabase.GetItemByID(lineItem.ItemCode);
                            if (itemSQLite == null)
                            {
                                ItemSQLite newItem = new ItemSQLite()
                                {
                                    ItemCode = lineItem.ItemCode,
                                    Description = lineItem.Description,
                                    Weight = lineItem.Weight,
                                };
                                if (_InvoiceResponse.Invoices[i].Type == "ACCPAY")
                                {
                                    newItem.UnitCost = lineItem.UnitAmount;
                                }
                                App.ItemDatabase.InsertItem(newItem);

                                itemLineID++;
                                //create the id by referencing lineitemtable
                                LineItemSQLite lineItemSQLite = new LineItemSQLite()
                                {
                                    // if it's not set set the itemline id to 1 else increment 1 from the biggest value
                                    ItemLineID = itemLineID,
                                    InvoiceID = _InvoiceResponse.Invoices[i].InvoiceID,
                                    ItemCode = lineItem.ItemCode,
                                    Quantity = (int)lineItem.Quantity,
                                    UnitAmount = lineItem.UnitAmount,
                                };
                                //Save to db
                                App.LineItemDatabase.InsertLineItem(lineItemSQLite);
                            }
                            else
                            {
                                itemSQLite.Description = lineItem.Description;
                                itemSQLite.ItemCode = lineItem.ItemCode;
                                itemSQLite.Weight = lineItem.Weight;
                                if (_InvoiceResponse.Invoices[i].Type == "ACCPAY")
                                {
                                    itemSQLite.UnitCost = lineItem.UnitAmount;
                                }
                                App.ItemDatabase.UpdateItem(itemSQLite);
                                LineItemSQLite theLineItem = lineItemSQLiteList.Where(lineItemX => lineItemX.ItemCode == lineItem.ItemCode).FirstOrDefault();
                                if(theLineItem == null)
                                {
                                    itemLineID++;
                                    //create the id by referencing lineitemtable
                                    theLineItem = new LineItemSQLite()
                                    {
                                        // if it's not set set the itemline id to 1 else increment 1 from the biggest value
                                        ItemLineID = itemLineID,
                                        InvoiceID = _InvoiceResponse.Invoices[i].InvoiceID,
                                        ItemCode = lineItem.ItemCode,
                                        Quantity = (int)lineItem.Quantity,
                                        UnitAmount = lineItem.UnitAmount,
                                    };
                                    //Save to db
                                    App.LineItemDatabase.InsertLineItem(theLineItem);
                                } else
                                {
                                    theLineItem.Quantity = (int)lineItem.Quantity;
                                    theLineItem.UnitAmount = lineItem.UnitAmount;
                                    App.LineItemDatabase.UpdateLineItem(theLineItem);
                                    lineItemSQLiteList.Remove(theLineItem);
                                }
                            }
                        }

                        if (lineItemSQLiteList.Count > 0)
                        {
                            foreach(LineItemSQLite lineItemSQLite in lineItemSQLiteList)
                            {
                                App.LineItemDatabase.DeleteLineItem(lineItemSQLite);
                            }
                        }
                    }
                }
                else if(_InvoiceResponse.Invoices[i].Status == "VOIDED" && invoiceInDatabase != null)
                {
                    App.InvoiceDatabase.DeleteInvoiceByInvoice(invoiceInDatabase);
                }
            }
            return true;
        }
        private static async Task<bool> FillItems(Invoice invoice, int i)
        {
            var response = await HttpClientBuilder(RequestType.Invoice, invoice.InvoiceID);

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            _InvoiceResponse.Invoices[i] = JsonConvert.DeserializeObject<InvoiceResponse>(responseBody).Invoices[0];

            foreach (LineItem item in _InvoiceResponse.Invoices[i].LineItems)
            {
                double weight = GetWeight(item.Description);
                item.Weight = weight;

                if (!string.IsNullOrEmpty(item.Description))
                {
                    item.ItemCode = item.Description;
                }
            }
            return true;
        }

        public static double GetWeight(string description)
        {
            var weightRaw = Regex.Match(description, @"(\d+(\.\d+)?)|(\.\d+)");
            if (weightRaw.Success) {
                double weight = 0;
                Double.TryParse(weightRaw.ToString(), out weight);
                return weight;
            } else
            {
                return 0;
            }
        }  // GetWeight

        private static async Task<bool> FillContactAddress(Contact contact, int i)
        {

            HttpResponseMessage response = await HttpClientBuilder(RequestType.Contact, contact.ContactID);

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            ContactResponse contactResponse = JsonConvert.DeserializeObject<ContactResponse>(responseBody);
            _InvoiceResponse.Invoices[i].Contact = contactResponse.Contacts[0];
            
            return true;
        }
        public static async Task<bool> RefreshToken()
        {
            var formVariables = new List<KeyValuePair<string, string>>();
            formVariables.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            formVariables.Add(new KeyValuePair<string, string>("client_id", XeroSettings.clientID));
            formVariables.Add(new KeyValuePair<string, string>("refresh_token", Preferences.Get("RefreshToken", string.Empty)));

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            var request = new HttpRequestMessage(HttpMethod.Post, @"https://identity.xero.com/connect/token");
            request.Content = new FormUrlEncodedContent(formVariables);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var token = JsonConvert.DeserializeObject<Token>(await response.Content.ReadAsStringAsync());
            Preferences.Set("AccessToken", token.access_token);
            Preferences.Set("ExpiresIn", token.expires_in);
            Preferences.Set("CurrentTime", UnixTime.GetCurrentTime());
            Preferences.Set("RefreshToken", token.refresh_token);

            DecodeAccessToken();
            return true;
        }
        private static async Task<HttpResponseMessage> HttpClientBuilder(RequestType requestType,params string[] identifier)
        {
            string url = @"https://api.xero.com/api.xro/2.0/";
            if (requestType == RequestType.Contact)
            {
                url += @"Contacts/" + identifier[0];
            }
            else if (requestType == RequestType.Invoice)
            {
                url += @"Invoices/" + identifier[0];
            }
            else
            {
                url += @"Invoices/";
            }

            string tenantID = Preferences.Get("TenantID", string.Empty);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get("AccessToken", string.Empty));
            client.DefaultRequestHeaders.Add("xero-tenant-id", tenantID);
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            return await client.GetAsync(url);
        }
    }
}
