using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity1week.Core.Editor
{
    public class MessageContainerEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _visualTreeAsset;
        [SerializeField] private MessageContainer _asset;

        private MessageContainerEditorModel _model;
        private SerializedObject _serializedObject;

        private MultiColumnListView _mainView;
        private VisualElement _rightPane;
        private ToolbarMenu _addMenu;
        private ToolbarButton _removeButton;

        [OnOpenAsset(0)]
        public static bool OpenWindow(int instanceID)
        {
            var obj = EditorUtility.EntityIdToObject(instanceID);
            if (obj is MessageContainer asset)
            {
                var window = GetWindow<MessageContainerEditorWindow>();
                window.titleContent = new GUIContent("Message Editor");
                window._asset = asset;
                window._model = new();
                window.Initialize();
                window.Show();
                return true;
            }

            return false;
        }

        private void CreateGUI()
        {
            if (_visualTreeAsset == null)
            {
                _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_unity1week/Core/Data/Editor/MessageContainerEditorWindow.uxml");
            }

            if (_visualTreeAsset == null)
            {
                rootVisualElement.Add(new Label("VisualTreeAsset is missing."));
                return;
            }
            
            _model ??= new MessageContainerEditorModel();

            _visualTreeAsset.CloneTree(rootVisualElement);

            _mainView = rootVisualElement.Q<MultiColumnListView>("MainView");
            _rightPane = rootVisualElement.Q<VisualElement>("RightPane");
            _addMenu = rootVisualElement.Q<ToolbarMenu>("AddMenu");
            _removeButton = rootVisualElement.Q<ToolbarButton>("RemoveButton");

            if (_mainView == null || _rightPane == null || _addMenu == null || _removeButton == null)
            {
                rootVisualElement.Add(new Label("UI Elements missing in UXML."));
                return;
            }

            SetupListView();
            SetupToolbar();

            if (_asset != null)
            {
                Initialize();
            }
        }

        private void SetupListView()
        {
            // Type Column
            _mainView.columns["TypeColumn"].makeCell = () =>
            {
                var dropdown = new DropdownField
                {
                    choices = _model.GetMessageTypeNames(),
                    style = { flexGrow = 1, justifyContent = Justify.Center }
                };
                
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    var element = evt.target as VisualElement;
                    if (element?.userData is int index)
                    {
                        ChangeMessageType(index, evt.newValue);
                    }
                });
                
                return dropdown;
            };

            _mainView.columns["TypeColumn"].bindCell = (element, index) =>
            {
                if (element is DropdownField dropdown)
                {
                    dropdown.userData = index;
                    var message = _mainView.itemsSource[index] as MessageBase;
                    if (message != null)
                    {
                        dropdown.SetValueWithoutNotify(message.GetType().Name);
                    }
                }
            };

            // Preview Column
            _mainView.columns["PreviewColumn"].makeCell = () => new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, flexGrow = 1 } };
            _mainView.columns["PreviewColumn"].bindCell = (element, index) =>
            {
                var message = _mainView.itemsSource[index] as MessageBase;
                (element as Label).text = message?.Message ?? "";
            };

            _mainView.selectionChanged += OnSelectionChanged;
            
            // Undo/Redo support: Refresh list when changes occur
            _mainView.RegisterCallback<PointerDownEvent>(_ => Repaint());
        }

        private void ChangeMessageType(int index, string newTypeName)
        {
            if (index < 0 || index >= _asset.Messages.Count) return;

            var oldMessage = _asset.Messages[index];
            if (oldMessage.GetType().Name == newTypeName) return;

            var newMessage = _model.ChangeMessageType(oldMessage, newTypeName);

            Undo.RecordObject(_asset, "Change Message Type");
            _asset.Messages[index] = newMessage;
            EditorUtility.SetDirty(_asset);

            // If the changed item was selected, refresh the inspector
            if (_mainView.selectedIndex == index)
            {
                OnSelectionChanged(_mainView.selectedItems);
            }
            
            // We don't need full Rebuild() here as the dropdown value is already updated by UI,
            // but we might want to refresh if there are other side effects. 
            // However, full Rebuild might reset the dropdown focus/state.
            // Let's just repaint.
            _mainView.RefreshItem(index);
        }

        private void SetupToolbar()
        {
            _removeButton.clicked += RemoveSelected;

            var types = _model.GetMessageTypes();
            foreach (var type in types)
            {
                _addMenu.menu.AppendAction(type.Name, action => AddMessage(type));
            }
        }

        private void Initialize()
        {
            _serializedObject = new SerializedObject(_asset);
            _mainView.itemsSource = _asset.Messages;
            _mainView.Rebuild();
        }

        private void AddMessage(Type type)
        {
            var newMessage = _model.CreateMessage(type);
            
            Undo.RecordObject(_asset, "Add Message");
            _asset.Messages.Add(newMessage);
            EditorUtility.SetDirty(_asset);
            
            _mainView.Rebuild();
            _mainView.SetSelection(_asset.Messages.Count - 1);
        }

        private void RemoveSelected()
        {
            var selectedIndices = new List<int>(_mainView.selectedIndices);
            if (selectedIndices.Count == 0) return;

            Undo.RecordObject(_asset, "Remove Message");
            selectedIndices.Sort((a, b) => b.CompareTo(a)); // Remove from back to front
            foreach (var index in selectedIndices)
            {
                if (index >= 0 && index < _asset.Messages.Count)
                {
                    _asset.Messages.RemoveAt(index);
                }
            }
            EditorUtility.SetDirty(_asset);

            _mainView.ClearSelection();
            _mainView.Rebuild();
            _rightPane.Clear();
        }

        private void OnSelectionChanged(IEnumerable<object> selection)
        {
            _rightPane.Clear();

            var selectedIndex = _mainView.selectedIndex;
            if (selectedIndex < 0 || selectedIndex >= _asset.Messages.Count) return;

            _serializedObject.Update();
            var messagesProp = _serializedObject.FindProperty("_messages");
            if (messagesProp == null || selectedIndex >= messagesProp.arraySize) return;

            var elementProp = messagesProp.GetArrayElementAtIndex(selectedIndex);
            
            // Iterate through children to avoid the top-level foldout
            var iterator = elementProp.Copy();
            var endProperty = iterator.GetEndProperty();
            bool enterChildren = true;
            
            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                var childField = new PropertyField(iterator.Copy());
                childField.Bind(_serializedObject);
                childField.RegisterValueChangeCallback(_ => _mainView.Rebuild());
                _rightPane.Add(childField);
                
                enterChildren = false; // Only enter the first level of children
            }
        }
    }
}
