using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace WpfGrowlNotifications {
	public partial class GrowlNotifications : INotificationsStorage {
	    private readonly object _sync;
	    private readonly byte _maxNotifications;
		private readonly Dispatcher _threadNotify;
		private readonly int _zeroBasedMonitorIndex;
		private readonly ContainerPosition _horziontalAlignment;


		private readonly int _leftOffset;
		private readonly int _topOffset;
		private readonly int _rightOffset;
		private readonly int _bottomOffset;

	    private readonly object _syncUserActions;
	    private readonly Thread _userActionsThread;
	    private readonly List<Action> _userActions;
	    private bool _stopThread;
	    private readonly AutoResetEvent _signalUserActionAdded;

	    private readonly GrowlNotificationsWindowViewModel _viewModel;

        public GrowlNotifications(DataTemplateSelector customTemplateSelector, string containerWindowTitle, byte maxNotifications, int zeroBasedMonitorIndex, ContainerPosition horziontalAlignment, int leftOffset, int topOffset, int rightOffset, int bottomOffset)
        {
            _viewModel = new GrowlNotificationsWindowViewModel(containerWindowTitle, customTemplateSelector, new Notifications(), new Notifications());
            DataContext = _viewModel;

			_maxNotifications = maxNotifications;
			_zeroBasedMonitorIndex = zeroBasedMonitorIndex;
			_horziontalAlignment = horziontalAlignment;
			_leftOffset = leftOffset;
			_topOffset = topOffset;
			_rightOffset = rightOffset;
			_bottomOffset = bottomOffset;
			
			_threadNotify = Dispatcher.CurrentDispatcher;
            _sync = new object();


            _syncUserActions = new object();
		    _stopThread = false;
            _signalUserActionAdded = new AutoResetEvent(false);
            _userActions = new List<Action>();
		    _userActionsThread = new Thread(WaitForAction) {IsBackground = true};
            _userActionsThread.SetApartmentState(ApartmentState.STA);

			InitializeComponent();
            _userActionsThread.Start();
		}



	    private void WaitForAction() {
            
	        while (true) {
                if (StopTrheadTs) break;
	            _signalUserActionAdded.WaitOne(1000);
                if (StopTrheadTs) break;

                int count;
	            lock (_sync) {
	                count = _userActions.Count;
	            }

	            while (count > 0) {
	                if (StopTrheadTs) break;
	                Action action;
	                lock (_syncUserActions) {
	                    action = _userActions[0];
	                }

	                try {
	                    action();
	                }
	                catch {
	                    //swallow exception
	                }
	                finally {
	                    _userActions.RemoveAt(0);
	                    count--;
	                }

	                if (StopTrheadTs) break;
	            }
                if (StopTrheadTs) break;
	        }

	        _userActions.Clear();
	    }

	    private void AddUserAction(Action action) {
	        lock (_syncUserActions) {
	            _userActions.Add(action);
	        }
	        _signalUserActionAdded.Set();
	    }

	    private bool StopTrheadTs {
	        get {
	            lock (_syncUserActions) {
	                return _stopThread;
	            }
	        }
	        set {
	            lock (_syncUserActions) {
	                _stopThread = value;
	            }
	        }
	    }



	    public Guid AddNotification(string title, string message, string footer, string xamlDefinitionPath, Action timeoutAction, Action closedBroadcastAction, params Action<Action>[] userActions) {
	        var id = Guid.NewGuid();
	        ExecOnGuiThread(() => {
	            var notification = new Notification(
	                this,
	                id,
	                title,
	                message,
	                footer,
	                xamlDefinitionPath,
	                timeoutAction,
	                () => AddUserAction(closedBroadcastAction),
	                userActions.Select(ua => (Action<Action>) (i => AddUserAction(() => ua(i)))).ToArray());
                   // {
	                   // HorizontalPosition = _horziontalAlignment == ContainerPosition.Left ? "Left" : "Right"
	                //};

	            lock (_sync) {
	                if (_viewModel.VisibleNotifications.Count >= _maxNotifications)
	                    _viewModel.BufferedNotifications.Add(notification);
	                else
                        _viewModel.VisibleNotifications.Add(notification);
	            }

                if (_viewModel.VisibleNotifications.Count > 0 && !IsActive)
	                ExecOnGuiThread(() => Topmost = false);
	            ExecOnGuiThread(Show);
	            ExecOnGuiThread(() => Topmost = true);
	        });
	        return id;
	    }

	    public void ExecOnGuiThread(Action action) {
			_threadNotify.Invoke(action);
		}

		public void DestroyContainer() {
		    StopTrheadTs = true;
		    _userActionsThread.Join();

			Action action = Close;
			_threadNotify.Invoke(action);
		}

		public void RemoveAllVisibleNotifications() {
		    ExecOnGuiThread(() => {
		        lock (_sync) {
		            // remove visible ones
                    for (int i = 0; i < _viewModel.VisibleNotifications.Count; ++i)
                    {
                        _viewModel.VisibleNotifications[i].SetNeedToCloseBroadcast();
		            }
                    for (int i = 0; i < _viewModel.VisibleNotifications.Count; ++i)
                    {
                        _viewModel.VisibleNotifications[i].SetNeedToCloseSingle();
                    }
		        }
		    });
		}

		public void RemoveAllNotifications() {
		    ExecOnGuiThread(() => {
		        lock (_sync) {
                    while (_viewModel.BufferedNotifications.Count > 0)
                    {
                        _viewModel.BufferedNotifications.RemoveAt(0);
		            }
		        }
		        RemoveAllVisibleNotifications();
		    });
		}

		public void RemoveNotification(Guid id) {
		    ExecOnGuiThread(() => {
		        lock (_sync) {
                    var notification = _viewModel.VisibleNotifications.FirstOrDefault(n => n.Id == id);
		            if (notification != null) {
                        _viewModel.VisibleNotifications.Remove(notification);

                        if (_viewModel.BufferedNotifications.Count == 0) return;
                        _viewModel.VisibleNotifications.Add(_viewModel.BufferedNotifications[0]);
                        _viewModel.BufferedNotifications.RemoveAt(0);
		            }
		        }

                if (_viewModel.VisibleNotifications.Count < 1 && IsActive)
		            Close();
		    });
		}



        public void SetNotificationToRemove(Guid id)
        {
            ExecOnGuiThread(() =>
            {
                lock (_sync)
                {
                    var notification = _viewModel.VisibleNotifications.FirstOrDefault(n => n.Id == id);
                    if (notification != null)
                    {
                        notification.SetNeedToCloseSingle();
                        return;
                    }

                    notification = _viewModel.BufferedNotifications.FirstOrDefault(n => n.Id == id);
                    if (notification != null)
                        _viewModel.BufferedNotifications.Remove(notification);
                }
            });
	    }



	    private void WindowLoaded(object sender, RoutedEventArgs e) {
			var screen = Screen.AllScreens[_zeroBasedMonitorIndex];

			// Set transperent container window to fit selected screen size:
			Top = screen.WorkingArea.Top + _topOffset;
			Left = screen.WorkingArea.Left + _leftOffset;
			Width = screen.WorkingArea.Width - _leftOffset - _rightOffset;
			Height = screen.WorkingArea.Height - _topOffset - _bottomOffset;
		}
	}
}
