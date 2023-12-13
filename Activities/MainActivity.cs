using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using OrdersExtractor.API;
using OrdersExtractor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Todoist.Net.Models;
using Xamarin.Essentials;

/*
 * In order to calibrate all current packages - 
 * You can create a temp project 'test', and sync with him, the app will save all already synced packages,
 * after that, change the project name to the required one, and next sync will sync only the updated
 */
namespace OrdersExtractor.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        // If app crashes - check if permission given
        private EditText tokenET;
        private EditText projectNameET;
        private EditText phoneNumberET;
        private Button syncB;
        private Button clearB;

        private ISharedPreferences prefs;


        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            await GetPermissions();
            SetContentView(Resource.Layout.activity_main);
            SetRefs();

            prefs = GetSharedPreferences("datafile", FileCreationMode.Private);

            RestoreSettings();  // restore existing settings, otherwise - it will empty
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async Task GetPermissions()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Sms>();
                if (status != PermissionStatus.Granted)
                {
                    Toast.MakeText(Application.Context, $"Need access to SMS", ToastLength.Long).Show();
                    Finish();
                }
            }
        }

        private void SetRefs()
        {
            tokenET = FindViewById<EditText>(Resource.Id.tokenET);
            projectNameET = FindViewById<EditText>(Resource.Id.projectNameET);
            phoneNumberET = FindViewById<EditText>(Resource.Id.phoneNumberET);
            syncB = FindViewById<Button>(Resource.Id.syncB);
            clearB = FindViewById<Button>(Resource.Id.clearB);
            clearB.Click += ClearB_Click;
            syncB.Click += SyncB_Click;
        }

        private void ClearB_Click(object sender, EventArgs e)
        {
            AndroidX.AppCompat.App.AlertDialog.Builder alert = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
            alert.SetTitle("Confirm clearing SP data");
            alert.SetMessage("Warning, clearing SP will clear the entire application data");
            alert.SetPositiveButton("Clear SP anyway", (senderAlert, args) =>
            {
                ISharedPreferencesEditor editor = prefs.Edit();
                editor.Clear();
                editor.Commit();

                Toast.MakeText(Application.Context, $"SP Cleared", ToastLength.Long).Show();
            });

            alert.SetNegativeButton("Nevermind", (senderAlert, args) =>
            {
                Toast.MakeText(this, "Cancelled", ToastLength.Short).Show();
            });

            Dialog dialog = alert.Create();
            dialog.Show();
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
                    await todoist.SetUserID();

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
                progressDialog.SetMessage($"Syncing task 0/{orders.Count()} with Todoist, please wait..");
                progressDialog.Indeterminate = true;
                progressDialog.SetCancelable(false);

                progressDialog.Show();

                foreach (Order order in orders)
                {
                    bool result = false;
                    Exception _ex = null;

                    try
                    {
                        // set current user as assignee
                        result = await todoist.AddTask(project.Id, order.PackageNumber, order.ToString(), todoist.UserID, "Package", Priority.Priority2);  // add each order as a Todoist task
                    }
                    catch (Exception ex)
                    {
                        _ex = ex;
                    }

                    if (!result || _ex != null)  // if bad result OR got any exception 
                        Toast.MakeText(Application.Context, $"ERROR with {order.PackageNumber} - {_ex.Message}\nContinuing..", ToastLength.Long).Show();

                    /* ## GIT README
                     * ERROR - ${PACKAGE_NUMBER}
                     * can be raised if:
                     * - required project not found
                     * - task title empty
                     * - task description empty
                     */
                    progressDialog.SetMessage($"Syncing tasks {++synced}/{orders.Count()} with Todoist, please wait..");
                }
                Toast.MakeText(Application.Context, "Sync Finished", ToastLength.Long).Show();

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