using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using OrdersExtractor.Activities;

namespace OrdersExtractor.Widgets
{
    [BroadcastReceiver(Label = "Widget Button Sync")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [IntentFilter(new string[] { "com.yonka.ordersextractor.ACTION_WIDGET_SYNC" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/my_widget_provider")]
    internal class ExtractWidget : AppWidgetProvider
    {
        public static string ACTION_WIDGET_SYNC = "sync";

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            //Update Widget layout
            //Run when create widget or meet update time
            ComponentName me = new ComponentName(context, Java.Lang.Class.FromType(typeof(ExtractWidget)).Name);
            appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));
        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
        {
            //Build widget layout
            RemoteViews widgetView = new RemoteViews(context.PackageName, Resource.Layout.widget_layout);


            //Handle click event of button on Widget
            RegisterClicks(context, appWidgetIds, widgetView);

            return widgetView;
        }


        private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
        {
            Intent intent = new Intent(context, typeof(ExtractWidget));
            intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            widgetView.SetOnClickPendingIntent(Resource.Id.widgetSync, GetPendingSelfIntent(context, ACTION_WIDGET_SYNC));
        }

        private PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            Intent intent = new Intent(context, typeof(ExtractWidget));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.Mutable);
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Check if the click is from the "ACTION_WIDGET_SYNC or ACTION_WIDGET_TURNON" button
            if (ACTION_WIDGET_SYNC.Equals(intent.Action))
            {
                Intent mainActivity = new Intent(context, typeof(MainActivity));
                mainActivity.SetFlags(ActivityFlags.NewTask);

                mainActivity.PutExtra("widget", true);
                context.StartActivity(mainActivity);
            }
        }
    }
}