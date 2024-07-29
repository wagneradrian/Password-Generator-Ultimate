using System;
using System.Linq;
using System.Text;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace PasswordGeneratorUWP
{
    public sealed partial class MainPage : Page
    {
        private static readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        private static StringBuilder password = new StringBuilder(100);
        private static readonly Random random = new Random();
        private static bool charAmountToLow;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 500));
            ApplicationView.PreferredLaunchViewSize = new Size(500, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
#if DEBUG
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en";
#endif
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Application.Current.Suspending += OnSuspending;
        }

        private void Generate()
        {
            charAmountToLow = false;

            for (int i = 0; i < Slider_Amount.Value; i++)
            {
                password.Clear();

                string upper = CheckBox_Uppercase.IsChecked == true || ToggleSwitch_Custom.IsOn == true ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
                string lower = CheckBox_Lowercase.IsChecked == true || ToggleSwitch_Custom.IsOn == true ? "abcdefghijklmnopqrstuvwxyz" : "";
                string number = CheckBox_Numbers.IsChecked == true || ToggleSwitch_Custom.IsOn == true ? "12345678901234567890" : "";
                string symbol = CheckBox_Symbols.IsChecked == true || ToggleSwitch_Custom.IsOn == true ? "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~" : "";
                string include = CheckBox_Include.IsChecked == true || ToggleSwitch_Custom.IsOn == true ? TextBox_Include.Text : "";

                if (CheckBox_Exclude.IsChecked == true)
                {
                    char[] exclude = TextBox_Exclude.Text.ToCharArray();
                    upper = string.Join("", upper.Split(exclude));
                    lower = string.Join("", lower.Split(exclude));
                    number = string.Join("", number.Split(exclude));
                    symbol = string.Join("", symbol.Split(exclude));
                    include = string.Join("", include.Split(exclude));
                }

                if (ToggleSwitch_Custom.IsOn == false)
                {
                    RandomizeChars(Slider_Lenght.Value, upper + lower + number + symbol + include);
                    if (charAmountToLow == true)
                    {
                        Warning();
                        return;
                    }
                    ListView_Passwords.Items.Insert(0, password.ToString());
                    continue;
                }

                RandomizeChars(Slider_Uppercase.Value, upper);
                RandomizeChars(Slider_Lowercase.Value, lower);
                RandomizeChars(Slider_Number.Value, number);
                RandomizeChars(Slider_Symbol.Value, symbol);
                if (CheckBox_Include.IsChecked == true)
                    RandomizeChars(Slider_Include.Value, include);
                if (charAmountToLow == true)
                {
                    Warning();
                    return;
                }

                if (CheckBox_Preserve.IsChecked == false)
                {
                    int remainingChars = password.Length;
                    while (remainingChars > 1)
                    {
                        remainingChars--;
                        int randomIndex = random.Next(remainingChars + 1);
                        (password[remainingChars], password[randomIndex]) = (password[randomIndex], password[remainingChars]);
                    }

                }
                ListView_Passwords.Items.Insert(0, password.ToString());
            }
            AppBarButton_Clear.IsEnabled = true;
        }

        private void RandomizeChars(double sliderValue, string chars)
        {
            for (int i = 0; i < sliderValue; i++)
            {
                if (chars != "")
                {
                    password.Append(chars[random.Next(chars.Length)]);
                    if (CheckBox_NoDuplicateChars.IsChecked == true)
                        chars = string.Join("", chars.Split(password[password.Length - 1]));
                }
                else
                {
                    charAmountToLow = true;
                    return;
                }
            }
        }

        private async void Warning()
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = resourceLoader.GetString("ContentDialog_WarningTitle"),
                Content = resourceLoader.GetString("ContentDialog_WarningContent"),
                CloseButtonText = "OK"
            };
            _ = await contentDialog.ShowAsync();
        }

        private void Copy()
        {
            if (ListView_Passwords.SelectedItem != null)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(ListView_Passwords.SelectedItem.ToString());
                Clipboard.SetContent(dataPackage);
            }
        }

        private void AppBarButton_Generate_Click(object sender, RoutedEventArgs e)
        {
            if (AppBarToggleButton_Append.IsChecked == false)
            {
                ListView_Passwords.Items.Clear();
                AppBarButton_Clear.IsEnabled = false;
                AppBarButton_Copy.IsEnabled = false;
            }
            Generate();
        }

        private void AppBarButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            Copy();
            AppBarButton_Copy.IsEnabled = false;
        }

        private void AppBarButton_Clear_Click(object sender, RoutedEventArgs e)
        {
            ListView_Passwords.Items.Clear();
            AppBarButton_Copy.IsEnabled = false;
            AppBarButton_Clear.IsEnabled = false;
        }

        private void ListView_Passwords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppBarButton_Copy.IsEnabled = true;
            if (AppBarToggleButton_InstantCopy.IsChecked == true)
                Copy();
        }

        private void ToggleSwitch_Custom_Toggled(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitch_Custom.IsOn == true)
            {
                GridCustomCharLength.Visibility = Visibility.Visible;
                GridRandomCharLength.Visibility = Visibility.Collapsed;
                TextBox_Include.Width = 71;
                UpdateLengthCountCustomChar();
            }
            else
            {
                GridCustomCharLength.Visibility = Visibility.Collapsed;
                GridRandomCharLength.Visibility = Visibility.Visible;
                TextBox_Include.Width = 158;
                UpdateGenerateButtonStateRandomChar();
            }
        }

        private void UpdateLengthCountCustomChar()
        {
            if (ToggleSwitch_Custom?.IsOn == true)
            {
                double totalLength = Slider_Uppercase.Value + Slider_Lowercase.Value + Slider_Number.Value + Slider_Symbol.Value;
                if (CheckBox_Include.IsChecked == true)
                    totalLength += Slider_Include.Value;
                TextBlock_LengthCount.Text = totalLength.ToString();
                AppBarButton_Generate.IsEnabled = totalLength != 0;
            }
        }

        private void Slider_CustomChar_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateLengthCountCustomChar();
        }

        private void UpdateGenerateButtonStateRandomChar()
        {
            if (ToggleSwitch_Custom != null && !ToggleSwitch_Custom.IsOn)
            {
                bool anyChecked = CheckBox_Uppercase.IsChecked == true || CheckBox_Lowercase.IsChecked == true || CheckBox_Numbers.IsChecked == true || CheckBox_Symbols.IsChecked == true;
                if (CheckBox_Include.IsChecked == true)
                    anyChecked = anyChecked || CheckBox_Include.IsChecked == true;
                AppBarButton_Generate.IsEnabled = anyChecked;
            }
        }

        private void CheckboxRandomChar_Click(object sender, RoutedEventArgs e)
        {
            UpdateGenerateButtonStateRandomChar();
        }

        private void CheckBox_Include_Click(object sender, RoutedEventArgs e)
        {
            _ = CheckBox_Include.IsChecked == true ? TextBox_Include.IsEnabled = true : TextBox_Include.IsEnabled = false;
            _ = CheckBox_Include.IsChecked == true ? Slider_Include.IsEnabled = true : Slider_Include.IsEnabled = false;
            UpdateLengthCountCustomChar();
            UpdateGenerateButtonStateRandomChar();
        }

        private void TextBox_Include_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox_Include.Text))
                TextBox_Include.Text = "!#$%&?";
        }

        private void TextBox_Include_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (Slider_Include.Value != 0)
                AppBarButton_Generate.IsEnabled = !string.IsNullOrEmpty(TextBox_Include.Text);
            else
                UpdateLengthCountCustomChar();
        }

        private void CheckBox_Exclude_Click(object sender, RoutedEventArgs e)
        {
            _ = CheckBox_Exclude.IsChecked == true ? TextBox_Exclude.IsEnabled = true : TextBox_Exclude.IsEnabled = false;
        }

        private void TextBox_Exclude_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBox_Exclude.Text))
                TextBox_Exclude.Text = "il1Lo0O";
        }

        private void Textbox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => char.IsWhiteSpace(c));
        }

        private void AppBarToggleButton_InstantCopy_Click(object sender, RoutedEventArgs e)
        {
            _ = AppBarToggleButton_InstantCopy.IsChecked == true ? AppBarButton_Copy.Visibility = Visibility.Collapsed : AppBarButton_Copy.Visibility = Visibility.Visible;
        }

        private async void AppBarButton_Reset_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog contentDialog = new ContentDialog
            {
                Title = resourceLoader.GetString("ContentDialog_ResetTitle"),
                Content = resourceLoader.GetString("ContentDialog_ResetContent"),
                PrimaryButtonText = "OK",
                CloseButtonText = resourceLoader.GetString("ContentDialog_ResetClose")
            };

            ContentDialogResult cdResult = await contentDialog.ShowAsync();
            if (cdResult == ContentDialogResult.Primary)
            {
                await ApplicationData.Current.ClearAsync();
                Frame.Navigate(this.GetType());
            }
        }

        private void AppBarButton_Feedback_Click(object sender, RoutedEventArgs e)
        {
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9NRNDSZ216LH"));
        }

        private void AppBarButton_GitHub_Click(object sender, RoutedEventArgs e)
        {
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/wagneradrian/Password-Generator-Ultimate"));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            Slider_Lenght.Value = int.Parse(localSettings.Values["Slider_Lenght"] as string ?? "20");
            CheckBox_Uppercase.IsChecked = bool.Parse(localSettings.Values["Uppercase"] as string ?? "true");
            CheckBox_Lowercase.IsChecked = bool.Parse(localSettings.Values["Lowercase"] as string ?? "true");
            CheckBox_Numbers.IsChecked = bool.Parse(localSettings.Values["Numbers"] as string ?? "true");
            CheckBox_Symbols.IsChecked = bool.Parse(localSettings.Values["Symbols"] as string ?? "true");
            Slider_Uppercase.Value = int.Parse(localSettings.Values["Slider_Uppercase"] as string ?? "5");
            Slider_Lowercase.Value = int.Parse(localSettings.Values["Slider_Lowercase"] as string ?? "5");
            Slider_Number.Value = int.Parse(localSettings.Values["Slider_Number"] as string ?? "5");
            Slider_Symbol.Value = int.Parse(localSettings.Values["Slider_Symbol"] as string ?? "5");
            CheckBox_Preserve.IsChecked = bool.Parse(localSettings.Values["Preserve"] as string ?? "false");
            CheckBox_Include.IsChecked = bool.Parse(localSettings.Values["Include"] as string ?? "false");
            TextBox_Include.Text = localSettings.Values["IncludeText"] as string ?? "!#$%&?";
            Slider_Include.Value = int.Parse(localSettings.Values["Slider_Include"] as string ?? "5");
            CheckBox_Exclude.IsChecked = bool.Parse(localSettings.Values["Exclude"] as string ?? "false");
            TextBox_Exclude.Text = localSettings.Values["ExcludeText"] as string ?? "il1Lo0O";
            CheckBox_NoDuplicateChars.IsChecked = bool.Parse(localSettings.Values["NoDuplicateChars"] as string ?? "false");
            ToggleSwitch_Custom.IsOn = bool.Parse(localSettings.Values["ToggleSwitch_Custom"] as string ?? "false");
            Slider_Amount.Value = int.Parse(localSettings.Values["Slider_Amount"] as string ?? "10");
            AppBarToggleButton_InstantCopy.IsChecked = bool.Parse(localSettings.Values["InstantCopy"] as string ?? "false");
            AppBarToggleButton_Append.IsChecked = bool.Parse(localSettings.Values["Append"] as string ?? "false");
            _ = CheckBox_Include.IsChecked == true ? TextBox_Include.IsEnabled = true : TextBox_Include.IsEnabled = false;
            _ = CheckBox_Include.IsChecked == true ? Slider_Include.IsEnabled = true : Slider_Include.IsEnabled = false;
            _ = CheckBox_Exclude.IsChecked == true ? TextBox_Exclude.IsEnabled = true : TextBox_Exclude.IsEnabled = false;
            _ = AppBarToggleButton_InstantCopy.IsChecked == true ? AppBarButton_Copy.Visibility = Visibility.Collapsed : AppBarButton_Copy.Visibility = Visibility.Visible;

            UpdateLengthCountCustomChar();
            UpdateGenerateButtonStateRandomChar();
            if (AppBarButton_Generate.IsEnabled == true)
                Generate();
        }

        private void OnSuspending(Object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["Slider_Lenght"] = Slider_Lenght.Value.ToString();
            localSettings.Values["Uppercase"] = CheckBox_Uppercase.IsChecked.ToString();
            localSettings.Values["Lowercase"] = CheckBox_Lowercase.IsChecked.ToString();
            localSettings.Values["Numbers"] = CheckBox_Numbers.IsChecked.ToString();
            localSettings.Values["Symbols"] = CheckBox_Symbols.IsChecked.ToString();
            localSettings.Values["Slider_Uppercase"] = Slider_Uppercase.Value.ToString();
            localSettings.Values["Slider_Lowercase"] = Slider_Lowercase.Value.ToString();
            localSettings.Values["Slider_Number"] = Slider_Number.Value.ToString();
            localSettings.Values["Slider_Symbol"] = Slider_Symbol.Value.ToString();
            localSettings.Values["Preserve"] = CheckBox_Preserve.IsChecked.ToString();
            localSettings.Values["Include"] = CheckBox_Include.IsChecked.ToString();
            localSettings.Values["IncludeText"] = TextBox_Include.Text;
            localSettings.Values["Slider_Include"] = Slider_Include.Value.ToString();
            localSettings.Values["Exclude"] = CheckBox_Exclude.IsChecked.ToString();
            localSettings.Values["ExcludeText"] = TextBox_Exclude.Text;
            localSettings.Values["NoDuplicateChars"] = CheckBox_NoDuplicateChars.IsChecked.ToString();
            localSettings.Values["ToggleSwitch_Custom"] = ToggleSwitch_Custom.IsOn.ToString();
            localSettings.Values["Slider_Amount"] = Slider_Amount.Value.ToString();
            localSettings.Values["InstantCopy"] = AppBarToggleButton_InstantCopy.IsChecked.ToString();
            localSettings.Values["Append"] = AppBarToggleButton_Append.IsChecked.ToString();
            deferral.Complete();
        }
    }
}
