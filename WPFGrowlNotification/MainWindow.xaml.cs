using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using WpfGrowlNotifications;

namespace WPFGrowlNotification {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
	    private const string TemplatesDirectoryPath = "Templates";
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
		        Path.Combine(TemplatesDirectoryPath, "template1.xaml"),
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
		        Path.Combine(TemplatesDirectoryPath, "template2.xaml"),
				//Path.Combine(TemplatesDirectoryPath, "std_green_forever.xaml"),
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
		    //var window = _notificationsContainer as Window;
		    //if (window != null) window.Close();
		    //base.OnClosed(e);
            _notificationsContainer.DestroyContainer();
		}

	    private void ButtonClickCloseLastNotification(object sender, RoutedEventArgs e) {
	        if (_addedIds.Count > 0) {
	            var indexToRemove = _addedIds.Count - 1;
                var guidToRemove = _addedIds[indexToRemove];
                _addedIds.RemoveAt(indexToRemove);
                _notificationsContainer.SetNotificationToRemove(guidToRemove);
	        }
	    }

	    private void ButtonClickAddCustomSampleNotification(object sender, RoutedEventArgs e) {
	        var dialog = new OpenFileDialog {InitialDirectory = Path.Combine(typeof (MainWindow).GetAssemblyDirectoryPath(), TemplatesDirectoryPath), Filter = "XAML templates | *.xaml", Multiselect = true};
	        var dialogResult = dialog.ShowDialog();
	        if (dialogResult.HasValue && dialogResult.Value) {
	            foreach (var fileName in dialog.FileNames) {
	                var id = _notificationsContainer.AddNotification(
	                    "Custom message",
	                    "Long long time ago\nIn a galaxy far away",
	                    DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss"),
	                    fileName,
	                    () => MessageBox.Show("time has gone"),
	                    () => MessageBox.Show("broadcast close"),
	                    closeNotificationAction => closeNotificationAction());
	                _addedIds.Add(id);
	            }
	        }
	    }
	}
}
