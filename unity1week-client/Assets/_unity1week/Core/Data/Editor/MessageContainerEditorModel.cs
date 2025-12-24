using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity1week.Core.Editor
{
    public class MessageContainerEditorModel
    {
        private List<Type> _cachedTypes;
        private List<string> _cachedTypeNames;

        public MessageContainerEditorModel()
        {
            _cachedTypes = TypeCache.GetTypesDerivedFrom<MessageBase>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToList();
            
            _cachedTypeNames = _cachedTypes.Select(t => t.Name).ToList();
        }

        public List<Type> GetMessageTypes() => _cachedTypes;
        
        public List<string> GetMessageTypeNames() => _cachedTypeNames;

        public MessageBase CreateMessage(Type type)
        {
            return (MessageBase)Activator.CreateInstance(type);
        }

        public MessageBase ChangeMessageType(MessageBase oldMessage, string newTypeName)
        {
            var newType = _cachedTypes.FirstOrDefault(t => t.Name == newTypeName);
            if (newType == null || oldMessage.GetType() == newType) return oldMessage;

            // Create new instance
            var newMessage = (MessageBase)Activator.CreateInstance(newType);

            // Preserve data using JsonUtility (works well for base class fields like _message)
            var json = JsonUtility.ToJson(oldMessage);
            JsonUtility.FromJsonOverwrite(json, newMessage);

            return newMessage;
        }
    }
}