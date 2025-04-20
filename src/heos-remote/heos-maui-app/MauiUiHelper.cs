using Microsoft.Maui.Controls.Platform;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace heos_maui_app
{
    public static class MauiUiHelper
    {
        public static async Task ShowToast(string msg)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            await Toast.Make(msg,
                             ToastDuration.Long,
                             16)
                        .Show(cancellationTokenSource.Token);
        }
    }
}
