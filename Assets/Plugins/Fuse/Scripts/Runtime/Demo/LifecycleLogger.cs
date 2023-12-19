/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

namespace Fuse
{
    /// <summary>
    /// Simple module that simply logs out lifecycle steps within the module for learning purposes.
    /// </summary>
    [Document("Demo", 
        "Example module that simply logs out lifecycle steps within the module for learning purposes.\n\n" + 
        "Assign this to any global or state scope, which will then log out each lifecycle phase.")]
    public class LifecycleLogger : Module
    {
        [Invoke(Lifecycle = Lifecycle.Setup)]
        private void Setup()
        {
            LogLifecycle(Lifecycle.Setup);
        }
 
        [Invoke(Lifecycle = Lifecycle.Active)]
        private void Active()
        {
            LogLifecycle(Lifecycle.Active);
        }

        [Invoke(Lifecycle = Lifecycle.Cleanup)]
        private void Cleanup()
        {
            LogLifecycle(Lifecycle.Cleanup);
        }

        private static void LogLifecycle(Lifecycle lifecycle)
        {
            Logger.Info("[" + nameof(LifecycleLogger) + "] entered " + lifecycle);
        }
    }
}