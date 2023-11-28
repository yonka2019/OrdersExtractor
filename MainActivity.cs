using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using OrdersExtractor.API;
using OrdersExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Todoist.Net.Models;


namespace OrdersExtractor
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private EditText tokenET;
        private EditText projectNameET;
        private EditText phoneNumberET;
        private Button syncB;

        private ISharedPreferences prefs;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);
            SetRefs();

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            RestoreSettings();  // restore existing settings, otherwise - it will empty
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void SetRefs()
        {
            tokenET = FindViewById<EditText>(Resource.Id.tokenET);
            projectNameET = FindViewById<EditText>(Resource.Id.projectNameET);
            phoneNumberET = FindViewById<EditText>(Resource.Id.phoneNumberET);
            syncB = FindViewById<Button>(Resource.Id.syncB);
            syncB.Click += SyncB_Click;
        }

        private async void SyncB_Click(object sender, EventArgs e)
        {
            int synced = 0;
            if (SettingsSet)
            {
                SaveSettings();

                TodoistAPI todoist = default;
                Project project = default;
                try
                {
                    todoist = new TodoistAPI(tokenET.Text);
                    await todoist.TestAuth();  // test the given token

                    project = await todoist.GetProject(projectNameET.Text);
                }
                catch (Exception ex)
                {
                    Toast.MakeText(Application.Context, $"ERROR - {ex.Message}", ToastLength.Long).Show();
                    return;
                }


                List<string> alreadySyncedOrders = RestoreAlreadySyncedOrders().ToList();  // restore already synced orders
                List<Order> orders = SMSData.ExtractOrders(ContentResolver, phoneNumberET.Text);  // get all orders
                orders = orders.Where(order => !alreadySyncedOrders.Contains(order.PackageNumber + order.TrackNumber)).ToList();  // filter orders which were already synced before

                ProgressDialog progressDialog = new ProgressDialog(this);
                progressDialog.SetTitle("Syncing...");
                progressDialog.SetMessage($"Syncing tasks 0/{orders.Count()} with Todoist, please wait..");

                progressDialog.Show();

                foreach (Order order in orders)
                {
                    bool result = await todoist.AddTask(project.Id, order.PackageNumber, order.ToString());  // add each order as a Todoist task

                    if (!result)
                        Toast.MakeText(Application.Context, $"ERROR - {order.PackageNumber}\nContinuing..", ToastLength.Long).Show();

                    /* ## GIT README
                     * ERROR - ${PACKAGE_NUMBER}
                     * can be raised if:
                     * - required project not found
                     * - task title empty
                     * - task description empty
                     */
                    progressDialog.SetMessage($"Syncing tasks {++synced}/{orders.Count()} with Todoist, please wait..");
                }
                Toast.MakeText(Application.Context, "Synced successfully", ToastLength.Long).Show();

                progressDialog.Dismiss();

                alreadySyncedOrders.AddRange(orders.Select(order => order.PackageNumber + order.TrackNumber));  // add all orders which were added now, they are already synced. This is in order to skip them next sync
                SaveAlreadySyncedOrders(alreadySyncedOrders);

                Finish();
            }
            else
                Toast.MakeText(Application.Context, "Fill all settings", ToastLength.Long).Show();
        }

        private void SaveSettings()
        {
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString("token", tokenET.Text);
            editor.PutString("projectName", projectNameET.Text);
            editor.PutString("phoneNumber", phoneNumberET.Text);

            editor.Commit();
        }

        private void RestoreSettings()
        {
            tokenET.Text = prefs.GetString("token", "");
            projectNameET.Text = prefs.GetString("projectName", "");
            phoneNumberET.Text = prefs.GetString("phoneNumber", "");
        }

        private void SaveAlreadySyncedOrders(List<string> ordersPackageNumbers)
        {
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutStringSet("orders", ordersPackageNumbers);

            editor.Commit();
        }

        private ICollection<string> RestoreAlreadySyncedOrders()
        {
            return prefs.GetStringSet("orders", new List<string>());
        }

        private bool SettingsSet => tokenET.Text != "" && projectNameET.Text != "" && phoneNumberET.Text != "";
    }
}