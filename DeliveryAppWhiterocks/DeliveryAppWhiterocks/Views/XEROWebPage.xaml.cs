﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliveryAppWhiterocks.Models.XeroAPI;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class XEROWebPage : ContentPage
    {
        public XEROWebPage()
        {
            InitializeComponent();
            InitXeroWebView();

            
        }

        private void InitXeroWebView()
        {
            
            xeroWebView.Source = AuthorizeLink();
            xeroWebView.Navigated += async (object sender, WebNavigatedEventArgs e) =>
            {
                if (e.Url.Contains(@"https://www.xero.com/nz/"))
                {
                    int indexStart = e.Url.IndexOf("code=") + 5;
                    int length = e.Url.IndexOf("&scope=") - indexStart;
                    Preferences.Set("Code", e.Url.Substring(indexStart, length));

                    var isSuccess = await XeroAPI.GetToken();
                    await XeroAPI.GetTenantID();
                    await XeroAPI.GetInvoices();
                    await DisplayAlert("SUCCESS", $"{XeroAPI._InvoiceResponse.Invoices[0].InvoiceNumber}", "OK");
                }
            };
            //await Navigation.PopModalAsync();
        }

        private WebViewSource AuthorizeLink()
        {
                string urlLink;
                string rawData = "thisIsMyCodethisIsMyCodethisIsMyCodethisIsMyCodethisIsMyCode";
                Preferences.Set("CodeVerifier", rawData);
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    string code_challenge = Base64UrlEncoder.Encode(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)));
                    string clientID = XeroSettings.clientID;
                    string scope = XeroSettings.scope;
                    string redirect_uri = XeroSettings.redirectURI;
                    string state = "123";
                    urlLink = string.Format(@"https://login.xero.com/identity/connect/authorize?response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&code_challenge={4}&code_challenge_method=S256", clientID, redirect_uri, scope, state, code_challenge);
                }
                return urlLink;
        }

        
    }
}