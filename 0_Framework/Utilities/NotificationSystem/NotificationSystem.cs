using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace _0_Framework.Utilities.NotificationSystem
{
    public static class NotificationSystem
    {
        public static void ShowNotification(ITempDataDictionary tempData, string title, string message, string icon)
        {
            tempData["NotifTitle"] = title;
            tempData["NotifText"] = message;
            tempData["NotifIcon"] = icon;
        }
    }
}
