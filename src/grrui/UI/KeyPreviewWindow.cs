namespace grrui.UI
{
    using System;
    using System.Collections.Generic;
    using Terminal.Gui;

    public class KeyPreviewWindow : Window
    {
        private readonly Dictionary<Key, Action> _keyActions = new Dictionary<Key, Action>();

        public KeyPreviewWindow(NStack.ustring title)
            : base(title)
        {
        }

        public KeyPreviewWindow(NStack.ustring title, int padding)
            : base(title, padding)
        {
        }

        public override bool ProcessKey(KeyEvent keyEvent)
        {
            if (_keyActions.TryGetValue(keyEvent.Key, out Action action))
            {
                action.Invoke();
            }

            return base.ProcessKey(keyEvent);
        }

        public void DefineKeyAction(Key key, Action action)
        {
            _keyActions[key] = action;
        }
    }
}