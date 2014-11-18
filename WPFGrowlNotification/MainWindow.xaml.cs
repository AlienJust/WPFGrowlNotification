using System;
using System.Collections.Generic;
using System.Windows;
using WpfGrowlNotifications;

namespace WPFGrowlNotification {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		private readonly INotificationsContainer _notificationsContainer;
	    private readonly List<Guid> _addedIds;
		public MainWindow() {
			InitializeComponent();
            _addedIds = new List<Guid>();
            _notificationsContainer = new GrowlNotifications(
                new NotificationDataTemplateSelectorFromFile(typeof (MainWindow).GetAssemblyDirectoryPath(), ContainerPosition.Right), 
				"Glow notifications",
				6,
				1, 
				ContainerPosition.Left, 
				10, 
				10, 
				10, 
				10);
		}

		private void ButtonClickAddSampleNotification(object sender, RoutedEventArgs e) {
		    var id = _notificationsContainer.AddNotification(
		        "Mesage #2",
		        "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
		        DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
		        "template1.xaml",
		        () => MessageBox.Show("time has gone"),
		        () => MessageBox.Show("broadcast close"),
		        closeNotificationAction => {
		            MessageBox.Show("User request closing...");
		            closeNotificationAction();
		        },
		        closeNotificationAction => {
		            MessageBox.Show("User's custom action #1");
		            closeNotificationAction();
		        },
		        closeNotificationAction => MessageBox.Show("User's custom action #2 without closing"));

            _addedIds.Add(id);
		}

		private void ButtonClickAddAnotherSampleNotification(object sender, RoutedEventArgs e) {
            var id = _notificationsContainer.AddNotification(
		        "Title of msg #3",
		        "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
		        DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
		        "template2.xaml",
		        () => MessageBox.Show("time has gone"),
		        () => MessageBox.Show("broadcast close"),
		        closeNotificationAction => {
		            MessageBox.Show("User request closing...");
		            closeNotificationAction();
		        });
            _addedIds.Add(id);
		}

		private void ButtonClickRemoveVisibleNotifications(object sender, RoutedEventArgs e) {
			_notificationsContainer.RemoveAllVisibleNotifications();
		}

		private void ButtonClickCloseAllNotifications(object sender, RoutedEventArgs e) {
			_notificationsContainer.RemoveAllNotifications();
		}

		protected override void OnClosed(EventArgs e) {
			(_notificationsContainer as Window).Close();
			base.OnClosed(e);
		}

	    private void ButtonClickCloseLastNotification(object sender, RoutedEventArgs e) {
	        if (_addedIds.Count > 0) {
	            var indexToRemove = _addedIds.Count - 1;
                var guidToRemove = _addedIds[indexToRemove];
                _addedIds.RemoveAt(indexToRemove);
                _notificationsContainer.SetNotificationToRemove(guidToRemove);
	        }
	    }
	}
}
