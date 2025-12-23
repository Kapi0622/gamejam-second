using System;
using UnityEngine;

namespace Unity1week.Core
{
    [Serializable]
    public abstract class MessageBase
    {
        public string Message => _message;
        [SerializeField] private string _message;
    }   
}
