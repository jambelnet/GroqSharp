using GroqSharp.Core.Models;

namespace GroqSharp.Core.Builders;

public class ChatRequestBuilder
{
    private readonly ChatRequest _request = new();

    public ChatRequestBuilder WithModel(string model)
    {
        _request.Model = model;
        return this;
    }

    public ChatRequestBuilder WithMessages(IEnumerable<Message> messages)
    {
        _request.Messages = messages.ToArray();
        return this;
    }

    public ChatRequestBuilder WithTemperature(double temperature)
    {
        _request.Temperature = temperature;
        return this;
    }

    public ChatRequestBuilder WithMaxTokens(int maxTokens)
    {
        _request.MaxTokens = maxTokens;
        return this;
    }

    public ChatRequestBuilder WithTopP(double topP)
    {
        _request.TopP = topP;
        return this;
    }

    public ChatRequestBuilder WithStream(bool stream)
    {
        _request.Stream = stream;
        return this;
    }

    public ChatRequestBuilder WithReasoningEffort(string? effort)
    {
        if (!string.IsNullOrWhiteSpace(effort))
            _request.ReasoningEffort = effort;

        return this;
    }

    public ChatRequestBuilder WithReasoningFormat(string reasoningFormat)
    {
        _request.ReasoningFormat = reasoningFormat;
        return this;
    }

    public ChatRequestBuilder WithSearchSettings(SearchSettings settings)
    {
        _request.SearchSettings = settings;
        return this;
    }

    public ChatRequest Build()
    {
        return _request;
    }
}
