using System;
using System.Collections;
using Beamable.Coroutines;
using UnityEngine;

namespace Beamable.Platform.SDK
{
	public class ConnectivityService
	{
		private const string _reachabilityAddress = "8.8.8.8";
		private const float _timeout = 5.0f;

    private float _pingStartDateTime;
    private Ping _ping;
    private WaitForSeconds _delay;
    private CoroutineService _coroutineService;

    public bool HasConnectivity { get; private set; }
    public event Action<bool> OnConnectivityChanged;

    public ConnectivityService(CoroutineService coroutineService)
    {
        _delay = new WaitForSeconds(3);
        _coroutineService = coroutineService;
        _coroutineService.StartCoroutine(CheckConnectivity());
    }
    private IEnumerator CheckConnectivity()
    {
        SetHasInternet(true);
        while (true)
        {
            _pingStartDateTime = Time.time;
            _ping = new Ping(_reachabilityAddress);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                _ping = null;
            }
            yield return _delay;
            if (_ping != null)
            {
                if (_ping.isDone)
                {
                    SetHasInternet(true);
                }
                else if (Time.time - _pingStartDateTime > _timeout)
                {
                    SetHasInternet(false);
                }
                else
                {
                    SetHasInternet(false);
                }
            }
            else
            {
                SetHasInternet(false);
            }
        }
    }
    public void SetHasInternet(bool hasInternet)
    {
        if (hasInternet != HasConnectivity)
        {
          OnConnectivityChanged?.Invoke(hasInternet);
        }
        HasConnectivity = hasInternet;
    }
  }
}