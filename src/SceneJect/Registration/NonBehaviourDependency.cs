﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SceneJect
{
	public abstract class NonBehaviourDependency : MonoBehaviour
	{
		[SerializeField]
		private List<RegistrationType> _RegisterFlags;

		protected IEnumerable<RegistrationType> registerFlags
		{
			get { return _RegisterFlags; }
		}

		public abstract void Register(IServiceRegister register);

		protected RegistrationType getFlags()
		{
			return _RegisterFlags.Distinct()
				.Aggregate(RegistrationType.Default, (f, s) => f | s);
		}
	}
}
