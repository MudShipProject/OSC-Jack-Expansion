using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OscJackExpansion
{
    public class OscReceiverEx : MonoBehaviour
    {
        [SerializeField] private OscServerEx _server;
        [SerializeField] private List<AddressEventPair> _addressEventPairs = new List<AddressEventPair>();

        [Serializable] public class StringEvent  : UnityEvent<string> { }
        [Serializable] public class IntegerEvent : UnityEvent<int>    { }
        [Serializable] public class FloatEvent   : UnityEvent<float>  { }
        [Serializable] public class BooleanEvent : UnityEvent<bool>   { }
        [Serializable] public class BlobEvent    : UnityEvent<byte[]> { }

        [Serializable]
        public class AddressEventPair
        {
            public string address;
            public StringEvent  stringEvent  = new StringEvent();
            public IntegerEvent integerEvent = new IntegerEvent();
            public FloatEvent   floatEvent   = new FloatEvent();
            public BooleanEvent booleanEvent = new BooleanEvent();
            public BlobEvent    blobEvent    = new BlobEvent();
        }

        public OscServerEx Server
        {
            get => _server;
            set
            {
                if (_server != null)
                    _server.onDataReceived.RemoveListener(OnDataReceived);

                _server = value;

                if (_server != null && isActiveAndEnabled)
                    _server.onDataReceived.AddListener(OnDataReceived);
            }
        }

        public List<AddressEventPair> AddressEventPairs => _addressEventPairs;

        private void OnEnable()
        {
            EnsureServer();
            if (_server != null)
                _server.onDataReceived.AddListener(OnDataReceived);
        }

        private void OnDisable()
        {
            if (_server != null)
                _server.onDataReceived.RemoveListener(OnDataReceived);
        }

        private void EnsureServer()
        {
            if (_server != null) return;
            _server = GetComponent<OscServerEx>();
            if (_server == null)
                _server = gameObject.AddComponent<OscServerEx>();
        }

        public AddressEventPair AddAddressEventPair(string oscAddress)
        {
            var pair = new AddressEventPair { address = oscAddress };
            _addressEventPairs.Add(pair);
            return pair;
        }

        public bool RemoveAddressEventPair(string oscAddress)
        {
            return _addressEventPairs.RemoveAll(p => p.address == oscAddress) > 0;
        }

        public AddressEventPair GetAddressEventPair(string oscAddress)
        {
            for (int i = 0; i < _addressEventPairs.Count; i++)
            {
                if (_addressEventPairs[i].address == oscAddress)
                    return _addressEventPairs[i];
            }
            return null;
        }

        private void OnDataReceived(uOSC.Message message)
        {
            string incomingAddress = message.address;
            object[] values = message.values;

            for (int i = 0; i < _addressEventPairs.Count; i++)
            {
                var pair = _addressEventPairs[i];
                if (string.IsNullOrEmpty(pair.address)) continue;
                if (!MatchAddress(incomingAddress, pair.address)) continue;

                for (int v = 0; v < values.Length; v++)
                {
                    var val = values[v];

                    if (val is string strVal)
                        pair.stringEvent.Invoke(strVal);
                    else if (val is int intVal)
                        pair.integerEvent.Invoke(intVal);
                    else if (val is float floatVal)
                        pair.floatEvent.Invoke(floatVal);
                    else if (val is bool boolVal)
                        pair.booleanEvent.Invoke(boolVal);
                    else if (val is byte[] blobVal)
                        pair.blobEvent.Invoke(blobVal);
                }
            }
        }

        private static bool MatchAddress(string incoming, string pattern)
        {
            if (pattern == "*") return true;
            if (pattern.EndsWith("/*"))
            {
                string prefix = pattern.Substring(0, pattern.Length - 1);
                return incoming.StartsWith(prefix, StringComparison.Ordinal);
            }
            return string.Equals(incoming, pattern, StringComparison.Ordinal);
        }
    }
}
