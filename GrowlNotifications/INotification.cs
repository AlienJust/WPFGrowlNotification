using System;

namespace WpfGrowlNotifications {
	public interface INotification {
        Guid Id { get; }
		string Title { get; }
		string Message { get; }
		string Footer { get; }
		string DataTemplateSelectorArgument { get; }
		bool IsClosed { get; }
		void SetNeedToCloseBroadcast();
		void SetNeedToCloseSingle();
	}
}