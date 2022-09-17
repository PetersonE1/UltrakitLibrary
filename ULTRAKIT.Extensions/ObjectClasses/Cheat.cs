﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;
using ULTRAKILL.Cheats;


namespace ULTRAKIT.Extensions
{
	public class Cheat : ICheat
	{
		private static Cheat _lastInstance;

		private bool active;

		public Action EnableScript;
		public Action DisableScript;
		public Action UpdateScript;

		public string LongName { get; set; }

		public string Identifier { get; set; }

		public string ButtonEnabledOverride { get; set; }

		public string ButtonDisabledOverride { get; set; }

		public string Icon => "warning";

		public bool IsActive => active;

		public bool DefaultState { get; set; }

		public StatePersistenceMode PersistenceMode { get; set; }

		public void Enable()
		{
			active = true;
			_lastInstance = this;
			EnableScript();
		}

		public void Disable()
		{
			active = false;
			DisableScript();
		}

		public void Update()
		{
			UpdateScript();
		}
	}
}
