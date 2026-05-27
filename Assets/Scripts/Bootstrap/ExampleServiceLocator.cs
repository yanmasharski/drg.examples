using System;
using System.Collections.Generic;
using DRG.Core;

namespace FlappyExample.Bootstrap
{
	public sealed class ExampleServiceLocator : IServiceLocator
	{
		private readonly Dictionary<Type, object> _services = new();

		public void Register<T>(T service) where T : class
		{
			_services[typeof(T)] = service;
		}

		public bool TryGet<T>(out T service) where T : class
		{
			if (_services.TryGetValue(typeof(T), out var value))
			{
				service = (T)value;
				return true;
			}

			service = null;
			return false;
		}
	}
}
