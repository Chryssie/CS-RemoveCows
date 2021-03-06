﻿using ColossalFramework;
using ColossalFramework.Plugins;
using ICities;
using System;

namespace RemoveCows
{
    public class Remover : ThreadingExtensionBase
    {
        private Settings _settings;
        private Helper _helper;

        private bool _initialized;
        private bool _terminated;

        protected bool IsOverwatched()
		{
			foreach (var plugin in PluginManager.instance.GetPluginsInfo())
			{
				if (!plugin.isEnabled)
					continue;
				foreach (var assembly in plugin.GetAssemblies())
				{
					try
					{
						var attributes = assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
						foreach (var attribute in attributes)
						{
							var guidAttribute = attribute as System.Runtime.InteropServices.GuidAttribute;
							if (guidAttribute == null)
								continue;
							if (guidAttribute.Value == "837B2D75-956A-48B4-B23E-A07D77D55847")
								return true;
						}
					}
					catch (TypeLoadException)
					{
						// This occurs for some types, not sure why, but we should be able to just ignore them.
					}
				}
			}

			return false;
		}

        public override void OnCreated(IThreading threading)
        {
            _settings = Settings.Instance;
            _helper = Helper.Instance;

            _initialized = false;
            _terminated = false;

            base.OnCreated(threading);
        }

        public override void OnBeforeSimulationTick()
        {
            if (_terminated) return;

            if (!_helper.GameLoaded)
            {
                _initialized = false;
                return;
            }

            base.OnBeforeSimulationTick();
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (_terminated) return;

            if (!_helper.GameLoaded) return;

            try
            {
                if (!_initialized)
                {
                    if (!IsOverwatched())
                    {
                        _helper.NotifyPlayer("Skylines Overwatch not found. Terminating...");
                        _terminated = true;

                        return;
					}
					else
						_helper.NotifyPlayer($"Skylines Overwatch found, initialising {this.GetType()}");

					SkylinesOverwatch.Settings.Instance.Enable.AnimalMonitor = true;

                    _initialized = true;

                    _helper.NotifyPlayer("Initialized");
                }
                else
                {
                    CitizenManager instance = Singleton<CitizenManager>.instance;

                    ushort[] cows = SkylinesOverwatch.Data.Instance.Cows;

                    foreach (ushort i in cows)
                    {
                        CitizenInstance cow = instance.m_instances.m_buffer[(int)i];

                        if (cow.Info != null)
                        {
                            cow.Info.m_maxRenderDistance = float.NegativeInfinity;

                            ((LivestockAI)cow.Info.m_citizenAI).m_randomEffect = null;
                        }

                        SkylinesOverwatch.Helper.Instance.RequestAnimalRemoval(i);
                    }
                }
            }
            catch (Exception e)
            {
                string error = String.Format("Failed to {0}\r\n", !_initialized ? "initialize" : "update");
                error += String.Format("Error: {0}\r\n", e.Message);
                error += "\r\n";
                error += "==== STACK TRACE ====\r\n";
                error += e.StackTrace;

                _helper.Log(error);

                _terminated = true;
            }

            base.OnUpdate(realTimeDelta, simulationTimeDelta);
        }

        public override void OnReleased ()
        {
            _initialized = false;
            _terminated = false;

            base.OnReleased();
        }
    }
}

