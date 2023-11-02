namespace DarknessUnbound.CodeAssist;

public static class Constants {
    public static class Diagnostics {
        public class DefaultResourceCount {
            public const string ID = "TML0001";
            public const string TITLE = "Redundant setting of default research count";
            public const string MESSAGE_FORMAT = TITLE;
            public const string DESCRIPTION = "The default research count is already set to 1, so setting it to 1 again is redundant.";
        }

        public class DirectionPossibleValues {
            public const string ID = "TML0002";
            public const string TITLE = "Possible values are: -1, 1";
            public const string MESSAGE_FORMAT = TITLE;
            public const string DESCRIPTION = "The direction of an entity may only be -1 or 1.";
        }

        public class ModClassShouldBePublic {
            public const string ID = "TML0003";
            
        }

        public const string ID_TYPE = "IDType";
        public const string BAD_ID_TYPE = "BadIDType";
    }

    public static class Categories {
        public const string MAINTAINABILITY = "Maintainability";
        public const string USAGE = "Usage";
    }

    public const string SYSTEM_RANDOM = "global::System.Random";
    public const string TERRARIA_ITEM = "global::Terraria.Item";
    public const string TERRARIA_ENTITY = "global::Terraria.Entity";

    public const string RESEARCH_UNLOCK_COUNT = "ResearchUnlockCount";
    public const string DIRECTION = "direction";
}
