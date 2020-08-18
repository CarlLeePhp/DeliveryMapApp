using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using UnixTimeStamp;
using DeliveryAppWhiterocks.Models.XeroAPI;
using IdentityModel.Client;
using IdentityModel.Jwk;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using Xamarin.Forms;

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

            // decode the access_token to get the authentication_event_id

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(token.access_token))
            {
                JwtSecurityToken accessToken = handler.ReadJwtToken(token.access_token);
                JwtPayload myPayload = accessToken.Payload;
                string myPayloadJSON = myPayload.SerializeToJson();
                _accessToken = JsonConvert.DeserializeObject<AccessToken>(myPayloadJSON);
                
            }
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

            // find a tenant by authentication_event_id
            // In case nothing could be found, just use the first one
            string tenantId = "";
            foreach (Tenant t in tenant)
            {
                if (t.authEventId == _accessToken.authentication_event_id)
                    tenantId = t.tenantId;
            }
            if(tenantId == "") Preferences.Set("TenantID", tenant[0].tenantId);
            Preferences.Set("TenantID", tenantId);

            return true;
        }

        public static async Task<bool> GetInvoices()
        {
            var tenantID = Preferences.Get("TenantID", string.Empty);
            if (tenantID == string.Empty)
            {
                return false;
            }
            
            HttpResponseMessage response = await HttpClientBuilder(RequestType.Invoices);

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            _InvoiceResponse = JsonConvert.DeserializeObject<InvoiceResponse>(responseBody);

            return true;
        }

        public static async Task<bool> FillData()
        {
            
            for (int i = 0; i < _InvoiceResponse.Invoices.Count; i++)
            {
                if (App.InvoiceDatabase.CheckIfExisted(_InvoiceResponse.Invoices[i].InvoiceID) == false) {
                    if(_InvoiceResponse.Invoices[i].Status == "AUTHORISED" || _InvoiceResponse.Invoices[i].Status == "PAID") { 
                        await FillItems(_InvoiceResponse.Invoices[i], i);
                        await FillContactAddress(_InvoiceResponse.Invoices[i].Contact, i);

                        InvoiceSQLite invoiceSqlite = new InvoiceSQLite()
                        {
                            InvoiceID = _InvoiceResponse.Invoices[i].InvoiceID,
                            InvoiceNumber = _InvoiceResponse.Invoices[i].InvoiceNumber,
                            CompletedDeliveryStatus = false,
                            ContactID = _InvoiceResponse.Invoices[i].Contact.ContactID,
                            Subtotal = _InvoiceResponse.Invoices[i].SubTotal
                        };
                        App.InvoiceDatabase.InsertInvoice(invoiceSqlite, _InvoiceResponse.Invoices[i].LineItems, _InvoiceResponse.Invoices[i].Contact);
                    }
                }
            }
            return true;
        }

        private static async Task<bool> FillItems(Invoice invoice, int i)
        {
            Dictionary<string, Stock> itemDictionary = new Dictionary<string, Stock>();

            var response = await HttpClientBuilder(RequestType.Invoice, invoice.InvoiceID);

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            _InvoiceResponse.Invoices[i] = JsonConvert.DeserializeObject<InvoiceResponse>(responseBody).Invoices[0];

            foreach(LineItem item in _InvoiceResponse.Invoices[i].LineItems)
            {
                string codeX;
                //GET WEIGHT HERE

                if (!string.IsNullOrEmpty(item.ItemCode)) { 
                    codeX = item.ItemCode; 
                } else if (!string.IsNullOrEmpty(item.Description))
                {
                    codeX = item.Description;
                } else
                {
                    return false;
                }

                if (!itemDictionary.ContainsKey(codeX))
                {

                    //Get Weight from description
                    //has an {itemName " "?} + {number} kg
                    //possible format 20kg , 20 kg , (20kg), (20)kg, (20) kg

                    double weight = GetWeight(item.Description);
                    Stock stock = new Stock(codeX, item.Description, weight, item.Quantity);
                    itemDictionary.Add(codeX, stock);
                } else
                {
                    //Get Weight from description
                    itemDictionary[codeX].AddStockQuantity(Convert.ToInt32(item.Quantity));
                    itemDictionary[codeX].AddStockWeight(0);
                }
            }
            return true;
        }

        public static double GetWeight(string description)
        {
            double weight = 0;
            description = description.Trim().ToLower();
            if (description.Contains("kg"))
            {
                int index = description.IndexOf("kg");
                description = description.Remove(index).Trim();
                string[] parts = description.Split(' ');
                weight = double.Parse(parts[parts.Length - 1]);
            }
            return weight;
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
