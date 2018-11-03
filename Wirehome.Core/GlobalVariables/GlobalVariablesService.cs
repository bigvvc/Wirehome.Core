﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Wirehome.Core.MessageBus;
using Wirehome.Core.Model;
using Wirehome.Core.Python;
using Wirehome.Core.Python.Proxies;
using Wirehome.Core.Storage;

namespace Wirehome.Core.GlobalVariables
{
    public class GlobalVariablesService
    {
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

        private readonly StorageService _storageService;
        private readonly MessageBusService _messageBusService;
        private readonly ILogger _logger;

        public GlobalVariablesService(
            StorageService storageService, 
            PythonEngineService pythonEngineService, 
            MessageBusService messageBusService, 
            ILoggerFactory loggerFactory)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _messageBusService = messageBusService ?? throw new ArgumentNullException(nameof(messageBusService));

            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<GlobalVariablesService>();

            if (pythonEngineService == null) throw new ArgumentNullException(nameof(pythonEngineService));
            pythonEngineService.RegisterSingletonProxy(new GlobalVariablesPythonProxy(this));
        }

        public void Start()
        {
            lock (_variables)
            {
                Load();
            }
        }

        public void RegisterValue(string uid, object value)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            lock (_variables)
            {
                if (!_variables.ContainsKey(uid))
                {
                    _variables.Add(uid, value);
                    Save();
                }
            }
        }

        public Dictionary<string, object> GetValues()
        {
            var result = new Dictionary<string, object>();
            lock (_variables)
            {
                foreach (var variable in _variables)
                {
                    result[variable.Key] = variable.Value;
                }
            }

            return result;
        }

        public object GetValue(string uid, object defaultValue = null)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            lock (_variables)
            {
                if (!_variables.TryGetValue(uid, out var value))
                {
                    return defaultValue;
                }

                return value;
            }
        }

        public bool ValueExists(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            lock (_variables)
            {
                return _variables.ContainsKey(uid);
            }
        }

        public void SetValue(string uid, object value)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            object oldValue;
            lock (_variables)
            {
                if (_variables.TryGetValue(uid, out oldValue))
                {
                    if (Equals(oldValue, value))
                    {
                        return;
                    }
                }
                
                _variables[uid] = value;

                Save();
            }

            var busMessage = new WirehomeDictionary()
                .WithValue("type", "global_variables.event.value_set")
                .WithValue("uid", uid)
                .WithValue("old_value", oldValue)
                .WithValue("new_value", value);

            _logger.LogInformation("Global variable '{0}' changed to '{1}'.", uid, value);
            _messageBusService.Publish(busMessage);
        }

        public void DeleteValue(string uid)
        {
            if (uid == null) throw new ArgumentNullException(nameof(uid));

            lock (_variables)
            {
                if (!_variables.Remove(uid))
                {
                    return;
                }
            }

            var busMessage = new WirehomeDictionary()
                .WithValue("type", "global_variables.event.value_deleted")
                .WithValue("uid", uid);

            _logger.LogInformation("Global variable '{0}' removed.", uid);
            _messageBusService.Publish(busMessage);
        }

        private void Load()
        {
            if (_storageService.TryRead(out Dictionary<string, object> globalVariables, "GlobalVariables.json"))
            {
                if (globalVariables == null)
                {
                    return;
                }
                
                foreach (var variable in globalVariables)
                {
                    _variables[variable.Key] = variable.Value;
                }
            }
        }

        private void Save()
        {
            _storageService.Write(_variables, "GlobalVariables.json");
        }
    }
}
