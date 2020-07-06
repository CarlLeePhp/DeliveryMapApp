using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliveryAppWhiterocks.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
    
        public LoginPage()
        {
            InitializeComponent();
            SetAppMargin();
            Init();
        }

        private void SetAppMargin()
        {
            //DisplayAlert("Test", $"{App.screenWidth.ToString()} {App.screenHeight.ToString()}", "OK");
            topRowMargin.Height = App.screenHeight / 12;
            botRowMargin.Height = App.screenHeight / 12;
            leftColMargin.Width = App.screenWidth / 6;
            rightColMargin.Width = App.screenWidth / 6;
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            BackgroundColor = Constants.backgroundColor;
            List<Label> labelList = new List<Label>();
            labelList.Add(usernameLbl);
            labelList.Add(passwordLbl);

            foreach (Label label in labelList)
            {
                label.TextColor = Constants.secondaryTextColor;
                label.FontAttributes = FontAttributes.Bold;
                label.FontSize = 16;
                label.Padding = new Thickness(0, 10, 0, 0);
            }

            List<Entry> entryList = new List<Entry>();
            entryList.Add(passwordEntry);
            entryList.Add(usernameEntry);

            usernameEntry.Completed += UsernameEntry_Completed;
            passwordEntry.Completed += PasswordEntry_Completed;

            foreach (Entry entry in entryList)
            {
                entry.TextColor = Constants.contrastColor;
                entry.FontAttributes = FontAttributes.Bold;
                entry.BackgroundColor = Constants.secondaryTextColor;
            }

            signInBtn.CornerRadius = 10;
            signInBtn.BackgroundColor = Constants.mainTextColor;
            signInBtn.FontAttributes = FontAttributes.Bold;

            App.CheckInternetIfConnected(noInternetLbl, this);
            activityIndicator.IsVisible = false;    
        }

        private void PasswordEntry_Completed(object sender, EventArgs e)
        {
            SignInBtn_Clicked(sender, e);
        }

        private void UsernameEntry_Completed(object sender, EventArgs e)
        {
            passwordEntry.Focus();
        }

        private async void SignInBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                activityIndicator.IsVisible = true;
                User user = new User(usernameEntry.Text, passwordEntry.Text);
                if (user.checkInformation())
                {
                    App.UserDatabase.SaveUser(user);
                    activityIndicator.IsVisible = false;
                    await this.Navigation.PushAsync(new OrderPage());
                }
                else
                {
                    await DisplayAlert("Login", "Wrong credentials, please try again", "OK");
                    activityIndicator.IsVisible = false;
                }
            } catch
            {
                await DisplayAlert("Login", "Wrong credentials, please try again", "OK");
                activityIndicator.IsVisible = false;
            }
        }
    }
}