namespace Bloomy.Models.Enums
{
    public enum AIConversationStatus
    {
        Consulting = 0,
        ReadyForConcept = 1,
        ConceptGenerated = 2,
        Completed = 3
    }

    public enum AIMessageRole
    {
        User = 0,
        Assistant = 1,
        System = 2
    }

    public enum AIMessageType
    {
        Text = 0,
        Image = 1,
        Concept = 2,
        SpaceAnalysis = 3
    }

    public enum AIUsageType
    {
        Chat = 0,
        ImageAnalysis = 1,
        ConceptGenerate = 2,
        ImageGenerate = 3
    }
}
