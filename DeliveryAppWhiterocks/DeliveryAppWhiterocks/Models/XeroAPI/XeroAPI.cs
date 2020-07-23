﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Essentials;
using System.Net.Http.Headers;
using UnixTimeStamp;
using DeliveryAppWhiterocks.Models.XeroAPI;
using IdentityModel.Client;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class XeroAPI
    {
        public static InvoiceResponse _InvoiceResponse;

        public static Dictionary<string, Stock> _ItemDictionary;


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
            return true;
        }

        public static async Task<bool> GetTenantID()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get("AccessToken", string.Empty));

            HttpResponseMessage response = await client.GetAsync(@"https://api.xero.com/connections");

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            var tenant = JsonConvert.DeserializeObject<List<Tenant>>(responseBody);
            Preferences.Set("TenantID", tenant[0].tenantId);
            return true;
        }

        public static async Task<bool> GetInvoices()
        {
            var tenantID = Preferences.Get("TenantID", string.Empty);
            if (tenantID == string.Empty)
            {
                return false;
            }
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get("AccessToken", string.Empty));
            client.DefaultRequestHeaders.Add("xero-tenant-id", tenantID);
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            HttpResponseMessage response = await client.GetAsync(@"https://api.xero.com/api.xro/2.0/Invoices");

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            _InvoiceResponse = JsonConvert.DeserializeObject<InvoiceResponse>(responseBody);

            return true;
        }

        public static async Task<bool> FillData()
        {
            _ItemDictionary = new Dictionary<string, Stock>();

            for (int i = 0; i < _InvoiceResponse.Invoices.Count; i++)
            {
                 await FillItems(_InvoiceResponse.Invoices[i], i);
                 await FillContactAddress(_InvoiceResponse.Invoices[i].Contact, i);
            }
            return true;
        }

        private static async Task<bool> FillItems(Invoice invoice, int i)
        {

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get("AccessToken", string.Empty));
            client.DefaultRequestHeaders.Add("xero-tenant-id", Preferences.Get("TenantID", string.Empty));
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            var response = await client.GetAsync(@"https://api.xero.com/api.xro/2.0/Invoices/" + invoice.InvoiceID);

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

                if (!_ItemDictionary.ContainsKey(codeX))
                {
                    //Get Weight from description
                    //has an {itemName " "?} + {number} kg
                    //possible format 20kg , 20 kg , (20kg), (20)kg, (20) kg
                    Stock stock = new Stock(codeX, item.Description, 0, item.Quantity);
                    _ItemDictionary.Add(codeX, stock);
                } else
                {
                    //Get Weight from description
                    _ItemDictionary[codeX].AddStockQuantity(Convert.ToInt32(item.Quantity));
                    _ItemDictionary[codeX].AddStockWeight(0);
                }
            }
            return true;
        }

        private static async Task<bool> FillContactAddress(Contact contact, int i)
        {
            string tenantID = Preferences.Get("TenantID", string.Empty);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Preferences.Get("AccessToken", string.Empty));
            client.DefaultRequestHeaders.Add("xero-tenant-id", tenantID);
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            HttpResponseMessage response = await client.GetAsync(@"https://api.xero.com/api.xro/2.0/Contacts/" + contact.ContactID);

            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();
            ContactResponse contactResponse = JsonConvert.DeserializeObject<ContactResponse>(responseBody);
            _InvoiceResponse.Invoices[i].Contact = contactResponse.Contacts[0];
            
            return true;
        }
    }
}