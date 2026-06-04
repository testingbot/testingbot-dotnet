using System.Collections.Generic;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>A mobile OS family to filter devices by.</summary>
public enum DevicePlatform
{
    /// <summary>Android (emulators and devices).</summary>
    Android,

    /// <summary>iOS (simulators and devices).</summary>
    Ios,

    /// <summary>Real Android devices only.</summary>
    RealAndroid,

    /// <summary>Real iOS devices only.</summary>
    RealIos,
}

/// <summary>Operations for discovering physical mobile devices in the TestingBot grid.</summary>
public interface IDevicesClient
{
    /// <summary>Lists physical devices, optionally filtered to a single platform (<c>GET /v1/devices</c>).</summary>
    /// <param name="platform">An optional platform filter.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The matching devices, each with an availability flag.</returns>
    Task<IReadOnlyList<Device>> ListAsync(DevicePlatform? platform = null, CancellationToken cancellationToken = default);

    /// <summary>Lists devices the account can acquire right now (<c>GET /v1/devices/available</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The currently available devices.</returns>
    Task<IReadOnlyList<Device>> ListAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a single device by id (<c>GET /v1/devices/:id</c>).</summary>
    /// <param name="deviceId">The numeric device id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The device.</returns>
    Task<Device> GetAsync(int deviceId, CancellationToken cancellationToken = default);
}

internal sealed class DevicesClient(TestingBotConnection connection) : ResourceClient(connection), IDevicesClient
{
    public Task<IReadOnlyList<Device>> ListAsync(DevicePlatform? platform = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("platform", platform switch
        {
            DevicePlatform.Android => "Android",
            DevicePlatform.Ios => "iOS",
            DevicePlatform.RealAndroid => "REAL_ANDROID",
            DevicePlatform.RealIos => "REAL_IOS",
            _ => null,
        });

        return Connection.GetListAsync<Device>("devices", query, cancellationToken);
    }

    public Task<IReadOnlyList<Device>> ListAvailableAsync(CancellationToken cancellationToken = default)
        => Connection.GetListAsync<Device>("devices/available", query: null, cancellationToken);

    public Task<Device> GetAsync(int deviceId, CancellationToken cancellationToken = default)
        => Connection.GetAsync<Device>($"devices/{deviceId}", query: null, cancellationToken);
}
