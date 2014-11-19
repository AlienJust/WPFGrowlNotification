using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfGrowlNotifications {
    internal class Notification : INotification, INotifyPropertyChanged {
        private readonly Guid _id;
        private readonly string _title;
        private readonly string _message;
        private readonly string _footer;
        private readonly string _dataTemplateSelectorArgument;

        private readonly Action _timeoutAction;
        private readonly Action _closedBroadcastAction;
        private readonly Action<Action>[] _userActions;

        private readonly object _sync;
        private readonly LineGeometry _closedNotifyLine;
        private readonly ICommand _userCommand;
        private bool _isCloseNeeded;


        private bool _isClosed;
        private readonly INotificationsStorage _notificationsContainer;

        public Notification(INotificationsStorage container, Guid id, string title, string message, string footer, string dataTemplateSelectorArgument, Action timeoutAction, Action closedBroadcastAction, Action<Action>[] userActions) {

            _isClosed = false;
            _notificationsContainer = container;
            _id = id;
            _title = title;
            _message = message;
            _footer = footer;
            _dataTemplateSelectorArgument = dataTemplateSelectorArgument;
            _timeoutAction = timeoutAction;
            _closedBroadcastAction = closedBroadcastAction;
            _userActions = userActions;

            _sync = new object();

            _closedNotifyLine = new LineGeometry(new Point(0, 0), new Point(0, 0));
            _closedNotifyLine.Changed += ClosedLineChanged;

            _userCommand = new RelayCommand<string>(ExecuteUserCommand);


            _isCloseNeeded = false;
        }


        private void ExecuteUserCommand(string number) {
            try {
                var n = int.Parse(number);
                lock (_sync) {
                    if (!_isClosed) {
                        _userActions[n].Invoke(() => _notificationsContainer.SetNotificationToRemove(_id));
                    }
                }
            }
            catch {
                // TODO: remove empty catch
            }
        }

        private void ClosedLineChanged(object sender, EventArgs eventArgs) {
            if (_closedNotifyLine.EndPoint.X >= 1) {
                //IsCloseNeeded = true;
                lock (_sync) {
                    try {
                        if (!_isClosed) {
                            _isClosed = true;
                            _timeoutAction();
                        }
                    }
                    finally {
                        RemoveLinksUnsubscribeEventsSetCloseFlag(); // отписка в любом случае по завершению анимации
                    }
                }
            }
        }

        private void RemoveLinksUnsubscribeEventsSetCloseFlag() {
            _notificationsContainer.RemoveNotification(_id); // Self removing from container
            _notificationsContainer.ExecOnGuiThread(() => _closedNotifyLine.Changed -= ClosedLineChanged);
        }

        public void SetNeedToCloseBroadcast() {
            lock (_sync) {
                if (_closedBroadcastAction != null) _closedBroadcastAction();
            }
        }

        public void SetNeedToCloseSingle() {
            lock (_sync) {
                if (!_isClosed) {
                    _isClosed = true;
                    IsCloseNeeded = true;
                }
            }
        }

        public string DataTemplateSelectorArgument {
            get { return _dataTemplateSelectorArgument; }
        }

        public string Message {
            get { return _message; }
        }

        public string Footer {
            get { return _footer; }
        }

        public Guid Id {
            get { return _id; }
        }

        public string Title {
            get { return _title; }
        }

        public LineGeometry Close {
            get { return _closedNotifyLine; }
        }

        public ICommand UserCommand {
            get { return _userCommand; }
        }

        public bool IsCloseNeeded {
            get { return _isCloseNeeded; }
            set {
                if (_isCloseNeeded != value) {
                    _isCloseNeeded = value;
                    OnPropertyChanged("IsCloseNeeded");
                }
            }
        }

        public bool IsClosed {
            get { lock (_sync) return _isClosed; }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}