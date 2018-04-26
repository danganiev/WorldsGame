namespace WorldsGame.Saving.DataClasses
{
    public enum ItemType
    {
        Usable, // Anything you can hold in hand and use in a variety of ways

        // This one is last because it can't be selected in item options
        Block, // This always does one thing only: sets block on the ground (although there will be rules to it later, like could not be placed near some blocks)
    }

    public enum ItemActionType
    {
        Nothing, // Nothing happens, default action type
        Consume, // Could only be applied to self only
        Swing, // Could be applied to blocks and creatures,
        // Throw, // This is not implemented yet
        // Shoot, // This is not implemented yet
        // Charge, // This is not implemented yet

        PlaceBlock // This is not shown in selectable options, works for blocks only
    }

    //    public enum ItemTarget
    //    {
    //        Block,
    //        Creature,
    //        Self
    //    }

    public enum ItemQuality
    {
        Consumable, // Can only be consumed or thrown once, but swinged around forever
        Unbreakable,
        // The followings are for the future
        // Durable, // Can be "consumed" or swinged in a way that detracts durability        
        // OtherEffect // Other custom effects, like transforming into another item

    }
}