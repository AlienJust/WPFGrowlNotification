using System;

namespace WpfGrowlNotifications {
	interface INotificationsStorage : INotificationsContainer {
        void RemoveNotification(Guid id);
		void ExecOnGuiThread(Action action);
	}
}