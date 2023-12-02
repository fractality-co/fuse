namespace Fuse
{
    /// <summary>
    /// Represents the individual phases that a <see cref="Module"/> can be in.
    /// Each step in the lifecycle is asynchronous but blocking until complete.
    /// To set a default Lifecycle, tag your attribute with <see cref="DefaultLifecycleAttribute" />.
    /// </summary>
    [Document("Module",
        "Represents the individual phases a Module can be in. " + 
        "Each step in the lifecycle is async but blocking until done." +
        "\n\nSetup - Initialization before Active" + 
        "\nActive - The module is running" +
        "\nCleanup - De-initialization after Active")]
    public enum Lifecycle
    {
        /// <summary>
        /// Either not setup yet or already cleaned up.
        /// Both represent an inactive state.
        /// </summary>
        None,
        
        /// <summary>
        /// Step for setting up before the module is set <see cref="Active"/>.
        /// </summary>
        Setup,

        /// <summary>
        /// Step where the module is active.
        /// </summary>
        Active,

        /// <summary>
        /// Step for cleaning up the module before being shutdown.
        /// </summary>
        Cleanup,
    }
}