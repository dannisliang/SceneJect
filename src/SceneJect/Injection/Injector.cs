﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SceneJect
{
	public class Injector
	{
		private readonly IResolver resolver;

		private readonly Type objectType;

		private readonly object objectInstance;

		public Injector(object instance, IResolver res)
		{
			objectInstance = instance;
			objectType = instance.GetType();
			resolver = res;
		}

		public void Inject()
		{
			try
			{
				foreach (FieldInfo f in objectType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ))
				{
					if (f.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0) //Means it is targeted by the attribute.
					{
						f.SetValue(objectInstance, resolver.Resolve(f.FieldType),
							BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField, null, null);
					}
				}

				foreach (PropertyInfo p in objectType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
				{
					if (p.GetCustomAttributes(typeof(InjectAttribute), false).Length > 0)
						p.SetValue(objectInstance, resolver.Resolve(p.PropertyType),
							BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, null, null);
				}
			}
			catch (Exception e)
			{
				throw new Exception("Error: " + e.Message + " failed to inject for " + objectType.ToString() + " on instance: " + objectInstance.ToString(), e);
			}
		}
	}
}
