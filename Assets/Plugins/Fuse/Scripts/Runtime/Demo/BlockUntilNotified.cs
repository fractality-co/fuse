/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

using System.Collections;

namespace Fuse
{
    /// <summary>
    /// Simple blocking logic that will hold on the Setup lifecycle phase until receiving 'block.notify' event.
    /// </summary>
    [Document("Demo", 
        "Simple blocking logic that will hold on the Setup lifecycle phase until receiving 'block.notify' event. " + 
        "Assign this to any global or state scope, which will block advancement until receiving that event.\n\n" + 
        "When you want it to advance, use a publisher either within a component, module or via the Relay with the above event.")]
    public class BlockUntilNotified : Module
    {
        public const string NotifyEvent = "block.notify";

        private bool _notified;

        [Coroutine(Lifecycle = Lifecycle.Setup)]
        private IEnumerator Setup()
        {
            _notified = false;

            while (!_notified)
                yield return null;
        }

        [Subscribe(NotifyEvent)]
        private void OnNotify()
        {
            _notified = true;
        }
    }
}