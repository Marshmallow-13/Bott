using Bott.Core;
using Bott.Core.Contracts;

namespace Bott.WPF;

internal class Commands
{

	[CommandName("Мяу")]
	public static async void Squak(Client client, Message message)
	{
		await client.ReplyMessageAsync(message.Id, message.Chat.Id, "Мяу!");
	}

	[CommandName("Очистить чат")]
	public static async void Clear(Client client, Message message)
	{
		var lastMessageId = 0;

		do
		{
			await client.DeleteMessages(message.Chat.Id, Enumerable.Range(lastMessageId, 100).Select(id => (long)id).ToArray());
			lastMessageId += 100;
		}
		while (lastMessageId < message.Id);

	}
}
