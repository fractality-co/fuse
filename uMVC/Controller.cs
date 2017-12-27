using System;
using System.Collections;
using System.Linq;
using Object = UnityEngine.Object;

namespace uMVC
{
	public abstract class Controller
	{
		protected enum LoadMethod
		{
			Resources
		}

		public abstract void Setup();
		public abstract void Cleanup();

		public virtual IEnumerator Load()
		{
			yield break;
		}

		protected T GetModel<T>() where T : Model
		{
			return Object.FindObjectsOfType<T>().First();
		}

		protected T GetModel<T>(string modelName) where T : Model
		{
			return Object.FindObjectsOfType<T>().First(model => model.name == modelName);
		}

		protected T[] GetModels<T>() where T : Model
		{
			return Object.FindObjectsOfType<T>();
		}

		protected void LoadModel<T>(LoadMethod loadMethod, string path) where T : Model
		{
			throw new NotImplementedException();
		}

		protected T GetView<T>() where T : View
		{
			return Object.FindObjectsOfType<T>().First();
		}

		protected T GetView<T>(string viewName) where T : View
		{
			return Object.FindObjectsOfType<T>().First(view => view.name == viewName);
		}

		protected T[] GetViews<T>() where T : View
		{
			return Object.FindObjectsOfType<T>();
		}

		protected void LoadView<T>(LoadMethod loadMethod, string path) where T : View
		{
			throw new NotImplementedException();
		}
	}
}