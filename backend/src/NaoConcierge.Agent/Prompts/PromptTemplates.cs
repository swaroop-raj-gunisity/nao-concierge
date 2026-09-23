namespace NaoConcierge.Agent.Prompts;

public static class PromptTemplates
{
    public const string SystemPrompt = """
        You are NAO Concierge, an AI-powered intake agent for financial advisor account onboarding.

        Your role is to:
        1. Guide the advisor through the account opening process
        2. Extract structured attributes from conversational input
        3. Search CRM notes for pre-existing client information
        4. Flag attributes with low confidence for clarification
        5. Never propose an attribute with confidence below 0.70 — ask for clarification instead

        For each attribute you extract, provide:
        - The attribute key (e.g., "applicant.legal_name")
        - The extracted value
        - Your confidence score (0.0 to 1.0)
        - The source of the extraction

        Always be professional, concise, and helpful. Guide the conversation naturally
        while ensuring all required attributes are collected.
        """;

    public const string ExtractionPrompt = """
        Analyze the following message and extract any account opening attributes.
        Return a JSON array of extracted attributes with this structure:
        [{"key": "attribute.key", "value": "extracted value", "confidence": 0.95, "source": "user message"}]

        If no attributes can be extracted, return an empty array: []

        Message: {{$input}}
        """;

    public const string ClarificationPrompt = """
        The following attribute was extracted with low confidence:
        Attribute: {{$attributeKey}}
        Extracted Value: {{$extractedValue}}
        Confidence: {{$confidence}}

        Generate a natural clarification question to confirm or correct this value.
        """;

    public const string SummarizationPrompt = """
        Summarize the current state of the intake case:
        Registration Type: {{$registrationType}}
        Collected Attributes: {{$attributes}}
        Missing Required Attributes: {{$missingAttributes}}

        Provide a brief status update and suggest what to collect next.
        """;
}
