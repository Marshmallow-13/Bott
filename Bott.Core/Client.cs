using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bott.Core.Contracts;
using Bott.Core.Contracts.Responses;
using Bott.Core.Contracts.Updates;

namespace Bott.Core;

public class Client(string token, ILogger responseLogger, ILogger updateLogger)
{
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri($"https://api.telegram.org/bot{token}/") };
    private readonly ILogger _responseLogger = responseLogger;
    private readonly ILogger _updateLogger = updateLogger;

    private long _lastUpdateId;
    private bool _polling = false;

    public event EventHandler<Message> MessageCreated = null!;
    public event EventHandler<Contracts.MemberUpdate> MemberUpdated = null!;

    public static readonly string[] AvailableReactions = ["👍", "👎", "❤", "🔥", "🥰", "👏", "😁", "🤔", "🤯", "😱", "🤬", "😢", "🎉", "🤩", "🤮", "💩", "🙏", "👌", "🕊", "🤡", "🥱", "🥴", "😍", "🐳", "❤‍🔥", "🌚", "🌭", "💯", "🤣", "⚡", "🍌", "🏆", "💔", "🤨", "😐", "🍓", "🍾", "💋", "🖕", "😈", "😴", "😭", "🤓", "👻", "👨‍💻", "👀", "🎃", "🙈", "😇", "😨", "🤝", "✍", "🤗", "🫡", "🎅", "🎄", "☃", "💅", "🤪", "🗿", "🆒", "💘", "🙉", "🦄", "😘", "💊", "🙊", "😎", "👾", "🤷‍♂", "🤷", "🤷‍♀", "😡"];

    public async Task Poll()
    {
        if (_polling)
            return;

        _polling = true;

        try
        {
            using var response = await _httpClient.GetAsync($"getUpdates?offset={_lastUpdateId + 1}&timeout=100");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _responseLogger.Error(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

                return;
            }

            _responseLogger.Log(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

            var updates = IdentifyUpdates(JsonConvert.DeserializeObject<SuccessfulResponse<JObject[]>>(content)!.Result);

            if (updates == null)
                return;

            foreach (var update in updates)
            {
                _lastUpdateId = update.UpdateId;

                _updateLogger.Log(JsonConvert.SerializeObject(update, Formatting.Indented));

                if (update is MessageCreate messageCreate)
                    MessageCreated?.Invoke(this, messageCreate.Message);

                if (update is Contracts.Updates.MemberUpdate memberUpdate)
                    MemberUpdated?.Invoke(this, memberUpdate.MyChatMember);
            }
        }
        catch (TaskCanceledException) { }
        finally
        {
            _polling = false;
        }
    }

    public async Task<Message?> SendMessageAsync(long chatId, string text)
    {
        using var response = await _httpClient.PostAsync("sendMessage", new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "chat_id", chatId.ToString() },
            { "text", text }
        }));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _responseLogger.Error(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

            return default;
        }

        _responseLogger.Log(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

        var messageResponse = JsonConvert.DeserializeObject<SuccessfulResponse<Message>>(content);

        if (messageResponse == null)
            return default;

        return messageResponse.Result;
    }

    public async Task<Message?> SendKeyboardAsync(long chatId, string text, string[,] buttons)
    {
        var keyboardButtons = new JArray();

        for (int row = 0; row < buttons.GetLength(0); ++row)
        {
            var jsonArray = new JArray();

            for (int column = 0; column < buttons.GetLength(1); ++column)
            {
                var jsonObject = new JObject
                {
                    { "text", buttons[row, column] }
                };

                jsonArray.Add(jsonObject);
            }

            keyboardButtons.Add(jsonArray);
        }

        using var response = await _httpClient.PostAsync("sendMessage", new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "chat_id", chatId.ToString() },
            { "text", text },
            { "reply_markup", new JObject(){ {"keyboard", keyboardButtons } }.ToString() }
        }));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _responseLogger.Error(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

            return default;
        }

        _responseLogger.Log(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

        var messageResponse = JsonConvert.DeserializeObject<SuccessfulResponse<Message>>(content);

        if (messageResponse == null)
            return default;

        return messageResponse.Result;
    }

    public async Task<Message?> ReplyMessageAsync(long messageId, long chatId, string text)
    {
        using var response = await _httpClient.PostAsync("sendMessage", new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "chat_id", chatId.ToString() },
            { "text", text },
            { "reply_parameters", $"{{\"message_id\": {messageId}}}" }
        }));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _responseLogger.Error(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

            return default;
        }

        _responseLogger.Log(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

        var messageResponse = JsonConvert.DeserializeObject<SuccessfulResponse<Message>>(content);

        if (messageResponse == null)
            return default;

        return messageResponse.Result;
    }

    public async Task<bool> ReactMessageAsync(long messageId, long chatId, string reaction)
    {
        if (!AvailableReactions.Contains(reaction))
            throw new ArgumentException($"{reaction} is not available reaction", nameof(reaction));

        using var response = await _httpClient.PostAsync("setMessageReaction", new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "chat_id", chatId.ToString() },
            { "message_id", messageId.ToString() },
            { "reaction", $"[{{\"type\": \"emoji\", \"emoji\": \"{reaction}\"}}]" }
        }));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _responseLogger.Error(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

            return default;
        }

        _responseLogger.Log(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

        var messageResponse = JsonConvert.DeserializeObject<SuccessfulResponse<bool>>(content);

        if (messageResponse == null)
            return default;

        return messageResponse.Result;
    }

    public async Task<bool> DeleteMessages(long chatId, long[] messageIds)
    {
        using var response = await _httpClient.PostAsync("deleteMessages", new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "chat_id", chatId.ToString() },
            { "message_ids", JArray.FromObject(messageIds).ToString() }
        }));
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _responseLogger.Error(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

            return default;
        }

        _responseLogger.Log(JsonConvert.SerializeObject(JsonConvert.DeserializeObject(content), Formatting.Indented));

        var messageResponse = JsonConvert.DeserializeObject<SuccessfulResponse<bool>>(content);

        if (messageResponse == null)
            return default;

        return messageResponse.Result;
    }

    private IEnumerable<Update> IdentifyUpdates(JObject[] jsons) => jsons.Select(IdentifyUpdate);

    private Update IdentifyUpdate(JObject json)
    {
        if (json["message"] != null)
            return json.ToObject<MessageCreate>()!;

        if (json["my_chat_member"] != null)
            return json.ToObject<Contracts.Updates.MemberUpdate>()!;

        return json.ToObject<Update>()!;
    }
}