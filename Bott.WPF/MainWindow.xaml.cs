using Bott.Core;
using Bott.Core.Contracts;
using Bott.WPF;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Bott;

public partial class MainWindow : Window
{
	private readonly Client _client;
	private readonly DispatcherTimer _timer = new()
	{
		Interval = TimeSpan.FromSeconds(1)
	};

	private readonly Dictionary<string, Action<Client, Message>> _commands;

	public MainWindow()
	{
		InitializeComponent();

		_client = new(
			"7840531152:AAE8TLfe3zarMP-AYsi6P06ScvVco8InG9k",
			new Logger(LogResponse, ErrorResponse),
			new Logger(LogUpdate, ErrorUpdate)
		);
		_client.MessageCreated += OnMessageCreated;

		_timer.Tick += async (s, e) => await _client.Poll();

		_commands = typeof(Commands).GetMethods(BindingFlags.Public | BindingFlags.Static)
			.Select(method => new KeyValuePair<string, Action<Client, Message>>(
				(method.GetCustomAttribute<CommandNameAttribute>()?.CommandName ?? string.Empty).ToLower(),
				method.CreateDelegate<Action<Client, Message>>()))
			.ToDictionary();
	}

	private async void OnLoaded(object sender, RoutedEventArgs e)
	{
		await _client.Poll();

		_timer.Start();
	}

	private async void OnMessageCreated(object? sender, Message message)
	{
		if (message.Text == "/start")
		{
			await _client.ReactMessageAsync(message.Id, message.Chat.Id, "☃");

			var commandNames = _commands.Keys.ToArray();
			var buttons = new string[1, commandNames.Length];

			for (int i = 0; i < commandNames.Length; ++i)
				buttons[0, i] = $"{commandNames[i][..1].ToUpper()}{commandNames[i][1..]}";

			await _client.SendKeyboardAsync(message.Chat.Id, "Команды", buttons);
			return;
		}

		var command = _commands.GetValueOrDefault(message.Text.ToLower());

		command?.Invoke(_client, message);
	}

	private void LogResponse(string message)
	{
		logs.Items.Insert(0, new TextBox { IsReadOnly = true, Text = message });
	}

	private void ErrorResponse(string message)
	{
		logs.Items.Insert(0, new TextBox { IsReadOnly = true, Foreground = Brushes.Red, Text = message });
	}

	private void LogUpdate(string message)
	{
		updates.Items.Insert(0, new TextBox { IsReadOnly = true, Text = message });
	}

	private void ErrorUpdate(string message)
	{
		updates.Items.Insert(0, new TextBox { IsReadOnly = true, Foreground = Brushes.Red, Text = message });
	}
}