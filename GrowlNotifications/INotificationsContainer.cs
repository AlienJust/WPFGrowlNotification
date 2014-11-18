using System;

namespace WpfGrowlNotifications {
	public interface INotificationsContainer {
		/// <summary>
		/// Adds notification to container
		/// </summary>
		/// <param name="title">Title of notification</param>
		/// <param name="message">Message text</param>
		/// <param name="footer">Footer of notification</param>
		/// <param name="xamlDefinitionPath">XAML data template file location</param>
		/// <param name="timeoutAction">Action will be executed after notification hides after timeout</param>
		/// <param name="closedBroadcastAction">Action will be executed when all or visible notifications are closed</param>
		/// <param name="userActions">User's actions with possibility to hide notification that binded to view as command UserCommand with params from 0 to ArrayLength</param>
		/// <returns>Id of the added notification, it allows to remove notification by Id later</returns>
		Guid AddNotification(string title, string message, string footer, string xamlDefinitionPath, Action timeoutAction, Action closedBroadcastAction, params Action<Action>[] userActions);

		/// <summary>
		/// TODO: do I really need this method
		/// </summary>
		void DestroyContainer();
		
		/// <summary>
		/// Removes all visible for now notifications
		/// </summary>
		void RemoveAllVisibleNotifications();

		/// <summary>
		/// Remove all notifications
		/// </summary>
		void RemoveAllNotifications();

		/// <summary>
		/// Removes notification by id
		/// </summary>
		/// <param name="id">Id of notification to remove</param>
		void SetNotificationToRemove(Guid id);
	}
}