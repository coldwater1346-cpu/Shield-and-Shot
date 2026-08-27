using UnityEngine;

namespace Shield_Shot.GameplayCore.Field
{
    public readonly struct ElementReactionResult
    {
        public readonly ElementReactionType ReactionType;
        public readonly ElementType ResultElement;
        public readonly float DurationMultiplier;
        public readonly float SpreadRadius;
        public readonly int SpreadCellRadius;
        public readonly ElementType SpreadElement;
        public readonly float SpreadDelay;
        public readonly float SpreadDurationMultiplier;
        public readonly bool ShouldApplyStatus;

        public bool HasReaction => ReactionType != ElementReactionType.None;

        public ElementReactionResult(
            ElementReactionType reactionType,
            ElementType resultElement,
            float durationMultiplier = 1f,
            float spreadRadius = 0f,
            int spreadCellRadius = 0,
            ElementType spreadElement = ElementType.None,
            float spreadDelay = 0f,
            float spreadDurationMultiplier = 1f,
            bool shouldApplyStatus = false)
        {
            ReactionType = reactionType;
            ResultElement = resultElement;
            DurationMultiplier = durationMultiplier;
            SpreadRadius = spreadRadius;
            SpreadCellRadius = spreadCellRadius;
            SpreadElement = spreadElement;
            SpreadDelay = spreadDelay;
            SpreadDurationMultiplier = spreadDurationMultiplier;
            ShouldApplyStatus = shouldApplyStatus;
        }

        public static ElementReactionResult None =>
            new ElementReactionResult(
                ElementReactionType.None,
                ElementType.None
            );
    }

    public readonly struct ElementPaintContext
    {
        public readonly ElementType Element;
        public readonly int ElementLevel;
        public readonly float PowerMultiplier;
        public readonly Object Source;

        public ElementPaintContext(
            ElementType element,
            int elementLevel = 1,
            float powerMultiplier = 1f,
            Object source = null)
        {
            Element = element;
            ElementLevel = Mathf.Max(1, elementLevel);
            PowerMultiplier = Mathf.Max(0f, powerMultiplier);
            Source = source;
        }
    }
}
