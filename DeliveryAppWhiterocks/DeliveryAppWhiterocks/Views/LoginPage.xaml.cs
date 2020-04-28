using System;
using System.Collections.Generic;
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
            }

            List<Entry> entryList = new List<Entry>();
            entryList.Add(passwordEntry);
            entryList.Add(usernameEntry);

            foreach (Entry entry in entryList)
            {
                entry.TextColor = Constants.contrastColor;
                entry.FontAttributes = FontAttributes.Bold;
                entry.BackgroundColor = Constants.secondaryTextColor;
            }

            signInBtn.FontAttributes = FontAttributes.Bold;

        }

        private void SignInBtn_Clicked(object sender, EventArgs e)
        {
            User user = new User(usernameEntry.Text, passwordEntry.Text);
            if (user.checkInformation())
            {
                DisplayAlert("Login", "Successful in log in into the system", "OK");
            }
            else
            {
                DisplayAlert("Login", "Unsuccessful in log in into the system", "OK");
            }
        }
    }
}