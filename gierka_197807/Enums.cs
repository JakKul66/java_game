namespace gierka_197807
{
    public enum GameState
    {
        Menu,
        BasicGame,
        ContextGame,
        TestPhase,
        Summary
    }

    public enum DotColor
    {
        Red,
        Green,
        Blue,
        Yellow
    }

    public enum ContextItem
    {
        None,
        TrashPaper,
        TrashPlastic,
        TrashGlass,
        TrashMixed,

        Phone112,
        Phone999,
        Phone998,
        Phone997,

        PhonePizza,
        PhoneFamily,

        PillMorning,
        PillEvening,
        PillPain
    }

    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }
}