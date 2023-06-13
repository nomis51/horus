﻿using WinPass.Shared.Abstractions;
using WinPass.Shared.Models.Fs;

namespace WinPass.Core.Services;

public class AppService : IService
{
    #region Singleton

    private static readonly object LockInstance = new();
#pragma warning disable CS8618
    private static AppService _instance;
#pragma warning restore CS8618

    public static AppService Instance
    {
        get
        {
            lock (LockInstance)
            {
                // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                _instance ??= new AppService();
            }

            return _instance;
        }
    }

    #endregion

    #region Services

    private readonly FsService _fsService;
    private readonly GpgService _gpgService;

    #endregion

    #region Constructors

    private AppService()
    {
        _fsService = new FsService();
        _gpgService = new GpgService();
    }

    #endregion

    #region Public methods

    public string GetPassword(string name)
    {
        var path = _fsService.GetPath(name);
        return _gpgService.Decrypt(path);
    }

    public IEnumerable<StoreEntry> ListStoreEntries()
    {
        return _fsService.ListStoreEntries();
    }

    public void InitializeStoreFolder(string gpgKey)
    {
        _fsService.InitializeStoreFolder(gpgKey);
    }

    public bool DoGpgKeyExists(string key)
    {
        return _gpgService.DoKeyExists(key);
    }

    public void Initialize()
    {
        _fsService.Initialize();
    }

    #endregion
}