using Dashboard.Core.Abstractions.Calendar;
using Microsoft.Maui.ApplicationModel;

namespace Dashboard.App.Platforms.Android.Services;

/// <summary>
/// Implémentation MAUI de <see cref="ICalendarPermissionRequester"/> :
/// délègue à <see cref="Permissions.RequestAsync{TPermission}"/> pour
/// <c>READ_CALENDAR</c>. Les appels MAUI sont marshalés sur le main thread
/// via <see cref="MainThread.InvokeOnMainThreadAsync(Func{Task{bool}})"/>
/// car le runtime exige que les demandes de permission y soient effectuées.
/// </summary>
internal sealed class AndroidCalendarPermissionRequester : ICalendarPermissionRequester
{
    public async Task<bool> IsGrantedAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var status = await MainThread.InvokeOnMainThreadAsync(
            () => Permissions.CheckStatusAsync<Permissions.CalendarRead>())
            .ConfigureAwait(false);
        return status == PermissionStatus.Granted;
    }

    public async Task<bool> RequestAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var status = await MainThread.InvokeOnMainThreadAsync(
            () => Permissions.RequestAsync<Permissions.CalendarRead>())
            .ConfigureAwait(false);
        return status == PermissionStatus.Granted;
    }
}
