

using System.Collections;
using UnityEngine;

namespace Fuse
{
	/// <summary>
	/// Simple visualizer that fades in and out a <see cref="CanvasGroup"/>.
	/// </summary>
	[Document("Loader",
		"Simple implementation of Loader that fades in/out an attached CanvasGroup." +
		"\n\nPut this on the root of a prefab with your custom display, then assign it to Configuration (home).")]
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupLoader : Loader
	{
		private const float ShowValue = 1f;
		private const float HideValue = 0f;

		[SerializeField, Range(0, 10)] private float _duration = 0.5f;
		[SerializeField, Range(0, 10)] private float _delay = 1f;

		private CanvasGroup _canvasGroup;

		private void Awake()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}

		public override IEnumerator Show()
		{
			yield return Transition(ShowValue);
			yield return new WaitForSeconds(_delay);
		}

		public override IEnumerator Hide()
		{
			yield return new WaitForSeconds(_delay);
			yield return Transition(HideValue);
		}

		private IEnumerator Transition(float to)
		{
			float initial = _canvasGroup.alpha;
			float delta = to - initial;
			float start = Time.time;

			float progress = 0f;
			while (progress < 1f)
			{
				progress = (Time.time - start) / _duration;
				_canvasGroup.alpha = initial + delta * progress;
				yield return null;
			}

			_canvasGroup.alpha = to;
		}
	}
}