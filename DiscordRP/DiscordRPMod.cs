﻿using System;
using UnityEngine;
using DiscordRP.Discord;
using DiscordRP.States;

namespace DiscordRP
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class DiscordRPMod : MonoBehaviour
    {
        private PresenceController presenceController;
        private StateTracker stateTracker;

        private PresenceState state;

        private float lastUpdate = 0.0F;
        private float updateInterval = 15.0F;

        private bool initialized;

        void Awake()
        {
            lastUpdate = Time.time;
            state = new IdlingState(Utils.GetEpochTime(), GameScenes.LOADING);
        }

        void Start()
        {
            presenceController = new PresenceController();
            stateTracker = new StateTracker();

            Debug.Log("DiscordRP: Plugin startup");
            presenceController.Initialize();

            DontDestroyOnLoad(this);

            GameEvents.onGamePause.Add(() =>
            {
                stateTracker.Paused = true;

                UpdatePresence(stateTracker.UpdateState());
            });

            GameEvents.onGameUnpause.Add(() =>
            {
                stateTracker.Paused = false;

                UpdatePresence(stateTracker.UpdateState());
            });
        }

        void OnDisable()
        {
            Debug.Log("DiscordRP: Plugin disable");
            presenceController.Disable();

            initialized = false;
        }

        void Update()
        {
            presenceController.UpdateCallbacks();

            stateTracker.UpdateTimers();

            float currentTime = Time.time;

            if (currentTime - lastUpdate > updateInterval || !initialized)
            {
                lastUpdate = currentTime;

                UpdatePresence(stateTracker.UpdateState());

                initialized = true;
            }
        }

        private void UpdatePresence(PresenceState state)
        {
            PresenceState previousState = this.state;

            this.state = state;

            if (!state.Equals(previousState) || !initialized)
            {
                presenceController.UpdatePresence(state);
            }
        }
    }
}
