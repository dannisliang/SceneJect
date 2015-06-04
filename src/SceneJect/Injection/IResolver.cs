﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SceneJect
{
	public interface IResolver
	{
		T Resolve<T>();

		object Resolve(Type t);
	}
}
