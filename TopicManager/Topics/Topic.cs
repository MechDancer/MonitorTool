namespace TopicManager.Topics {
    public enum TopicState {
        None, Subscribed, Activate
    }

    public interface ITopic {
        byte       Dimension { get; }
        bool       FrameMode { get; }
        TopicState State     { get; set; }

        bool Add(ITopic others);
    }
}
