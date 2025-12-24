using System.Collections.Generic;
using UnityEngine;

namespace Unity1week.Core
{
    [CreateAssetMenu(menuName = "Unity1week/Message Container")]
    public sealed class MessageContainer : ScriptableObject
    {
        public List<MessageBase> Messages
        {
            get
            {
                _messages ??= new();
                return _messages;
            }
        }

        [SerializeReference] private List<MessageBase> _messages;
    }
}