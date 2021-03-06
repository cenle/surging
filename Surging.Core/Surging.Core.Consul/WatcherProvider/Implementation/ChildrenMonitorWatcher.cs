﻿using Consul;
using System;
using System.Threading.Tasks;
using Surging.Core.Consul.Utilitys;
using Surging.Core.Consul.WatcherProvider.Implementation;

namespace Surging.Core.Consul.WatcherProvider
{
    public class ChildrenMonitorWatcher : WatcherBase
    {
        private readonly Action<string[], string[]> _action;
        private readonly IClientWatchManager _manager;
        private readonly ConsulClient _client;
        private readonly string _path;
        private string[] _currentData = new string[0];
        public ChildrenMonitorWatcher(ConsulClient client, IClientWatchManager manager,string path, Action<string[], string[]> action)
        {
            this._action = action;
            _manager = manager;
            _client = client;
            _path = path;
            RegisterWatch();
        }

        public ChildrenMonitorWatcher SetCurrentData(string[] currentData)
        {
            _currentData = currentData ?? new string[0];
            return this;
        }

        protected override async Task ProcessImpl()
        {
            Func<ChildrenMonitorWatcher> getWatcher = () => new ChildrenMonitorWatcher(_client, _manager, _path, _action);
            var watcher = getWatcher();
            RegisterWatch(watcher);
            var result = await _client.GetChildrenAsync(_path);
            if (result != null)
            {
                _action(_currentData, result);
                watcher.SetCurrentData(result);
            }
        }

        private void RegisterWatch(Watcher watcher=null)
        {
            ChildWatchRegistration wcb = null;
            if (watcher != null)
            {
                wcb = new ChildWatchRegistration(_manager, watcher, _path);
            }
            else
            {
                wcb = new ChildWatchRegistration(_manager, this, _path);
            }
            wcb.Register();
        }
    }
}
